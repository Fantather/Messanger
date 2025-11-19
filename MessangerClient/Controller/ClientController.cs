using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
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

using System.Diagnostics;
using MessangerClient.Models;
using MessangerClient.Repositories;

/* === Нужно сделать === */
// Работу с Message потом допилю
// Добавить в UdpListener защиту от многократного создания
// Дописать сохранение в Логи
// Дописать метод, для завершения прослушивания, без уничтожения всего объекта, хотя возможно в моём случае это излишне
// Дописать работу с UserListUpdated
// Добавить CTS и добавить его в Dispose и ReceiveLoopAsync

// Добавить имя или id получателя в пакеты данных для сервера
// и что бы не нарушать конфиденциальность, у меня будет приходить сначала длинна имени, потом имя
// потом длинна сообщения и само сообщение, опять переписывать, ыаааа

namespace MessangerClient.Controller
{
    /// <summary>
    /// Контроллер для управление клиентской стороной мессенджера
    /// </summary>
    internal class ClientController : IDisposable
    {
        public User Client { get; set; }

        public bool IsMultiacastListening { get; private set; }
        public bool IsServerConnected { get; private set; }

        private UdpNetworkMultycastListener? _udpListener;
        private TcpNetworkTransport? _tcpTransport;

        ChatRepository _chatRepository;
        ContactsListRepository _contactListRepository;

        

        private ILogger _logger;


        /* === События === */
        // Все события тут созданы для общения с UI-потоком

        /// <summary>
        /// Вызывается при получении сообщения для Чата.
        /// </summary>
        /// <remarks>
        /// Будет вызвано из <see cref="ThreadPool"/>, а не из UI-потока.
        /// </remarks>
        public event EventHandler<ChatMessageEventArgs>? ChatMessageReceived;

        /// <summary>
        /// Вызывается при изменении состояния соединения (подключен/отключен).
        /// </summary>
        public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// Срабатывает при отлове исключения
        /// </summary>
        /// <remarks>
        /// Может быть вызвано из <see cref="ThreadPool"/>, а не из UI-потока
        /// </remarks>
        public event EventHandler<ExceptionEventArgs>? NetworkError;

        /// <summary>
        /// Вызывается при получении обновлений в списке чатов
        /// </summary>
        /// <remarks>
        /// Будет вызвано из <see cref="ThreadPool"/>, а не из UI-потока
        /// </remarks>
        public event EventHandler<ContactsListUpdateEventArgs>? ContactsListUpdated;



        /// <summary>
        /// Инициализирует объект <see cref="ClientController"/>
        /// и логгер
        /// </summary>
        /// <param name="name">
        /// Имя пользателя, которым будут подписаны сообщения.
        /// Так как у меня нет системы входа, сервер будет по имени определять, это одинаковые пользователи или нет.
        /// Я добавил в User поле Id, так что может сделаю через него
        /// </param>
        /// <remarks>
        /// Tcp-соединение с сервером или запуск UdpMultycast в отдельных методах
        /// 
        /// Принимает логгер снаружи, а не создаёт его, решил не давать возможности его не передавать, 
        /// что бы во все вызовы логгера не пришлось добавлять "?"
        /// </remarks>
        public ClientController(string name, ILogger logger)
        {
            Client = new User(name);
            _logger = logger;

            _chatRepository = new ChatRepository(logger);
            _contactListRepository = new ContactsListRepository();
        }


