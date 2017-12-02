using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Linq;

namespace AuthServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>();
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "API")
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients() =>
            // client credentials client
            new List<Client>
            {
                new Client
                {
                    ClientId = "test-client",
                    ClientName = "Test client",
                    AllowedGrantTypes = GrantTypes.Code.Concat(GrantTypes.ResourceOwnerPassword).ToList(),
                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess = true,
                    RequireConsent = false,
                    RedirectUris = { "https://localhost:63939/" },
                    EnableLocalLogin = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "api"
                    },
                    ClientSecrets = new List<Secret>() { new Secret("test-secret".Sha256()) }
                }
            };

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "User1",
                    Password = "pass1"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "User2",
                    Password = "pass2"
                }
            };
        }
    }
}
