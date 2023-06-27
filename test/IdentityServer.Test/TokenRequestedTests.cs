namespace IdentityServer.Test;

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using TestApi;
using Xunit;

public class TokenRequestedTests
{
    private const string CorrectScope = "dv_organisatieregister_testclient";
    private const string CorrectSecret = "secret";

    private const string NonExistantScope = "incorrect_scope";
    private const string WrongScope = "dv_organisatieregister_cjmbeheerder";
    private const string IncorrectSecret = "wrong secret";

    [Fact]
    public async Task NoTokenRequested_Unauthorized()
    {
        using var client = await CreateClient();

        var httpResponseMessage = await client.GetAsync("/weatherforecast");

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenRequestedWithIncorrectSecret_Unauthorized()
    {
        var token = await GetToken(IncorrectSecret, CorrectScope);
        using var client = await CreateClient(token);

        var httpResponseMessage = await client.GetAsync("/weatherforecast");

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenRequestedWithNonExistantScope_Unauthorized()
    {
        var token = await GetToken(CorrectSecret, NonExistantScope);
        using var client = await CreateClient(token);

        var httpResponseMessage = await client.GetAsync("/weatherforecast");

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenRequestedWithNonWrongScope_Unauthorized()
    {
        var token = await GetToken(CorrectSecret, WrongScope);
        using var client = await CreateClient(token);

        var httpResponseMessage = await client.GetAsync("/weatherforecast");

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TokenRequestedWithCorrectSecret_Authorized()
    {
        const string correctSecret = "secret";

        var token = await GetToken(correctSecret, CorrectScope);
        using var client = await CreateClient(token);

        var httpResponseMessage = await client.GetAsync("/weatherforecast");

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<HttpClient> CreateClient(string? token = null)
    {
        var application = new WebApplicationFactory<Program>();
        var client = application.CreateClient();

        if (string.IsNullOrWhiteSpace(token))
            return client;

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static async Task<string> GetToken(string secret, string scope)
    {
        using var identityServerclient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5050"),
        };


        var tokenClient = new TokenClient(
            () => identityServerclient,
            new TokenClientOptions
            {
                Address = "/connect/token",
                ClientId = "testClient",
                ClientSecret = secret,
                Parameters = new Parameters(
                    new[]
                    {
                        new KeyValuePair<string, string>("scope", scope),
                    }),
            });

        var acmResponse = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);
        var token = acmResponse.AccessToken;

        return token;
    }
}
