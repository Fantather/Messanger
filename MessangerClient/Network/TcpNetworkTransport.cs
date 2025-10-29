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
    internal class TcpNetworkTransport
    {
        private IPEndPoint _serverEndPoint;

        private IProgress<NetworkReport> _reporter;

        private TcpClient? _client;
        private NetworkStream? _stream;

        public TcpNetworkTransport(IProgress<NetworkReport> reporter)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            _serverEndPoint = new IPEndPoint(ipAddress, 57000);

            _reporter = reporter;
        }

        public async Task ConnectToServer()
        {
            if(_client != null && _client.Connected)
            {
                return;
            }

            try
            {
                _client = new TcpClient(_serverEndPoint);
                await _client.ConnectAsync(_serverEndPoint);
                _stream = _client.GetStream();
            }
            catch (SocketException ex) { _reporter.Report(new ExceptionReport(ex)); }
        }
    }
}
