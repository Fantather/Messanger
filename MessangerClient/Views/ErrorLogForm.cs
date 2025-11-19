using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessangerClient.Views
{
    // Сюда выводились исключения
    // Хотел ещё логи из файла выводить, но файлов логов может быть несколько и я решил не возиться с этим
    public partial class ErrorLogForm : Form
    {
        public ErrorLogForm()
        {
            InitializeComponent();
        }

        // Метод для добавления ошибки
        public void LogError(string message, Exception? ex = null)
        {
            // Если вызов идет не из UI-потока, перенаправляем его через Invoke
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => LogError(message, ex)));
                return;
            }

            // Формируем текст ошибки
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            string fullMessage = $"[{timeStamp}] {message}";

            if (ex != null)
            {
                fullMessage += $"\nDetails: {ex.Message}";
            }
            fullMessage += "\n---------------------------------\n";

            // Добавляем текст в конец
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;

            // Красим слово "ERROR" или весь текст в красный
            rtbLog.SelectionColor = Color.Red;
            rtbLog.AppendText(fullMessage);

            // Сбрасываем цвет для следующего текста
            rtbLog.SelectionColor = rtbLog.ForeColor;

            // Автопрокрутка вниз
            rtbLog.ScrollToCaret();
        }


        // Метод для обычных сообщений (не исключений)
        public void LogInfo(string message)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => LogInfo(message)));
                return;
            }

            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            rtbLog.AppendText($"[{timeStamp}] INFO: {message}\n");
        }


        // Чтобы окно не уничтожалось при закрытии пользователем, а просто скрывалось
        // Иначе попытка обращения к форме вызовет исключение
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnFormClosing(e);
        }
    }
}