        /* === UDP === */
        /// <summary>
        /// Запускает Multycast-прослушиваение
        /// </summary>
        /// <param name="MultycastIpAdress">IPAdress мультикаст-группы</param>
        /// <param name="port">Прослушиваемый port, на который будут приходить мультикаст-сообщения</param>
        /// <remarks>
        /// Запускает прослушиваение Multycast группы, 
        /// подписывается на события у <see cref="_udpListener"/>, 
        /// изменяет состояние <see cref="IsMultiacastListening"/>
        /// </remarks>
        public void StartListenMultycast(string MultycastIpAdress, int port)
        {
            IPAddress multicastIP = IPAddress.Parse(MultycastIpAdress);
            _udpListener = new UdpNetworkMultycastListener(multicastIP, port);
            _udpListener.StartLisnteningMultycast();
            IsMultiacastListening = true;

            _udpListener.ErrorOccured += OnExceptionOccured;
            _udpListener.MessageReceived += OnUdpMessageReceived;
        }


        /* === TCP === */
        /// <summary>
        /// Запускает попытку соединения с сервером
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="Task{bool}"/>
        /// Результат <c>true</c>, если соединение было успешно установлено (или уже существовало, до вызова метода)
        /// или <c>false</c> в случае ошибки
        /// </returns>
        /// 
        /// <remarks>
        /// Подписывается на события <see cref="_tcpTransport"/>.
        /// Делает запись в логгер
        /// Изменяет состояние <see cref="IsServerConnected"/> на <c>true</c>
        /// </remarks>
        public async Task<bool> ConnectToServer()
        {
            try
            {
                _tcpTransport = new TcpNetworkTransport(_logger);
                bool connectionState = await _tcpTransport.ConnectToServerAsync();

                _tcpTransport.ErrorOccured += OnExceptionOccured;
                _tcpTransport.ConnectionStateChanged += OnConnectionStateChanged;
                _tcpTransport.DataPacketReceived += OnDataPacketReceived;

                IsServerConnected = true;
                _logger.Debug("Соединение с сервером установлено");

                await LoadContactList();

                return connectionState;
            }
            catch (Exception ex)
            {
                InvokeNetworkError(ex);
                return false;
            }
        }


        /// <summary>
        /// Асинхронно отправляет сообщение на сервер
        /// </summary>
        /// 
        /// <param name="message">Сообщение, которе будет отправлено</param>
        /// 
        /// <returns>
        /// <see cref="Task{TResult}"/> с результатом <c>true</c>, если данные были успешно отправлены
        /// или с <c>false</c>, если возникло исключение.
        /// </returns>
        /// 
        /// <remarks>
        /// Имя пользоватебя извлекается из свойства <see cref="Client"/>
        /// </remarks>
        public async Task<bool> SendMessageAsync(string message, User recipient)
        {
            if (IsTcpTransportNotExist())
                return false;

            ChatMessage chatMessage = new ChatMessage(Client, recipient, message);
            byte[] messageByte = DataPacketSerializer.Serialize(chatMessage);
            await _tcpTransport.SendMessageAsync(messageByte);

            await _chatRepository.SaveMessageToChat(chatMessage, Client);

            return true;
        }


        public async Task<Chat> LoadChatHistoryAsync(User companion)
        {
            try
            {
                _logger.Debug($"Загрузка истории чата для пользователя: {companion.Name}");

                Chat? chat = await _chatRepository.LoadAsync(companion);

                if (chat == null)
                {
                    _logger.Debug($"Чат для {companion.Name} не найден, создается новый");
                    chat = new Chat(companion);
                }
                else
                {
                    _logger.Debug($"Загружен чат с {chat.Messages.Count} сообщениями");
                }

                return chat;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при загрузке истории чата: {ex}");
                return new Chat(companion);
            }
        }


