using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Класс для доставки текстовых сообщений между клиентами и сервером
namespace MessangerClient.Models.Interop
{
    internal class Message
    {
        public string Name { get; set; }
        public string Msg { get; set; }
        
        public Message(string name, string msg)
        {
            Name = name;
            Msg = msg;
        }
    }
}
