using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain;
using AuthServer.Domain.Entities;
using AuthServer.Domain.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace AuthServer.Persistence.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService tokenService;
        private readonly IStringLocalizer<SharedResource> localizer;
        public AuthenticationService(UserManager<AppUser> userManager, ITokenService tokenService, IStringLocalizer<SharedResource> localizer)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.localizer = localizer;
        }

        public async Task<ApiResponse<TokenResponseDto>> SignInAsync(LoginDto loginDto,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(loginDto.UserNameOrEmail);

            AppUser? appUser = await userManager.FindByEmailAsync(loginDto.UserNameOrEmail) ?? await userManager.FindByNameAsync(loginDto.UserNameOrEmail);
            if (appUser is null)
                return ApiResponse<TokenResponseDto>.Fail(localizer["EntityNotFound", "User"], StatusCodes.Status404NotFound);

            bool isPasswordCorrect = await userManager.CheckPasswordAsync(appUser, loginDto.Password);
            if (!isPasswordCorrect)
                return ApiResponse<TokenResponseDto>.Fail("Password is incorrect", StatusCodes.Status400BadRequest);

            var tokenResponseDto = tokenService.CreateToken(appUser);
            appUser.RefreshToken = tokenResponseDto.RefreshToken;
            appUser.RefreshTokenExpiration = tokenResponseDto.RefreshTokenExpiration;

            await userManager.UpdateAsync(appUser);

            return ApiResponse<TokenResponseDto>.Success(tokenResponseDto, StatusCodes.Status200OK);
        }

        public async Task<ApiResponse<NoContentDto>> SignOutAsync(RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(refreshTokenRequestDto);

            AppUser? appUser = await userManager.FindByEmailAsync(refreshTokenRequestDto.RefreshToken) ?? await userManager.FindByNameAsync(refreshTokenRequestDto.UsernameOrEmail);
            if (appUser is null)
                return ApiResponse<NoContentDto>.Fail("User not found", StatusCodes.Status404NotFound);

            if (appUser.RefreshToken != refreshTokenRequestDto.RefreshToken)
                return ApiResponse<NoContentDto>.Fail("Invalid refresh token", StatusCodes.Status400BadRequest);

            appUser.RefreshToken = null;
            appUser.RefreshTokenExpiration = null;
            await userManager.UpdateAsync(appUser);

            return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
        }

        public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(refreshTokenDto.RefreshToken);

            AppUser? appUser = await userManager.FindByEmailAsync(refreshTokenDto.RefreshToken) ?? await userManager.FindByNameAsync(refreshTokenDto.UsernameOrEmail);
            if (appUser is null)
                return ApiResponse<TokenResponseDto>.Fail("User not found", StatusCodes.Status404NotFound);

            if (appUser.RefreshToken != refreshTokenDto.RefreshToken || appUser.RefreshTokenExpiration < DateTime.Now)
                return ApiResponse<TokenResponseDto>.Fail("Invalid refresh token", StatusCodes.Status400BadRequest);

            var tokenResponseDto = tokenService.CreateToken(appUser);
            appUser.RefreshToken = tokenResponseDto.RefreshToken;
            appUser.RefreshTokenExpiration = tokenResponseDto.RefreshTokenExpiration;

            await userManager.UpdateAsync(appUser);

            return ApiResponse<TokenResponseDto>.Success(tokenResponseDto, StatusCodes.Status200OK);
        }


    }
}
