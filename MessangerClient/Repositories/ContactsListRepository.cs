using MessangerClient.Models;
using User = MessangerClient.Models.User;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Serilog;

namespace MessangerClient.Repositories
{
    internal class ContactsListRepository
    {
        string FilePath { get; set; }
        string FolderPath { get; set; }
        public ContactsListRepository(string folderPath = "Contacts")
        {
            FolderPath = folderPath;
            FilePath = Path.Combine(folderPath, "Contacts.json");

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (!File.Exists(FilePath))
            {
                string newContacts = JsonSerializer.Serialize(new ContactList());
                File.WriteAllText(FilePath, newContacts);
            }
        }

        // Сохранить список контактов в файл
        public async Task SaveAsync(ContactList contacts)
        {
            await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(contacts));
        }

        // Сохранить список контактов из файла
        public async Task<ContactList> LoadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Log.Warning($"Файл контактов не существует: {FilePath}");
                    return new ContactList();
                }

                string jsonContacts = await File.ReadAllTextAsync(FilePath);
                var contacts = JsonSerializer.Deserialize<ContactList>(jsonContacts);

                Log.Debug($"Загружено контактов: {contacts?.Users?.Count ?? 0}");

                return contacts ?? new ContactList();
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при загрузке контактов: {ex}");
                return new ContactList();
            }
        }

        public async Task AddContact(User user)
        {
            ContactList contacts = await LoadAsync();
            contacts.AddContact(user);
            await SaveAsync(contacts);
        }
    }
}
