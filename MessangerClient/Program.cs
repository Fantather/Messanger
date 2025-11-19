using Serilog;

namespace MessangerClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Логгер Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    "Logs/log-.txt",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true
                    )
                .Enrich.WithProperty("Application name", "Messanger client")
                .CreateLogger();

            // Глобальные обработчики ошибок
            Application.ThreadException += OnGuiException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ApplicationConfiguration.Initialize();
            Application.Run(new MessengerUI(Log.Logger));
        }

        // Отлов неотловленных исключений из UI-потока
        // Записывает исключение в лог и выводит сообщение об исключении для пользователя
        private static void OnGuiException(object? sender, ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Поймана необработанная ошибка UI");

            MessageBox.Show("Произошла непредвиденная ошибка. Приложение попытается продолжить работу\n"+ "Ошибка была записана в лог", 
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Отлов не отловленных исключений, возникших в фоновом потоке, которые сейчас положат мне программу
        // Пытается успеть записать исключение в логи
        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                Log.Fatal(ex, "Пойман фатальный сбой (IsTerminating: {IsTerminating})", 
                    e.IsTerminating);
            }
            else
            {
                Log.Fatal("В обработчик был передан не Exception: {ExceptionObject} (IsTerminating: {IsTerminating})", 
                    e.ExceptionObject, e.IsTerminating);
            }

            Log.CloseAndFlush();
        }
    }
}