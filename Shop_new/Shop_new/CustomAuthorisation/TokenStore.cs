using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shop_new.CustomAuthorisation
{
    public class TokensStore
    {
        // Token: Name, Expiry, Last access time
        //Dictionary<string, (string, DateTime, DateTime)> tokens = new Dictionary<string, (string, DateTime, DateTime)>();
        IEnumerable<TokenInfo> tokens = new List<TokenInfo>();
        private object lck = new object();
        public TokensStore()
        {
            Task.Run(() =>
            {
                lock (lck)
                {
                    foreach (var token in tokens)
                        CheckToken(token.Token);
                }
                Thread.Sleep(60000);
            });
        }

        public CheckTokenResult CheckToken(string token)
        {
            lock (lck)
            {
                var currentToken = tokens.FirstOrDefault(t => t.Token == token);
                if (currentToken == null)
                    return CheckTokenResult.NotValid;
                if (currentToken.Expiry > DateTime.Now)
                {
                    if (DateTime.Now - currentToken.LastAccessDate < TimeSpan.FromMinutes(30))
                    {
                        currentToken.LastAccessDate = DateTime.Now;
                        return CheckTokenResult.Valid;
                    }
                }
                tokens = tokens.Where(t => t.Token != token);
                return CheckTokenResult.Expired;
            }
        }

        public string GetToken(string owner, TimeSpan expiration)
        {
            lock (lck)
            {
                var token = Guid.NewGuid().ToString();
                var expiry = DateTime.Now + expiration;
                tokens = tokens.Concat(new[]{
                    new TokenInfo
                    {
                        Token = token,
                        Expiry = expiry,
                        LastAccessDate = DateTime.Now,
                        Username = owner
                    }
                });
                return token;
            }
        }

        public string GetNameByToken(string token)
        {
            lock (lck)
            {
                if (tokens.Any(t => t.Token == token))
                    return tokens.First(t => t.Token == token).Username;
                return null;
            }
        }
    }

    public enum CheckTokenResult
    {
        Valid,
        NotValid,
        Expired
    }

    public class TokenInfo
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime LastAccessDate { get; set; }
    }
}
