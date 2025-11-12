using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MessangerClient.Models.DTO
{
    [JsonDerivedType(typeof(ChatMessage), typeDiscriminator: "ChatMessage")]
    internal abstract class NetworkMessage
    {
    }
}
