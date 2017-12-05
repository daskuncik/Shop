using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface IOAuthService
    {
        Task<Tuple<string, string>> GetAccessAndRefreshTokens(string username, string password);
        Task<Tuple<string, string>> GetAccessAndRefreshTokens(string refreshToken);
    }
}
