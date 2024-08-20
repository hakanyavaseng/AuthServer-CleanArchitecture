using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;

namespace AuthServer.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Signs in the user with the given information
        /// </summary>
        /// <param name="loginDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ApiResponse<TokenResponseDto>> SignInAsync(LoginDto loginDto, CancellationToken cancellationToken);

        public Task<ApiResponse<NoContentDto>> SignOutAsync(RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new token for the given user with the given refresh token
        /// </summary>
        /// <param name="refreshTokenDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto,
            CancellationToken cancellationToken);

    }
}
