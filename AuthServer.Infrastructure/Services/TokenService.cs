using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Infrastructure.Services
{
    public class TokenService(
        UserManager<AppUser> userManager,
        IOptions<Application.Options.TokenOptions> tokenOptions)
        : ITokenService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly Application.Options.TokenOptions _tokenOptions = tokenOptions.Value;

        public TokenResponseDto CreateToken(AppUser appUser)
        {
            // Parameters
            DateTime accessTokenExpiration = DateTime.Now.AddHours(_tokenOptions.AccessTokenExpirationAsHour);
            DateTime refreshTokenExpiration = DateTime.Now.AddHours(_tokenOptions.RefreshTokenExpirationAsHour);
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));


            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Create Token
            JwtSecurityToken jwtSecurityToken = new(
                issuer: _tokenOptions.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: GetClaims(appUser, _tokenOptions.Audience),
                signingCredentials: signingCredentials
                );

            //Return access and refresh tokens 
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            string accessToken = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

            string refreshToken = CreateRefreshToken();

            return new(accessToken, accessTokenExpiration, refreshToken, refreshTokenExpiration);
        }

        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            // Parameters
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = _tokenOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _tokenOptions.Audience[0],
                ValidateLifetime = true,
                IssuerSigningKey = securityKey
            };

            // Validate Token
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

            TokenValidationResult tokenValidationResult = await jwtSecurityTokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);
            if (tokenValidationResult.IsValid)
                return true;
            return false;
        }

        // Helper Methods 
        private IEnumerable<Claim> GetClaims(AppUser appUser, List<string> audiences)
        {
            if (appUser.UserName is null || appUser.Email is null || appUser.Id.ToString() is "")
                throw new Exception("User object must have UserName, Email and Id properties");

            var userClaims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier,appUser.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, appUser.Email),
            new Claim(ClaimTypes.Name,appUser.UserName),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            userClaims.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

            return userClaims;
        }
        private string CreateRefreshToken()
        {
            var bytes = new byte[32];

            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
