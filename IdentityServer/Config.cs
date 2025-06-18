using Duende.IdentityServer.Models;
using System.Collections.Generic;

public static class Config
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("xyz_api", "XYZ University API")
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "xyz_client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("xyz_secret".Sha256()) },
                AllowedScopes = { "xyz_api" }
            }
        };
}
