using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public interface ITokenStore
    {
        CheckTokenResult CheckToken(string token);
        string GetToken(string _owner, TimeSpan expiration);
        string GetNameByToken(string token);
    }
}
