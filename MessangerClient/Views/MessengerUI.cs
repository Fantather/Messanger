

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
using MessangerClient.Models.DTO;
using MessangerClient.Models.Events;

namespace MessangerClient
{
    public partial class MessengerUI : Form
    {
        ClientController _controller;
        public MessengerUI()
        {
            InitializeComponent();
            _controller = new ClientController(nameTextBox.Text);
            _controller.ChatMessageReceived += OnChatMessageReceive;
            _controller.NetworkError += OnNetworkError;
            _ = ConnectToServer();
        }

        private async void SendMessageButton_Click(object sender, EventArgs e)
        {
            await _controller.SendMessage(nameTextBox.Text, messageTextBox.Text);
            userChatListBox.Items.Add($"{nameTextBox}: {messageTextBox.Text}");
        }

        private void OnChatMessageReceive(object? sender, EventArgs e)
        {
            //if (e is ChatMessageEventArgs chat)
            //    userChatListBox.Items.Add(chat.ChatMessage.ToString());

            if (e is ChatMessageEventArgs chat)
            {
                ChatMessage message = chat.ChatMessage;
                string displayText = $"{message.Name}: {message.Message}";

                // Потокобезопасное обновление UI
                if (userChatListBox.InvokeRequired)
                {
                    userChatListBox.Invoke(() =>
                    {
                        userChatListBox.Items.Add(displayText);
                    });
                }
                else
                {
                    userChatListBox.Items.Add(displayText);
                }
            }
            else
                ShowWarningMessageBox($"В обработчик {nameof(OnChatMessageReceive)} передан неизвестный тип данных");
        }

        private void OnNetworkError(object? sender, EventArgs e)
        {
            if (e is ExceptionEventArgs exArgs)
            {
                Exception ex = exArgs.Exception;
                userChatListBox.Items.Add(ex);
            }

            else
                ShowWarningMessageBox($"В обработчик {nameof(OnNetworkError)} передан неизвестный тип данных");
        }



        /* === Вспомогательные методы === */

        private async Task ConnectToServer()
        {
            await _controller.ConnectToServer();
            _ = _controller.StartReceiveMessagesAsync();
        }

        private void ShowWarningMessageBox(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
