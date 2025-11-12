using MessangerClient.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangerClient.Models.Reports
{
    internal class UserListUpdateReport : NetworkReport
    {
        public UserListUpdateMessage Users { get; set; }
        public UserListUpdateReport(UserListUpdateMessage users) => Users = users;
    }
}
