using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Events
{
    internal class ContactsListUpdateEventArgs : EventArgs
    {
        public ContactList ContactsList { get; }
        public ContactsListUpdateEventArgs(User newUser) => ContactsList = new ContactList(newUser);
        public ContactsListUpdateEventArgs(UserListUpdateDataPacket userListUpdateDataPacket) => ContactsList = userListUpdateDataPacket.ContactList;
        public ContactsListUpdateEventArgs(ContactList updateList) => ContactsList = updateList;
    }
}
