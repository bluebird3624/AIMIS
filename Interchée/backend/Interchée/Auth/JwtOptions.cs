namespace Interchée.Auth
{
    /// <summary>
    /// Bound from appsettings.json -> "Jwt" section.
    /// Controls token audience/issuer/key and lifetimes.
    /// </summary>
    public class JwtOptions
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string Key { get; set; } = default!;
        public int AccessTokenMinutes { get; set; } = 30;
        public int RefreshTokenDays { get; set; } = 7;
    }
}
