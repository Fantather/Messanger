using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using MessangerClient.Models.Reports;
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
    internal class TcpNetworkTransport : IDisposable
    {
        private IPEndPoint _serverEndPoint;

        private TcpClient? _client;
        private NetworkStream? _stream;

        private SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource? _receiveCts;

        private const int LengthPrefixSize = 4;

        // public event EventHandler<NetworkMessageBytesEventArgs>? MessageReceived;    // Это событие уже не нужно
        public event EventHandler<ExceptionEventArgs>? ErrorOccured;
        public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

        public TcpNetworkTransport()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            _serverEndPoint = new IPEndPoint(ipAddress, 57000);
        }

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

                InvokeConnectionStateChanged(true);
            }
            catch (ObjectDisposedException) { return false; }
            catch (Exception ex)
            {
                InvokeErrorOccured(ex);
                Dispose();
                return false;
            }

            return true;
        }

        public async Task<bool> SendMessageAsync(byte[] messageBytes, CancellationToken cancellationToken = default)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (!IsConnected())
                {
                    InvokeErrorOccured(new InvalidOperationException("Клиент не готов для отправки сообщений"));
                    return false;
                }

                byte[] messageLengthBytes = BitConverter.GetBytes(messageBytes.Length);
                await _stream.WriteAsync(messageLengthBytes, 0, LengthPrefixSize, cancellationToken);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                InvokeErrorOccured(ex);
                return false;
            }
        }

        public async IAsyncEnumerable<byte[]> ReadMessagesAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested && _client.Connected)
            {
                byte[] receivedData;
                try
                {
                    byte[] messageLengthBytes = new byte[LengthPrefixSize];

                    if (!IsConnected())
                        break;

                    messageLengthBytes = await ReadExactlyAsync(LengthPrefixSize, ct);
                    int messageLength = BitConverter.ToInt32(messageLengthBytes, 0);

                    receivedData = await ReadExactlyAsync(messageLength, ct);
                }
                catch (ObjectDisposedException) { yield break; }
                catch (OperationCanceledException ex)
                {
                    InvokeErrorOccured(ex);
                    yield break;
                }
                catch (Exception ex)
                {
                    InvokeErrorOccured(ex);
                    Dispose();
                    yield break;
                }

                yield return receivedData;
            }
        }

        public void Dispose()
        {
            _receiveCts?.Cancel();
            _receiveCts?.Dispose();
            _receiveCts = null;

            _stream?.Dispose();
            _stream = null;

            _client?.Dispose();
            _client = null;

            InvokeConnectionStateChanged(false);
        }


        /* === Вспомогательные методы === */
        private bool IsConnected() => _client?.Connected == true && _stream != null;

        private void InvokeErrorOccured(Exception ex)
        {
            ErrorOccured?.Invoke(this, new ExceptionEventArgs(ex));
        }

        private void InvokeConnectionStateChanged(bool state)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(state));
        }

        //private void InvokeMessageReceived(byte[] message)
        //{
        //    MessageReceived?.Invoke(this, new NetworkMessageBytesEventArgs(message));
        //}

        private async Task<byte[]> ReadExactlyAsync(int messageLength, CancellationToken ct)
        {
            byte[] buffer = new byte[messageLength];
            int offset = 0;
            
            while(offset < messageLength)
            {
                int bytesRead = await _stream.ReadAsync(buffer, offset, messageLength - offset, ct);

                if(bytesRead == 0)
                    throw new EndOfStreamException("Соединение разорвано до получения всех данных");

                offset += bytesRead;
            }

            return buffer;
        }


    }
}
