using HealthForms.Api.Clients;
using HealthForms.Api.Shared;
using IdentityModel.Client;
using System.Net;
using System.Net.Http.Json;
#pragma warning disable CS8604 // Possible null reference argument.

namespace HealthForms.Api.Tests.Shared;

public class TokenCacheTests
{
    private async Task<TokenResponse?> GetTokenResponse(string token, int i = 10)
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                access_token = token,
                expires_in = i
            })
        };
        return await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(httpResponseMessage);
    }

    [Fact]
    public async Task SetNewToken()
    {
        var key = "key";
        var token = new AuthResponse(await GetTokenResponse("accessToken1"));
        TokenCache.Set(key, token);

        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token, cachedToken);
    }

    [Fact]
    public async Task SetNewToken_Replace()
    {
        var key = "key";
        var token = new AuthResponse(await GetTokenResponse("accessToken1"));
        TokenCache.Set(key, token);

        var token2 = new AuthResponse(await GetTokenResponse("accessToken2"));
        TokenCache.Set(key, token2);
        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token2, cachedToken);
    }

    [Fact]
    public async Task SetNewToken_ReplaceExpiredToken()
    {
        var key = "key";
        var token = new AuthResponse(await GetTokenResponse("accessToken1"));
        TokenCache.Set(key, token);
        await Task.Delay(1500);

        var token2 = new AuthResponse(await GetTokenResponse("accessToken2"));
        TokenCache.Set(key, token2);
        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token2, cachedToken);
    }

    [Fact]
    public async Task TryGetToken_ExpiredToken()
    {
        var key = "key";
        var token = new AuthResponse(await GetTokenResponse("accessToken1", 2));
        TokenCache.Set(key, token);
        await Task.Delay(1500);

        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.False(result);
        Assert.Null(cachedToken);
    }
}