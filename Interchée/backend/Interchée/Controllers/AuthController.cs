using Interchée.Auth;
using Interchée.Contracts.Auth;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Interchée.Controllers
{
    /// <summary>
    /// Authentication endpoints: login -> access/refresh tokens,
    /// refresh -> rotate access token, logout -> revoke refresh token, me -> identity peek.
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthController(
        SignInManager<AppUser> signIn,
        UserManager<AppUser> users,
        IJwtTokenService jwt,
        RefreshTokenService refreshSvc)
        : ControllerBase
    {
        private readonly SignInManager<AppUser> _signIn = signIn;
        private readonly UserManager<AppUser> _users = users;
        private readonly IJwtTokenService _jwt = jwt;
        private readonly RefreshTokenService _refreshSvc = refreshSvc;

        /// <summary>
        /// Log in with email + password.
        /// Returns a short-lived access token (JWT) and a server-stored refresh token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto dto)
        {
            // Find by email
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            // Ensure account is active
            if (!user.IsActive) return Unauthorized();

            // Validate password (does not sign-in cookie; just checks)
            var pass = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!pass.Succeeded) return Unauthorized();

            // Issue access token
            var (access, expiresUtc) = await _jwt.CreateAccessTokenAsync(user);

            //  Revoke any still-active refresh tokens for this user (single-session policy)
            await _refreshSvc.RevokeAllActiveForUserAsync(user.Id);

            //  Issue a fresh refresh token
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var refresh = await _jwt.CreateRefreshTokenAsync(user, ip);

            return Ok(new AuthResultDto(access, refresh, expiresUtc));
        }


        /// <summary>
        /// Exchange a valid refresh token for a new access token (ROTATES refresh token).
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResultDto>> Refresh([FromBody] RefreshRequestDto dto)
        {
            // 1) Validate incoming refresh token
            var token = await _refreshSvc.GetValidAsync(dto.RefreshToken);
            if (token == null) return Unauthorized();

            // 2) Validate the user is active
            var user = await _users.FindByIdAsync(token.UserId.ToString());
            if (user == null || !user.IsActive) return Unauthorized();

            // 3) Revoke the old refresh token
            await _refreshSvc.RevokeAsync(dto.RefreshToken);

            // 4) Create a NEW refresh token and a NEW access token
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var newRefresh = await _jwt.CreateRefreshTokenAsync(user, ip);
            var (access, expiresUtc) = await _jwt.CreateAccessTokenAsync(user);

            // 5) Return the NEW refresh token (rotation complete)
            return Ok(new AuthResultDto(access, newRefresh, expiresUtc));
        }


        /// <summary>
        /// Revoke a refresh token (log that session out). Idempotent.
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _refreshSvc.RevokeAsync(dto.RefreshToken /*, ip if you add it to the service */);
            return NoContent(); // 204 even if token didn't exist or was already revoked
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _refreshSvc.RevokeAllActiveForUserAsync(Guid.Parse(userId));
            return NoContent();
        }



        /// <summary>Inspect current identity.</summary>
        [HttpGet("me")]              // -> final path is /auth/me
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var u = await _users.FindByIdAsync(userId);
            if (u == null) return Unauthorized();

            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();

            return Ok(new
            {
                userId = u.Id,
                email = u.Email,
                userName = u.UserName,
                firstName = u.FirstName,
                lastName = u.LastName,
                middleName = u.MiddleName,
                roles
            });
        }
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok(new { ok = true, at = DateTime.UtcNow });

    }
}
