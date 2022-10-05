namespace IdentityServer.Config;

using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

public class JsonClient
{
    public string ClientId { get; set; } = null!;
    public List<string> ClientSecrets { get; set; } = new();
    public string AllowedGrantTypes { get; set; } = null!;
    public List<string> AllowedScopes { get; set; } = new();
    public bool? AlwaysSendClientClaims { get; set; }
    public bool? AlwaysIncludeUserClaimsInIdToken { get; set; }
    public int? AccessTokenLifetime { get; set; }
    public int? IdentityTokenLifetime { get; set; }
    public string ClientClaimsPrefix { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public string FrontChannelLogoutUri { get; set; }
    public List<JsonClaim> Claims { get; set; }

    public static Client Export(JsonClient jsonClient)
    {
        var client = new Client
        {
            ClientId = jsonClient.ClientId,
            ClientSecrets = jsonClient.ClientSecrets
                .Select(
                    secret =>
                        new Secret(secret.Sha256()))
                .ToList(),
            AllowedGrantTypes = GetAllowedGrantTypes(jsonClient.AllowedGrantTypes),
            AllowedScopes = GetAllowedScopes(jsonClient.AllowedScopes),
            RedirectUris = jsonClient.RedirectUris,
            PostLogoutRedirectUris = jsonClient.PostLogoutRedirectUris,
            FrontChannelLogoutUri = jsonClient.FrontChannelLogoutUri,
        };

        client.SetAccessTokenLifetimeOrDefault(jsonClient.AccessTokenLifetime);
        client.SetIdentityTokenLifetimeOrDefault(jsonClient.IdentityTokenLifetime);
        client.SetClientClaimsPrefixOrDefault(jsonClient.ClientClaimsPrefix);
        client.SetAlwaysSendClientClaimsOrDefault(jsonClient.AlwaysSendClientClaims);
        client.SetAlwaysIncludeUserClaimsInIdTokenOrDefault(jsonClient.AlwaysIncludeUserClaimsInIdToken);

        return client;
    }

    private static ICollection<string> GetAllowedScopes(List<string> allowedScopes)
    {
        Dictionary<string, string> predefinedScopes =
            new()
            {
                { "standardscopes.openid", IdentityServerConstants.StandardScopes.OpenId },
                { "standardscopes.profile", IdentityServerConstants.StandardScopes.Profile },
            };

        return allowedScopes
            .Select(scope => predefinedScopes.ContainsKey(scope.ToLower()) ? predefinedScopes[scope.ToLower()] : scope)
            .ToList();
    }

    private static ICollection<string> GetAllowedGrantTypes(string allowedGrantTypes)
        => allowedGrantTypes.ToLowerInvariant() switch
        {
            "clientcredentials" => GrantTypes.ClientCredentials,
            "code" => GrantTypes.Code,
            _ => throw new NotSupportedException(),
        };
}

public class JsonUser
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public List<JsonClaim> Claims { get; set; }
    public string SubjectId { get; set; }

    public static TestUser Export(JsonUser jsonUser)
    {
        return new TestUser
        {
            Username = jsonUser.Username,
            Password = jsonUser.Password,
            IsActive = jsonUser.IsActive,
            SubjectId = jsonUser.SubjectId,
            Claims = jsonUser.Claims.Select(c => new Claim(c.Type, c.Value)).ToList()
        };
    }
}

public class JsonClaim
{
    public string Type { get; set; }
    public string Value { get; set; }
}