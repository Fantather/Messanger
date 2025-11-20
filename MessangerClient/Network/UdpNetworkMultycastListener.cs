using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using MessangerClient.Network.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace MessangerClient.Network
{
    internal class UdpNetworkMultycastListener : IDisposable
    {
        private readonly IPAddress _multycastIp;
        private readonly int _port;

        private readonly UdpClient _udpClient;
        private CancellationTokenSource? _multycastCts;

        public event EventHandler<DataPackageBytesEventArgs>? MessageReceived;
        public event EventHandler<ExceptionEventArgs>? ErrorOccured;

        public UdpNetworkMultycastListener(IPAddress multycastIp, int port)
        {
            _multycastIp = multycastIp;
            _port = port;
            var localEndPoint = new IPEndPoint(IPAddress.Any, port);

            _udpClient = new UdpClient();
            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            try
            {
                _udpClient.Client.Bind(localEndPoint);
            }
            catch (SocketException ex)
            {
                Log.Error($"[UDP] Не удалось занять порт {port}. Возможно, приложение уже запущено? Ошибка: {ex.Message}");
                throw;
            }
        }

        public void StartLisnteningMultycast()
        {
            _multycastCts = new CancellationTokenSource();
            _ = ListenLoopAsync(_multycastCts.Token);
        }

        public void StopListeningMultycast()
        {
            _multycastCts?.Cancel();
            Log.Debug("[UDP] Прослушивание остановлено");
        }

        public void Dispose()
        {
            _multycastCts?.Cancel();
            _multycastCts?.Dispose();

            _udpClient?.Dispose();
        }


        /* === Вспомогательные методы === */
        private async Task ListenLoopAsync(CancellationToken ct)
        {
            try
            {
                _udpClient.JoinMulticastGroup(_multycastIp);

                Log.Debug($"[UDP] Присоединились к multicast группе {_multycastIp}:{_port}");

                while (!ct.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(ct);
                    Log.Debug($"[UDP] Получено {result.Buffer.Length} байт от {result.RemoteEndPoint}");
                    MessageReceived?.Invoke(this, new DataPackageBytesEventArgs(result.Buffer));
                }
            }
            catch (OperationCanceledException)
            {
                Log.Error("[UDP] Операция отменена");
            }
            catch (ObjectDisposedException)
            {
                Log.Error("[UDP] Объект disposed");
            }
            catch (Exception ex)
            {
                Log.Error($"[UDP] Ошибка: {ex.Message}");
                ErrorOccured?.Invoke(this, new ExceptionEventArgs(ex));
            }
            finally
            {
                try
                {
                    _udpClient.DropMulticastGroup(_multycastIp);
                    Log.Debug("[UDP] Покинули multicast группу");
                }
                catch (Exception ex)
                {
                    Log.Error($"[UDP] Ошибка при выходе из группы: {ex.Message}");
                }
            }
        }
    }
}