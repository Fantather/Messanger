using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    /// <summary>
    /// По задумке, отправляется в UdpMultycast, что бы сигнализировать о том что мы слушаем 
    /// и нам нужно отправить список сообщений, которые мы не получили.
    /// Передаётся по сети.
    /// </summary>
    internal class PingRequestDataPacket : DataPacket
    {
    }
}
