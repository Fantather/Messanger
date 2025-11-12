using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using MessangerClient.Models.Reports;
using MessangerClient.Network.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Network
{
    internal class UdpNetworkMultycastListener : IDisposable
    {
        private readonly IPAddress _multycastIp;
        private readonly int _port;

        private readonly UdpClient _udpClient;
        private CancellationTokenSource? _multycastCts;

        public event EventHandler<NetworkMessageBytesEventArgs>? MessageReceived;
        public event EventHandler<ExceptionEventArgs>? ErrorOccured;

        public UdpNetworkMultycastListener(IPAddress multycastIp, int port)
        {
            _multycastIp = multycastIp;
            _port = port;
            _udpClient = new UdpClient(port);

            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void StartLisnteningMultycast()
        {
            _udpClient.JoinMulticastGroup(_multycastIp);
            _multycastCts = new CancellationTokenSource();
            _ = ListenLoopAsync(_multycastCts.Token);
        }

        public void StopListeningMultycast()
        {
            _multycastCts?.Cancel();
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
                while (true)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(ct);
                    MessageReceived?.Invoke(this, new NetworkMessageBytesEventArgs(result.Buffer));
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { ErrorOccured?.Invoke(this, new ExceptionEventArgs(ex)); }
            finally { _udpClient.DropMulticastGroup(_multycastIp); }
        }
    }
}
