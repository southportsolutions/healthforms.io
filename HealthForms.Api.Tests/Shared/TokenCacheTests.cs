using HealthForms.Api.Clients;
using HealthForms.Api.Shared;
using IdentityModel.Client;

namespace HealthForms.Api.Tests.Shared;

public class TokenCacheTests
{
    [Fact]
    public void SetNewToken()
    {
        var key = "key";
        var token = new AuthResponse(new TokenResponse()) { ExpiresOn = DateTime.Now.AddMinutes(1) };
        TokenCache.Set(key, token);

        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token, cachedToken);
    }

    [Fact]
    public void SetNewToken_Replace()
    {
        var key = "key";
        var token = new AuthResponse(new TokenResponse()){ExpiresOn = DateTime.Now.AddMinutes(1)};
        TokenCache.Set(key, token);

        var token2 = new AuthResponse(new TokenResponse());
        TokenCache.Set(key, token2);
        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token2, cachedToken);
    }

    [Fact]
    public async Task SetNewToken_ReplaceExpiredToken()
    {
        var key = "key";
        var token = new AuthResponse(new TokenResponse()){ExpiresOn = DateTime.Now.AddSeconds(1)};
        TokenCache.Set(key, token);
        await Task.Delay(1500);

        var token2 = new AuthResponse(new TokenResponse());
        TokenCache.Set(key, token2);
        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.True(result);
        Assert.Equal(token2, cachedToken);
    }

    [Fact]
    public async Task TryGetToken_ExpiredToken()
    {
        var key = "key";
        var token = new AuthResponse(new TokenResponse()) { ExpiresOn = DateTime.Now.AddSeconds(1) };
        TokenCache.Set(key, token);
        await Task.Delay(1500);

        var result = TokenCache.TryGetValue(key, out var cachedToken);
        Assert.False(result);
        Assert.Null(cachedToken);
    }
}