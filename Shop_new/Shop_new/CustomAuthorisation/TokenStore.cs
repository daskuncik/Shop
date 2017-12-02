using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_new.CustomAuthorisation
{
    public class TokenStore
    {
        // Token: Name, Expiry, Last access time
        Dictionary<string, (string, DateTime, DateTime)> tokens = new Dictionary<string, (string, DateTime, DateTime)>();

        public CheckTokenResult CheckToken(string token)
        {
            if (!tokens.Keys.Contains(token))
                return CheckTokenResult.NotValid;
            if (tokens[token].Item2 > DateTime.Now)
            {
                if (DateTime.Now - tokens[token].Item3 < TimeSpan.FromMinutes(30))
                {
                    var prev = tokens[token];
                    tokens.Remove(token);
                    tokens.Add(token, (prev.Item1, prev.Item2, DateTime.Now));
                    return CheckTokenResult.Valid;
                }
            }
            tokens.Remove(token);
            return CheckTokenResult.Expired;
        }

        public string GetToken(string owner, TimeSpan expiration)
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.Now + expiration;
            tokens.Add(token, (owner, expiry, DateTime.Now));
            return token;
        }

        public string GetNameByToken(string token)
        {
            if (tokens.Keys.Contains(token))
                return tokens[token].Item1;
            return null;
        }
    }

    public enum CheckTokenResult
    {
        Valid,
        NotValid,
        Expired
    }
}
