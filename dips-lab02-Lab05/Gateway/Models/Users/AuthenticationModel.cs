using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models.Users
{
    public class AuthenticationModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Redirect { get; set; }
    }
}
