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


            _ = Task.Run(() => ReceiveMessagesAsync());
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

        public async Task SendMessageAsync(byte[] messageByte)
        {
            if(!IsClientOrStreamNotReady())
                return;

            await _stream.WriteAsync(messageByte, 0, messageByte.Length);
        }

        public async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while (_client.Connected)
                {
                    bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, receivedData, bytesRead);
                        NetworkMessage message = NetworkMessageSerializer.Deserialize(receivedData);
                        _reporter.Report(new MessageReport(message));
                    }
                    else if (bytesRead == 0)
                    {
                        _reporter.Report(new ConnectionReport(false));
                        break;
                    }
                }
            }
            catch(IOException ex) { _reporter.Report(new ExceptionReport(ex)); }
            catch(ObjectDisposedException ex) { _reporter.Report(new ExceptionReport(ex)); }
            catch(Exception ex) { _reporter.Report(new ExceptionReport(ex));}
        }
        

        /* === Вспомогательные методы === */
        public bool IsClientOrStreamNotReady()
        {
            if(_client is null)
            {
                _reporter.Report(new ExceptionReport(new NullReferenceException("Попытка обращения к переменной клиента, которая является null")));
                return false;
            }

            if (!_client.Connected)
            {
                _reporter.Report(new ExceptionReport(new SocketException((int)SocketError.NotConnected)));
                return false;
            }

            if (_client is null)
            {
                _reporter.Report(new ExceptionReport(new NullReferenceException("Попытка обращения к переменной Потока Данных, которая является null")));
                return false;
            }

            return true;
        }
    }
}
