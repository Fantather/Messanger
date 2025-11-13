

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
using Serilog;

namespace MessangerClient
{
    public partial class MessengerUI : Form
    {
        ClientController _controller;

        // Подключение к серверу, подписка на события, с подключением запускается прослушивание
        public MessengerUI(ILogger logger)
        {
            InitializeComponent();
            _controller = new ClientController(nameTextBox.Text, logger);
            _controller.ChatMessageReceived += OnChatMessageReceive;
            _controller.NetworkError += OnNetworkError;
            _ = ConnectToServer();
        }

        // Отправляет сообщение и записывает отображает это сообщение в Чате
        private async void SendMessageButton_Click(object sender, EventArgs e)
        {
            await _controller.SendMessageAsync(messageTextBox.Text);
            userChatListBox.Items.Add($"{nameTextBox}: {messageTextBox.Text}");
        }

        
        private void OnChatMessageReceive(object? sender, EventArgs e)
        {
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
                    userChatListBox.Items.Add(displayText + "111111111111111");
                }
            }
            else
                ShowWarningMessageBox($"В обработчик {nameof(OnChatMessageReceive)} передан неизвестный тип данных");
        }

        // Обработчик исключений из констроллера
        // Записывает исключения в поле чата (Что бы можно было посмотреть в любой момент,
        // потом вывод логов добавлю и будут выводиться окошечки)
        private void OnNetworkError(object? sender, EventArgs e)
        {
            if (e is ExceptionEventArgs exArgs)
            {
                Exception ex = exArgs.Exception;
                if (userChatListBox.InvokeRequired)
                {
                    userChatListBox.Invoke(() =>
                    {
                        userChatListBox.Items.Add(ex.Message);
                    });
                }
                else
                {
                    userChatListBox.Items.Add(ex.Message + "111111111111111");
                }
            }

            else
                ShowWarningMessageBox($"В обработчик {nameof(OnNetworkError)} передан неизвестный тип данных");
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
        }
    }
}
