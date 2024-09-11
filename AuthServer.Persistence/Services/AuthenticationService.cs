using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Persistence.Services;

public class AuthenticationService : BaseService, IAuthenticationService
{
    private readonly ITokenService tokenService;
    private readonly UserManager<AppUser> userManager;

    public AuthenticationService(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        this.userManager = userManager;
        this.tokenService = tokenService;
    }

    public async Task<ApiResponse<TokenResponseDto>> SignInAsync(LoginDto loginDto,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(loginDto.UserNameOrEmail);

        var appUser = await userManager.FindByEmailAsync(loginDto.UserNameOrEmail) ??
                      await userManager.FindByNameAsync(loginDto.UserNameOrEmail);
        if (appUser is null)
            return ApiResponse<TokenResponseDto>.Fail(L["EntityNotFound", "User"], StatusCodes.Status404NotFound);

        var isPasswordCorrect = await userManager.CheckPasswordAsync(appUser, loginDto.Password);
        if (!isPasswordCorrect)
            return ApiResponse<TokenResponseDto>.Fail(L["EmailOrPasswordIncorrect"], StatusCodes.Status400BadRequest);

        var tokenResponseDto = tokenService.CreateToken(appUser);
        appUser.RefreshToken = tokenResponseDto.RefreshToken;
        appUser.RefreshTokenExpiration = tokenResponseDto.RefreshTokenExpiration;

        await userManager.UpdateAsync(appUser);

        return ApiResponse<TokenResponseDto>.Success(tokenResponseDto, StatusCodes.Status200OK);
    }

    public async Task<ApiResponse<NoContentDto>> SignOutAsync(RefreshTokenRequestDto refreshTokenRequestDto,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenRequestDto);

        var appUser = await userManager.FindByEmailAsync(refreshTokenRequestDto.RefreshToken) ??
                      await userManager.FindByNameAsync(refreshTokenRequestDto.UsernameOrEmail);
        if (appUser is null)
            return ApiResponse<NoContentDto>.Fail(L["EntityNotFound", "User"], StatusCodes.Status404NotFound);

        if (appUser.RefreshToken != refreshTokenRequestDto.RefreshToken)
            return ApiResponse<NoContentDto>.Fail(L["RefreshTokenInvalid"], StatusCodes.Status400BadRequest);

        appUser.RefreshToken = null;
        appUser.RefreshTokenExpiration = null;
        await userManager.UpdateAsync(appUser);

        return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(refreshTokenDto.RefreshToken);

        var appUser = await userManager.FindByEmailAsync(refreshTokenDto.RefreshToken) ??
                      await userManager.FindByNameAsync(refreshTokenDto.UsernameOrEmail);
        if (appUser is null)
            return ApiResponse<TokenResponseDto>.Fail(L["EntityNotFound", "User"], StatusCodes.Status404NotFound);

        if (appUser.RefreshToken != refreshTokenDto.RefreshToken || appUser.RefreshTokenExpiration < DateTime.Now)
            return ApiResponse<TokenResponseDto>.Fail(L["RefreshTokenInvalid"], StatusCodes.Status400BadRequest);

        var tokenResponseDto = tokenService.CreateToken(appUser);
        appUser.RefreshToken = tokenResponseDto.RefreshToken;
        appUser.RefreshTokenExpiration = tokenResponseDto.RefreshTokenExpiration;

        await userManager.UpdateAsync(appUser);

        return ApiResponse<TokenResponseDto>.Success(tokenResponseDto, StatusCodes.Status200OK);
    }
}