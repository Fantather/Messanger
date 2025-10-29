using MessangerClient.Models;
using MessangerClient.Models.Events;
using MessangerClient.Models.Reports;
using MessangerClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Message = MessangerClient.Models.Interop.Message;


// Убедится что приём сообщений дописан, WaitMessageAsync
// Возможно PingRequestMessage принимать будет не нужно, скорее нужно будет создать PingResponseMessage
// А возможно метод PingServerAsync мне даже не понадобится
// Работу с Message потом допилю

// Добавить в UdpListener защиту от многократного создания


namespace MessangerClient.Controller
{
    internal class ClientController
    {
        public string ClientName { get; set; }
        public IPAddress MulticastIP { get; }
        public int MulticastPort { get; }
        public bool IsMultiacastListening { get; private set; }

        private UdpNetworkMultycastListener? _listener;
        private TcpNetworkTransport? _tcpTransport;

        private IProgress<NetworkReport> _reporter;
        
        public event EventHandler<ChatMessageEventArgs>? ChatMessageReceived;
        public event EventHandler<ExceptionEventArgs>? NetworkError;

        public ClientController(string name, string multicastIP, int multicastPort)
        {
            ClientName = name;
            MulticastIP = IPAddress.Parse(multicastIP);
            MulticastPort = multicastPort;
            _reporter = new Progress<NetworkReport>(HandleNetworkReport);
        }

        /* === UDP === */
        public void StartListenMultycast(string MultycastIpAdress, int port)
        {
            IPAddress multicastIP = IPAddress.Parse(MultycastIpAdress);
            _listener = new UdpNetworkMultycastListener(multicastIP, port);
            _listener.StartLisnteningMultycast(_reporter);
        }


        /* === TCP === */
        public void ConnectToServer()
        {
            _tcpTransport = new TcpNetworkTransport(_reporter);
        }
        



        /* === Вспомогательные методы === */

        private void HandleNetworkReport(NetworkReport report)
        {
            switch (report)
            {
                case MessageReport messageReport:
                    HandleNetworkMessage(messageReport.Message);
                    break;

                case ExceptionReport exceptionReport:
                    InvokeNetworkError(new(exceptionReport.Exception));
                    break;
            }
        }


        private void HandleNetworkMessage(NetworkMessage result)
        {
            switch (result)
            {
                case ChatMessage chatMsg:
                    ChatMessageReceived?.Invoke(this, new ChatMessageEventArgs(chatMsg));
                    break;
            }
        }

        private void InvokeNetworkError(ExceptionEventArgs ex)
        {
            NetworkError?.Invoke(this, ex);
        }


        // Скорее всего не нужен
        //private async Task PingServerAsync()
        //{
        //    await _udpClient.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PingRequest()));
        //}




        /* === Методы работающие с Message, потом допишу === */

        //private async Task WaitMessageAsync()
        //{
        //    while (IsMultiacastListening)
        //    {
        //        UdpReceiveResult udpReceive = await _udpClient.ReceiveAsync();
        //        _ = MessageProcessingAsync(udpReceive);
        //    }
        //}

        //private async Task MessageProcessingAsync(UdpReceiveResult result)
        //{
        //    await Task.Run(() =>
        //    {
        //        NetworkUpdate update = _messageFactory.CreateMessageFromJson(result.Buffer);

        //    });
        //}
    }
}
