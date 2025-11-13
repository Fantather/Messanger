using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    /// <summary>
    /// Абстрактный класс, от которого наследуются все классы, передающиеся по сети.
    /// Передаётся по сети.
    /// JSON умеет различать наследников.
    /// </summary>
    [JsonDerivedType(typeof(ChatMessage), typeDiscriminator: "ChatMessage")]
    [JsonDerivedType(typeof(UserListUpdateDataPacket), typeDiscriminator: "UserListUpdateMessage")]
    internal abstract class DataPacket
    {
    }
}
