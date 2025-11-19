using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.FileModel
{
    // Создайте отдельные классы для сериализации в файлы
    public class ChatFileModel
    {
        public List<ChatMessageFileModel> Messages { get; set; } = new();
        public UserFileModel Companion { get; set; } = new();
    }
}
