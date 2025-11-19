

// UDP - chat(multicast) / TCP
// 0) клієнт - сервер - клієнт

// 0.1)  сервер знає імена + EndPoint учасників, зберігає історія чату
// 1) mvc патерн  (виносимо асинхронні методи відправки і отримання сповіщень в контроллер, форма викликає методи контроллера)

// 2) переписати код, додати інфо хто відправляє сповіщення
// Для цього для учасника чату організовуємо ввод свого ім'я, яке потім відображається в сповіщеннях

// 3) Додаємо окремо колонку : список учасників
// Клікаємо по учаснику --> переходимо у спілкування TCP, + кнопка виходу в загальний чат   (Server)
// +Class Message (Msg , Name) + JSON

using MessangerClient.Controller;
using MessangerClient.Models;
using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;
using MessangerClient.Repositories;
using MessangerClient.Views;
using Serilog;

namespace MessangerClient
{
    public partial class MessengerUI : Form
    {
        ClientController _controller;
        ErrorLogForm _logForm;

        // Подключение к серверу, подписка на события, с подключением запускается прослушивание
        public MessengerUI(ILogger logger)
        {
            InitializeComponent();
            _controller = new ClientController(nameTextBox.Text, logger);

            _controller.ChatMessageReceived += OnChatMessageReceive;
            _controller.NetworkError += OnNetworkError;
            _controller.ContactsListUpdated += OnContactsListUpdated;

            _ = ConnectToServer();

            _logForm = new ErrorLogForm();
            _logForm.Show();
        }

        // Отправляет сообщение и записывает отображает это сообщение в Чате
        private async void SendMessageButton_Click(object sender, EventArgs e)
        {
            if (contactsListBox.SelectedItem is User recipient)
            {
                await _controller.SendMessageAsync(messageTextBox.Text, recipient);
                userChatListBox.Items.Add($"{nameTextBox.Text}: {messageTextBox.Text}");
            }
        }


        /* === Обработчики === */
        private void OnContactsListUpdated(object? sender, ContactsListUpdateEventArgs e)
        {
            UpdateContactList(e.ContactsList);
        }

        private void OnChatMessageReceive(object? sender, ChatMessageEventArgs e)
        {
            if (contactsListBox.SelectedItem is User selected)
            {
                if (e.ChatMessage.Sender.Equals(selected) ||
                    e.ChatMessage.Recipient.Equals(selected))
                {
                    userChatListBox.Items.Add(e.ChatMessage.ToString());
                }
            }
        }


        // Обработчик исключений из констроллера
        // Записывает исключения в поле чата (Что бы можно было посмотреть в любой момент,
        // потом вывод логов добавлю и будут выводиться окошечки)
        private void OnNetworkError(object? sender, EventArgs e)
        {
            if (e is ExceptionEventArgs exArgs)
            {
                _logForm.LogError(exArgs.Exception.Message);
            }

            else
                _logForm.LogError($"В обработчик {nameof(OnNetworkError)} передан неизвестный тип данных");
        }

        /* === Обработчики событий формы === */
        // Срабатывает при изменении выбранного контакта из списка контактов
        private async void contactsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (contactsListBox.SelectedItem is User messageSender)
            {
                Log.Debug($"Выбран контакт: {messageSender.Name}");

                Chat chat = await _controller.LoadChatHistoryAsync(messageSender);
                Log.Debug($"Загружено сообщений: {chat.Messages.Count}");

                UpdateChat(chat);
            }
            else
            {
                Log.Warning("SelectedItem не является User или равен null");
                ShowWarningMessageBox("SelectedItem не является User или равен null");
            }
        }

        // Срабатывает во время закрытия формы
        private void MessengerUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _controller.Dispose();
        }

        /* === Вспомогательные методы === */

        private async Task ConnectToServer()
        {
            await _controller.ConnectToServer();
        }

        private void ShowWarningMessageBox(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _logForm.LogError(message);
            Log.Warning(message);
        }


        private void UpdateChat(Chat chat)
        {
            userChatListBox.Items.Clear();
            foreach (ChatMessage message in chat.Messages)
            {
                userChatListBox.Items.Add(message.ToString());
            }
        }

        private void UpdateContactList(ContactList contactList)
        {
            contactsListBox.Items.Clear();
            foreach (User user in contactList.Users)
            {
                contactsListBox.Items.Add(user);
            }
        }
    }
}
