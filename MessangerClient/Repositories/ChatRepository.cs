using MessangerClient.Models;
using MessangerClient.Models.DTO;
using MessangerClient.Models.FileModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangerClient.Repositories
{
    // Класс для сохранения и считывания чатов из файлов 
    internal class ChatRepository
    {
        public string FolderPath { get; set; }
        ILogger _logger;
        
        public ChatRepository(ILogger logger, string folderPath = "Chats")
        {
            FolderPath = folderPath;
            _logger = logger;

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        // Сохраняет переданный объект чата в файл
        public async Task SaveAsync(Chat chat)
        {
            try
            {
                string fileName = $"{chat.Companion.Name}.json";
                string fullPath = Path.Combine(FolderPath, fileName);

                // Конвертируем в модель для файла
                var chatFileModel = new ChatFileModel
                {
                    Companion = new UserFileModel { Name = chat.Companion.Name },
                    Messages = chat.Messages.Select(m => new ChatMessageFileModel
                    {
                        Sender = new UserFileModel { Name = m.Sender.Name },
                        Recipient = new UserFileModel { Name = m.Recipient.Name },
                        Message = m.Message,
                        Timestamp = DateTime.Now
                    }).ToList()
                };

                string chatString = JsonSerializer.Serialize(chatFileModel, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(fullPath, chatString);
                _logger.Debug($"Чат успешно сохранен с {chat.Messages.Count} сообщениями");
            }
            catch (Exception ex)
            {
                _logger.Error("Исключение при сохранении чата в файл: {ex}", ex);
            }
        }

        // Сохраняет все чаты, не пригодилось
        public async Task SaveAllAsync(IEnumerable<Chat> chats)
        {
            List<Task> savingChats = new List<Task>();
            foreach (Chat chat in chats)
            {
                savingChats.Add(SaveAsync(chat));
            }

            await Task.WhenAll(savingChats);
        }

        public async Task SaveMessageToChat(ChatMessage message, User me)
        {
            User companion = message.Sender.Equals(me) ? message.Recipient : message.Sender;

            Chat? chat = await LoadAsync(companion);
            if (chat == null)
                chat = new Chat(companion);

            chat.AddMessage(message);
            await SaveAsync(chat);
        }

        // Загружает чат по переданному пути
        public async Task<Chat?> LoadAsync(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    _logger.Warning($"Файл не существует: {filepath}");
                    return null;
                }

                string chatJson = await File.ReadAllTextAsync(filepath);
                _logger.Debug($"Загружен JSON из {filepath}");

                var chatFileModel = JsonSerializer.Deserialize<ChatFileModel>(chatJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (chatFileModel == null)
                {
                    _logger.Warning($"Не удалось десериализовать файл: {filepath}");
                    return null;
                }

                // Конвертируем обратно в доменную модель
                var companion = new User(chatFileModel.Companion.Name);
                var chat = new Chat(companion);

                foreach (var messageModel in chatFileModel.Messages)
                {
                    var message = new ChatMessage(
                        new User(messageModel.Sender.Name),
                        new User(messageModel.Recipient.Name),
                        messageModel.Message
                    );
                    chat.AddMessage(message);
                }

                _logger.Debug($"Десериализовано сообщений: {chat.Messages.Count}");
                return chat;
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при загрузке чата из файла {filepath}: {ex}");
                return null;
            }
        }

        // Ищет путь к файлу, который принадлежит определённому пользователю
        public async Task<Chat?> LoadAsync(User user)
        {
            string? filepath = SearchChatByUserName(user);
            if (filepath == null)
            {
                _logger.Warning($"Файл чата для пользователя {user.Name} не найден");
                return null;
            }

            return await LoadAsync(filepath);
        }

        // Думал загружать все чаты, но это не понадобилось
        public async Task<Chat[]> LoadAllChats()
        {
            List<Task<Chat?>> loadingChats = new List<Task<Chat?>>();
            foreach (string filename in Directory.EnumerateFiles(FolderPath))
            {
                loadingChats.Add(LoadAsync(filename));
            }

            Chat?[] loadedChats = await Task.WhenAll(loadingChats);
            return loadedChats.Where(chat => chat != null).ToArray();
        }


        /* === Вспомогательные методы === */
        private string? SearchChatByUserName(User user)
        {
            _logger.Debug($"Поиск чата для пользователя: {user.Name}");

            foreach (string pathfile in Directory.EnumerateFiles(FolderPath, "*.json"))
            {
                var fileName = Path.GetFileNameWithoutExtension(pathfile);
                _logger.Debug($"Проверка файла: {fileName}");

                if (string.Equals(fileName, user.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Debug($"Файл найден: {pathfile}");
                    return pathfile;
                }
            }

            _logger.Debug($"Файл для пользователя {user.Name} не найден");
            return null;
        }
    }
}
