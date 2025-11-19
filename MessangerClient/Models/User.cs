using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    /// <summary>
    /// Класс представляющий пользователя
    /// </summary>
    internal class User
    {
        //public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }

        public User()
        {
            Name = string.Empty;
        }

        public User(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator !=(User a, User b)
        {
            return !(a == b);
        }

        public static bool operator ==(User a, User b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is User other)
                return string.Equals(Name, other.Name, StringComparison.Ordinal);

            return false;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
