using MessangerClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangerClient.Network
{
    internal static class NetworkMessageSerializer
    {
        public static byte[] Serialize(NetworkMessage data)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }

        public static NetworkMessage Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<NetworkMessage>(data) ?? throw new JsonException("Не удалось десериализовать байты с сервера в NetworkUpdate");
        }
    }
}
