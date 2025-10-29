using MessangerClient.Models;
using MessangerClient.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Network
{
    internal class UdpNetworkMultycastListener
    {
        private readonly IPAddress _multycastIp;
        private readonly int _port;

        private readonly UdpClient _udpClient;
        private CancellationTokenSource? _multycastCts;

        public UdpNetworkMultycastListener(IPAddress multycastIp, int port)
        {
            _multycastIp = multycastIp;
            _port = port;
            _udpClient = new UdpClient(port);

            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void StartLisnteningMultycast(IProgress<NetworkReport> reporter)
        {
            _udpClient.JoinMulticastGroup(_multycastIp);
            _multycastCts = new CancellationTokenSource();
            _ = ListenLoopAsync(_multycastCts.Token, reporter);
        }

        public void StopListeningMultycast()
        {
            _multycastCts?.Cancel();
        }


        /* === Вспомогательные методы === */
        private async Task ListenLoopAsync(CancellationToken ct, IProgress<NetworkReport> reporter)
        {
            try
            {
                while (true)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(ct);
                    NetworkMessage message = NetworkMessageSerializer.Deserialize(result.Buffer);
                    reporter.Report(new MessageReport(message));
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { reporter.Report(new ExceptionReport(ex)); }
            finally { _udpClient.DropMulticastGroup(_multycastIp); }
        }
    }
}
