using HealthForms.Api.Clients;

namespace HealthForms.Api.Shared;

internal static class TokenCache
{
    private static readonly HashSet<TokenCacheItem> Cache = new();
    
    public static bool TryGetValue(string key, out AuthResponse? authResponse)
    {
        RemoveExpired();
        var cacheItem = Cache.FirstOrDefault(x => x.Key == key);
        if (cacheItem is not null)
        {
            authResponse = cacheItem.Token!;
            return true;
        }

        authResponse = null;
        return false;
    }

    public static void Set(string key, AuthResponse token)
    {
        RemoveExpired();
        var cacheItem = Cache.FirstOrDefault(x => x.Key == key);
        if (cacheItem != null)
        {
            cacheItem.Token = token;
            cacheItem.Expires = token.ExpiresOn;
            return;
        }

        Cache.Add(new TokenCacheItem { Token = token, Expires = token.ExpiresOn, Key = key });
    }

    private static void RemoveExpired()
    {
        var now = DateTime.UtcNow;
        Cache.RemoveWhere(x => x.Expires < now);
    }
}

internal class TokenCacheItem
{
    public AuthResponse? Token { get; set; }
    public DateTime Expires { get; set; }
    public string Key { get; set; } = string.Empty;

}