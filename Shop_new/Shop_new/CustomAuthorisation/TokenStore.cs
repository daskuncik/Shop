using Shop_new.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Shop_new.CustomAuthorisation
{
    public class TokenStore
    {
        // Token: Name, Expiry, Last access time
        //Dictionary<string, (string, DateTime, DateTime)> tokens = new Dictionary<string, (string, DateTime, DateTime)>();
        TokenDbContext db = new TokenDbContext();
        public CheckTokenResult CheckToken(string token)
        {
            var tok = db.Tokens.FirstOrDefault(q => q.token == token);
            if (tok == null)
                return CheckTokenResult.NotValid;
            //if (!tokens.Keys.Contains(token))
            // return CheckTokenResult.NotValid;

            //if (tokens[token].Item2 > DateTime.Now)
            if (tok.Item1 > DateTime.Now)
            {
                //if (DateTime.Now - tokens[token].Item3 < TimeSpan.FromMinutes(30))
                if (DateTime.Now - tok.Item2 < TimeSpan.FromMinutes(30))
                {
                    //var prev = tokens[token];
                    var prev = tok;
                    //tokens.Remove(token);
                    db.Tokens.Remove(tok);
                    db.SaveChanges();
                    //tokens.Add(token, (prev.Item1, prev.Item2, DateTime.Now));
                    tok.Item2 = DateTime.Now;
                    db.Tokens.Add(tok);
                    db.SaveChanges();
                    return CheckTokenResult.Valid;
                }
            }
            //tokens.Remove(token);
            db.Tokens.Remove(tok);
            db.SaveChanges();
            return CheckTokenResult.Expired;
        }

        public string GetToken(string _owner, TimeSpan expiration)
        {
            var _token = Guid.NewGuid().ToString();
            var expiry = DateTime.Now + expiration;
            var tok = new Token
            {
                token = _token,
                owner = _owner,
                Item1 = expiry,
                Item2 = DateTime.Now
            };
            db.Tokens.Add(tok);
            //tokens.Add(_token, (owner, expiry, DateTime.Now));
            return _token;
        }

        public string GetNameByToken(string token)
        {
            var tok = db.Tokens.FirstOrDefault(q => q.token == token);
            if (tok == null)
                return null;
            return tok.owner;
           // if (tokens.Keys.Contains(token))
               // return tokens[token].Item1;
            //return null;
        }
    }

    public enum CheckTokenResult
    {
        Valid,
        NotValid,
        Expired
    }
}
