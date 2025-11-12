using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using MessangerClient.Models.Reports;
using MessangerClient.Network;
using MessangerClient.Network.Serializers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Message = MessangerClient.Models.Interop.Message;
using Serilog.Formatting.Display;
using System.Diagnostics.Eventing.Reader;


// Работу с Message потом допилю

// Добавить в UdpListener защиту от многократного создания

// В ConnectionReport изменить bool на ConnectionState

// Дописать сохранение в Логи

namespace MessangerClient.Controller
{
    internal class ClientController : IDisposable
    {
        public string ClientName { get; set; }
        public bool IsMultiacastListening { get; private set; }
        public bool IsServerConnected { get; private set; }

        private UdpNetworkMultycastListener? _udpListener;
        private TcpNetworkTransport? _tcpTransport;

        private ILogger _logger;
        
        public event EventHandler<ChatMessageEventArgs>? ChatMessageReceived;
        public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;
        public event EventHandler<ExceptionEventArgs>? NetworkError;
        public event EventHandler<UserListUpdateEventArgs>? UserListUpdated;

        public ClientController(string name)
        {
            ClientName = name;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File
                    (
                    "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp: yyyy-mm-dd HH:mm:ss.fff} [{level:u3}] {Message:lj} {NewLine}{Exception}"
                    )
                .CreateLogger();
        }

        /* === UDP === */
        public void StartListenMultycast(string MultycastIpAdress, int port)
        {
            IPAddress multicastIP = IPAddress.Parse(MultycastIpAdress);
            _udpListener = new UdpNetworkMultycastListener(multicastIP, port);
            _udpListener.StartLisnteningMultycast();

            _udpListener.ErrorOccured += OnExceptionOccured;
            _udpListener.MessageReceived += OnUdpMessageReceived;
        }


        /* === TCP === */
        public async Task ConnectToServer()
        {
            _tcpTransport = new TcpNetworkTransport();
            await _tcpTransport.ConnectToServerAsync();

            _tcpTransport.ErrorOccured += OnExceptionOccured;
            _tcpTransport.ConnectionStateChanged += OnConnectionStateChanged;

            _logger.Debug("Соединение с сервером установлено");
        }

        public async Task SendMessage(string name, string message)
        {
            if (IsTcpTransportNotExist())
                return;

            ChatMessage chatMessage = new ChatMessage(name, message);
            byte[] messageByte = NetworkMessageSerializer.Serialize(chatMessage);
            await _tcpTransport.SendMessageAsync(messageByte);
        }

        public async Task StartReceiveMessagesAsync()
        {
            if (IsTcpTransportNotExist())
                return;

            try
            {
                await foreach (byte[] messageBytes in _tcpTransport.ReadMessagesAsync())
                {
                    NetworkMessage message = NetworkMessageSerializer.Deserialize(messageBytes);
                    HandleNetworkMessage(message);
                }
            }
            catch (OperationCanceledException ex)
            {
                InvokeNetworkError(ex);
            }
            catch (Exception ex)
            {
                InvokeNetworkError(ex);
            }
        }

        public void Dispose()
        {

            if(_tcpTransport != null)
            {
                _tcpTransport.ErrorOccured -= OnExceptionOccured;
                _tcpTransport.ConnectionStateChanged -= OnConnectionStateChanged;
                _tcpTransport.Dispose();
            }

            if(_udpListener != null)
            {
                _udpListener.ErrorOccured -= OnExceptionOccured;
                _udpListener.MessageReceived -= OnUdpMessageReceived;
                _udpListener.Dispose();
            }
        }



        /* === Обработчики === */
        private void OnExceptionOccured(object? sender, ExceptionEventArgs e)
        {
            InvokeNetworkError(e.Exception);
        }

        private void OnConnectionStateChanged(object? sender, ConnectionStateEventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, e);
        }

        private void OnUdpMessageReceived(object? sender, NetworkMessageBytesEventArgs e)
        {
            NetworkMessage message = NetworkMessageSerializer.Deserialize(e.Message);
            HandleNetworkMessage(message);
        }

        /* === Вспомогательные методы === */

        private void HandleNetworkMessage(NetworkMessage result)
        {
            switch (result)
            {
                case ChatMessage chatMsg:
                    InvokeChatMessageReceived(chatMsg);
                    break;

                case UserListUpdateMessage listUpdate:
                    InvokeUserListUpdated(listUpdate);
                    break;

                default:
                    InvokeNetworkError(new ArgumentException($"Клиент не умеет работать с типом сообщений {result.GetType()}"));
                    break;
            }
        }

        private bool IsTcpTransportNotExist()
        {
            if(_tcpTransport is null)
            {
                InvokeNetworkError(new NullReferenceException($"Попытка обращения клиента к не созданному {nameof(_tcpTransport)}"));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InvokeNetworkError(Exception ex)
        {
            NetworkError?.Invoke(this, new ExceptionEventArgs(ex));
        }

        private void InvokeChatMessageReceived(ChatMessage message)
        {
            ChatMessageReceived?.Invoke(this, new ChatMessageEventArgs(message));
        }

        private void InvokeUserListUpdated(UserListUpdateMessage listUpdate)
        {
            UserListUpdated?.Invoke(this, new UserListUpdateEventArgs(listUpdate));
        }

        // Скорее всего не нужен
        //private async Task PingServerAsync()
        //{
        //    await _udpClient.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new PingRequest()));
        //}


        /* === Уже не нужный код === */
        // private IProgress<NetworkReport> _reporter;

        //private void HandleNetworkReport(NetworkReport report)
        //{
        //    switch (report)
        //    {
        //        case MessageBytesReport messageBytesReport:
        //            NetworkMessage reportMessage = NetworkMessageSerializer.Deserialize(messageBytesReport.Message);
        //            HandleNetworkMessage(reportMessage);
        //            break;

        //        case ExceptionReport exceptionReport:
        //            InvokeNetworkError(exceptionReport.Exception);
        //            break;

        //        case UserListUpdateReport userListUpdateReport:
        //            UserListUpdated?.Invoke(this, new UserListUpdateEventArgs(userListUpdateReport.Users));
        //            break;

        //        default:
        //            InvokeNetworkError(new ArgumentException($"Клиент не умеет работать с типом {report.GetType()}"));
        //            break;

        //            //case MessageReport messageReport:                   // Больше не использую этот тип
        //            //    HandleNetworkMessage(messageReport.Message);
        //            //    break;
        //    }
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
