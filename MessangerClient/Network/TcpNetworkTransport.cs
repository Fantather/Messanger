using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace MessangerClient.Network
{
    /// <summary>
    /// Управляет TCP-соединением с сервером.
    /// Отвечает за асинхронную отправку и получение данных, используя протокол с 4-байтным префиксом длинны передаваемых данных.
    /// </summary>
    /// <remarks>
    /// Этот класс обеспечивает потокобезопасную отправку данных с помощью <see cref="SemaphoreSlim"/>
    /// Он так же отслеживает состояние соединения и передаёт данные наружу через события
    /// </remarks>
    internal class TcpNetworkTransport : IDisposable
    {
        private IPEndPoint _serverEndPoint;

        private TcpClient? _client;
        private NetworkStream? _stream;

        private ILogger _logger;

        /// <summary>
        /// Семафор для обеспечения потокобезопасности при одновременных вызовах <see cref="SendMessageAsync"/>
        /// Гарантирует, что только один метод выполняет запись в поток одновременно
        /// </summary>
        private SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Токен отмены для остановки фонового цикла получения данных <see cref="ReceiveLoopAsync"/>
        /// 
        /// </summary>
        private CancellationTokenSource? _receiveCts;

        /// <summary>
        /// Константа, определяющая размер префикса, который указывает на длинну основного сообщения (В байтах)
        /// </summary>
        private const int LengthPrefixSize = 4;


        /* === События === */
        /// <summary>
        /// Событие, срабатывающее при получении полного пакета данных.
        /// </summary>
        public event EventHandler<DataPackageBytesEventArgs>? DataPacketReceived;

        /// <summary>
        /// Событие, срабатывающее при возникновении исключения во время работы транспорта (подключение, отправка, получение).
        /// </summary>
        public event EventHandler<ExceptionEventArgs>? ErrorOccured;

        /// <summary>
        /// Событие, уведомляющее об изменении состояния подключения (подключен/отключен).
        /// </summary>
        public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;


        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TcpNetworkTransport"/>.
        /// Настраивает конечную точку сервера на localhost:57000.
        /// </summary>
        /// 
        /// <remarks>
        /// Подумал, что Сервер обычно, имеет публичную и неизменную точку входа, поэтому решил не запрашивать EndPoint у пользователя
        /// </remarks>
        public TcpNetworkTransport(ILogger logger)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            _serverEndPoint = new IPEndPoint(ipAddress, 57000);
            _logger = logger;
        }

        /// <summary>Асинхронно устанавливает соединение с сервером и запускает асинхронный цикл получения данных.</summary>
        /// 
        /// <returns>
        /// <see cref="Task{TResult}"/>, представляющий асинхронную операцию.
        /// Результат <c>true</c>, если соединение было успешно установлено (или уже существовало, до вызова метода),
        /// Или <c>false</c> в случае ошибки
        /// </returns>
        /// 
        /// <remarks>
        /// При успехе этот метод инициализирует <see cref="NetworkStream"/> и <see cref="CancellationTokenSource"/>
        /// для работы цикла получения данных <see cref="ReceiveLoopAsync"/>
        /// </remarks>
        public async Task<bool> ConnectToServerAsync()
        {
            if (_client?.Connected == true)
            {
                return true;
            }

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_serverEndPoint);
                _stream = _client.GetStream();

                _receiveCts = new CancellationTokenSource();

                // Говорим не возвращаться в UI поток, после завершения метода
                // То есть, весь оставшийся код будет тоже выполнен в ThreadPool
                _ = ReceiveLoopAsync(_receiveCts.Token).ConfigureAwait(false);

                InvokeConnectionStateChanged(true);
            }

            // При попытке обращения к освобождённым объектам
            catch (ObjectDisposedException) { return false; }
            catch (Exception ex)
            {
                InvokeErrorOccured(ex);
                Dispose();  // Освобожение ресурсов, при неудачном создании соединения
                return false;
            }

            return true;
        }

        /// <summary> Асинхронно отправляет пакет данных, добавляя к нему префикс длинны.</summary>
        /// <param name="packetBytes">Массив байтов, для отправки</param>
        /// <param name="ct">Токен для отмены операции отправки</param>
        /// 
        /// <returns>
        /// <see cref="Task{TResult}"/> с результатом <c>true</c>, если данные были успешно отправлены
        /// или с <c>false</c>, если возникло исключение.
        /// </returns>
        /// 
        /// <remarks>
        /// Метод является потокобезопасным. Он ожидает <see cref="_sendLock"/> перед отправкой.
        /// При возникновении исключения, будет вызвано событие <see cref="ErrorOccured"/>.
        /// </remarks>
        public async Task<bool> SendMessageAsync(byte[] packetBytes, CancellationToken ct = default)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (!IsConnected())
                {
                    InvokeErrorOccured(new InvalidOperationException("Клиент не готов для отправки сообщений"));
                    return false;
                }
                // Отправляем 4-байтный префикс с длиной сообщения
                byte[] messageLengthBytes = BitConverter.GetBytes(packetBytes.Length);
                await _stream.WriteAsync(messageLengthBytes, 0, LengthPrefixSize, ct);

                // Отправляем само сообщение
                await _stream.WriteAsync(packetBytes, 0, packetBytes.Length, ct);

                return true;
            }
            catch (Exception ex)
            {
                InvokeErrorOccured(ex);
                return false;
            }
            finally
            {
                _sendLock.Release();
            }
        }


        /// <summary>
        /// Приватный цикл, который непрерывно слушает входящие данные от сервера.
        /// </summary>
        /// 
        /// <param name="ct">Токен отмены, для остановки цикла</param>
        /// 
        /// <remarks>
        /// При успешном получении данных вызывается событие <see cref="DataPacketReceived"/>,
        /// в аргументах которого хранятся данные в виде byte[]
        /// Цикл работает до тех пор, пока не будет запрошена отмена или пока <c>_client.Connected</c>
        /// не станет <c>false</c>.
        /// 
        /// В конце работы вызывает событие <see cref="ConnectionStateChanged"/>, которое хранит <c>false</c>.
        /// Это сигнализирует о закрытии соединения с сервером.
        /// </remarks>
        /// 
        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            if (!IsConnected())
                return;

            try
            {
                while (!ct.IsCancellationRequested && _client.Connected)
                {
                    // Считываем длинну сообщения
                    byte[] messageLengthBytes = await ReadExactlyAsync(LengthPrefixSize, ct).ConfigureAwait(false);
                    int messageLength = BitConverter.ToInt32(messageLengthBytes);
                    _logger.Verbose("Получена длинна сообщения: {messageLength}", messageLength);

                    // Считываем само сообщение
                    byte[] receivedData = await ReadExactlyAsync(messageLength, ct);
                    _logger.Verbose("Получено сообщение в байтах");

                    InvokeDataPacketRecieved(receivedData);
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.Warning("Объект был уничтожен", ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Debug("Прослушивание сервера было отменено", ex);
            }
            catch (EndOfStreamException ex)
            {
                _logger.Debug("Сервер прервал соединение", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            finally
            {
                InvokeConnectionStateChanged(false);
            }
        }

        /// <summary>
        /// Освобождает все ресурсы (TcpClient, NetworkStream, CancellationTokenSource), 
        /// используемые <see cref="TcpNetworkTransport"/>.
        /// </summary>
        public void Dispose()
        {
            _receiveCts?.Cancel();
            _receiveCts?.Dispose();
            _receiveCts = null;

            _stream?.Dispose();
            _stream = null;

            _client?.Dispose();
            _client = null;
        }


        /* === Вспомогательные методы === */
        /// <summary>
        /// Проверяет, что <see cref="TcpClient"/> существует, подключен к серверу и его поток (<see cref="NetworkStream"/>) доступен.
        /// </summary>
        private bool IsConnected() => _client?.Connected == true && _stream != null;

        /// <summary>
        /// Безопасно вызывает событие <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="ex">Объект исключения, который будет передан наружу</param>
        private void InvokeErrorOccured(Exception ex)
        {
            ErrorOccured?.Invoke(this, new ExceptionEventArgs(ex));
        }

        /// <summary>
        /// Безопасно вызывает событие <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="state">Состояние соединения с сервером, <c>true</c> соединение установлено, <c>false</c> соединение разорвано</param>
        private void InvokeConnectionStateChanged(bool state)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(state));
        }

        /// <summary>
        /// Безопасно вызывает событие <see cref="DataPacketReceived"/>
        /// </summary>
        /// <param name="receivedData"></param>
        private void InvokeDataPacketRecieved(byte[] receivedData)
        {
            DataPacketReceived?.Invoke(this, new DataPackageBytesEventArgs(receivedData));
        }

        /// <summary>
        /// Асинхронно ожидает прихода сообщения и читает из потока ровно <paramref name="messageLength"/> байт.
        /// Занимается фреймингом
        /// </summary>
        /// 
        /// <param name="messageLength">Количество байт, которое необходимо прочесть</param>
        /// <param name="ct">Токен для отмены ожидания сообщения с сервера</param>
        /// 
        /// <returns><see cref="Task{byte[]}"/>Массив байтов запрошенной длинны</returns>
        /// 
        /// <exception cref="EndOfStreamException">
        /// Выбрасывается, если соединение было разорвано (<c>ReadAsync</c> вернул 0) 
        /// до того, как удалось прочитать <paramref name="bytesToRead"/> байт.
        /// </exception>
        private async Task<byte[]> ReadExactlyAsync(int messageLength, CancellationToken ct)
        {
            byte[] buffer = new byte[messageLength];
            int offset = 0;
            
            while(offset < messageLength)
            {
                // Засыпает, до прихода данных
                int bytesRead = await _stream.ReadAsync(buffer, offset, messageLength - offset, ct).ConfigureAwait(false);

                if(bytesRead == 0)
                    throw new EndOfStreamException("Соединение разорвано до получения всех данных");

                offset += bytesRead;
            }

            return buffer;
        }
    }
}
