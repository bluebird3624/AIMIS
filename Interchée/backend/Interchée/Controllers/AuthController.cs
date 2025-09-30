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
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signIn;
        private readonly UserManager<AppUser> _users;
        private readonly IJwtTokenService _jwt;
        private readonly RefreshTokenService _refreshSvc;

        public AuthController(
            SignInManager<AppUser> signIn,
            UserManager<AppUser> users,
            IJwtTokenService jwt,
            RefreshTokenService refreshSvc)
        {
            _signIn = signIn;
            _users = users;
            _jwt = jwt;
            _refreshSvc = refreshSvc;
        }

        /// <summary>
        /// Log in with email + password.
        /// Returns a short-lived access token (JWT) and a server-stored refresh token.
        /// </summary>
        [HttpPost("login")]
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

            // Issue tokens
            var (access, expiresUtc) = await _jwt.CreateAccessTokenAsync(user);
            var refresh = await _jwt.CreateRefreshTokenAsync(user, HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new AuthResultDto(access, refresh, expiresUtc));
        }

        /// <summary>
        /// Exchange a valid refresh token for a new access token.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResultDto>> Refresh([FromBody] RefreshRequestDto dto)
        {
            // Validate the refresh token
            var token = await _refreshSvc.GetValidAsync(dto.RefreshToken);
            if (token == null) return Unauthorized();

            // Validate the user is active
            var user = await _users.FindByIdAsync(token.UserId.ToString());
            if (user == null || !user.IsActive) return Unauthorized();

            // Issue a new access token. (We keep the same refresh token for simplicity.)
            var (access, expiresUtc) = await _jwt.CreateAccessTokenAsync(user);
            return Ok(new AuthResultDto(access, dto.RefreshToken, expiresUtc));
        }

        /// <summary>
        /// Revoke a refresh token (log that session out). Idempotent.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            await _refreshSvc.RevokeAsync(dto.RefreshToken);
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
