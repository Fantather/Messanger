using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    internal enum ConnectionState
    {
        // Начальное состояние или соединение было разорвано
        Disconnected, 
        
        // Идет процесс подключения
        Connecting, 
        
        // Соединение успешно установлено
        Connected, 
        
        // Произошла ошибка при подключении
        Failed
    }
}
