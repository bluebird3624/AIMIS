using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Interchée.Auth
{
    /// <summary>
    /// Issues signed JWT access tokens and creates persisted refresh tokens.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>Creates a signed JWT access token for the given user.</summary>
        Task<(string accessToken, DateTime expiresUtc)> CreateAccessTokenAsync(AppUser user);

        /// <summary>Creates and stores a new refresh token for the user.</summary>
        Task<string> CreateRefreshTokenAsync(AppUser user, string? ip = null);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opt;
        private readonly UserManager<AppUser> _userMgr;
        private readonly AppDbContext _db;

        public JwtTokenService(JwtOptions opt, UserManager<AppUser> userMgr, AppDbContext db)
        {
            _opt = opt;
            _userMgr = userMgr;
            _db = db;
        }

        public async Task<(string accessToken, DateTime expiresUtc)> CreateAccessTokenAsync(AppUser user)
        {
            // Base claims
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

            // ADD THESE TWO so your controller can read them
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),

            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

            // Also include Identity roles as role claims, so [Authorize(Roles="...")] can work
            var roles = await _userMgr.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // Build token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);

            var jwt = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, expires);
        }

        public async Task<string> CreateRefreshTokenAsync(AppUser user, string? ip = null)
        {
            // Use a random opaque token (not a JWT) for refresh tokens
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." + Guid.NewGuid();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays),
                CreatedByIp = ip
            });

            await _db.SaveChangesAsync();
            return token;
        }
    }

}
