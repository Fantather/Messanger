using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Interop.Reports
{
    internal class UserListUpdateReport : NetworkReport
    {
        public UserListUpdateDataPacket Users { get; set; }
        public UserListUpdateReport(UserListUpdateDataPacket users) => Users = users;
    }
}