        /// <summary>
        /// Освобождает все ресурсы (_tcpTransport, _udpListener, events)
        /// используемые <see cref="ClientController"/>.
        /// </summary>
        public void Dispose()
        {

            if(_tcpTransport != null)
            {
                _tcpTransport.ErrorOccured -= OnExceptionOccured;
                _tcpTransport.ConnectionStateChanged -= OnConnectionStateChanged;
                _tcpTransport.DataPacketReceived -= OnDataPacketReceived;
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

            IsServerConnected = e.State;
        }

        /// <summary>
        /// Обработчик получения сообщения от транспортной части
        /// </summary>
        /// <param name="sender">Ссылка на класс, вызвавший событие</param>
        /// <param name="e">Класс-обёртка содержащий в себе обёртку, которая хранит в себе полученные от сервера байты</param>
        private async void OnDataPacketReceived(object? sender, DataPackageBytesEventArgs e)
        {
            try
            {
                // Десериализуем
                DataPacket data = DataPacketSerializer.Deserialize(e.PacketBytes);
                await HandleDataPacket(data);
            }
            catch (Exception ex)
            {
                InvokeNetworkError(ex);
            }
        }

        //UDP
        private async void OnUdpMessageReceived(object? sender, DataPackageBytesEventArgs e)
        {
            DataPacket message = DataPacketSerializer.Deserialize(e.PacketBytes);
            await HandleDataPacket(message);
        }

 

        /* === Вспомогательные методы === */

        /// <summary>
        /// Распаковывеает <see cref="DataPacket"/> в конкретный тип-наследник и в зависимости от него, решает что делать
        /// </summary>
        /// 
        /// <param name="result">Полученные от сервера данные</param>
        /// <exception cref="ArgumentException">Вызывается, если этот класс наследник не поддерживается</exception>
        private async Task HandleDataPacket(DataPacket result)
        {
            switch (result)
            {
                case ChatMessage chatMsg:
                    InvokeChatMessageReceived(chatMsg);

                    ContactList contactList = await _contactListRepository.LoadAsync();
                    if(!contactList.Users.Contains(chatMsg.Sender))
                    {
                        contactList.AddContact(chatMsg.Sender);
                        InvokeUserListUpdatedAsync(contactList);
                    }
                    

                    // Сохраняем новое сообщение в файл
                    await _chatRepository.SaveMessageToChat(chatMsg, Client);

                    _logger.Verbose("Сообщение в байтах сохранено и отправлено в UI {chatMsg}", chatMsg);
                    break;

                case UserListUpdateDataPacket listUpdate:
                    InvokeUserListUpdatedAsync(listUpdate.ContactList);
                    break;

                default:
                    InvokeNetworkError(new ArgumentException($"Клиент не умеет работать с типом сообщений {result.GetType()}"));
                    break;
            }
        }

        private async Task LoadContactList()
        {
            ContactList contactList = await _contactListRepository.LoadAsync();
            ContactsListUpdated?.Invoke(this, new ContactsListUpdateEventArgs(contactList));
        }

        


        /// <summary>
        /// Проверяет объект <see cref="TcpNetworkTransport"/> на null
        /// </summary>
        /// 
        /// <returns>Возвращает <c>true</c>, если объект <see cref="TcpNetworkTransport"/> оказывается <c>null</c></returns>
        /// 
        /// <remarks>
        /// Вызывает метод <see cref="InvokeNetworkError"/>, который сигнализирует об исключении.
        /// Логгирует исключение.
        /// </remarks>
        private bool IsTcpTransportNotExist()
        {
            if(_tcpTransport is null)
            {
                Exception ex = new NullReferenceException($"Попытка обращения клиента к не созданному {nameof(_tcpTransport)}");
                InvokeNetworkError(ex);
                _logger.Warning(ex.Message, ex);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InvokeNetworkError(Exception ex)
        {
            _logger.Error(ex.Message, ex);
            NetworkError?.Invoke(this, new ExceptionEventArgs(ex));
        }

        private void InvokeChatMessageReceived(ChatMessage message)
        {
            ChatMessageReceived?.Invoke(this, new ChatMessageEventArgs(message));
        }

        private async void InvokeUserListUpdatedAsync(ContactList listUpdate)
        {
            ContactsListUpdated?.Invoke(this, new ContactsListUpdateEventArgs(listUpdate));
            await _contactListRepository.SaveAsync(listUpdate);
        }
    }
}
