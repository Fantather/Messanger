using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.FileModel
{
    public class ChatMessageFileModel
    {
        public UserFileModel Sender { get; set; } = new();
        public UserFileModel Recipient { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
