using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangerClient.Network.Serializers
{
    /// <summary>
    /// Сериализует или десериализует DataPacket и его наследников.
    /// PingRequest нет
    /// </summary>
    internal static class DataPacketSerializer
    {
        public static byte[] Serialize(DataPacket data)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }

        public static DataPacket Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<DataPacket>(data) ?? throw new JsonException("Не удалось десериализовать байты с сервера в NetworkUpdate");
        }
    }
}
