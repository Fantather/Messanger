using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models
{
    internal class ContactList
    {
        public List<User> Users { get; set; }

        public ContactList() => Users = new List<User>();
        public ContactList(User user) => Users = new List<User> { user };
        public ContactList(IEnumerable<User> users) => Users = users.ToList();

        public void AddContact(User user)
        {
            Users.Add(user);
        }
    }
}
