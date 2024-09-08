using AuthServer.Application.Attributes;
using AuthServer.Application.Consts;
using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Enums;
using AuthServer.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IAuthorizationService authorizationService;

        public AuthController(IAuthenticationService authenticationService, IAuthorizationService authorizationService)
        {
            this.authenticationService = authenticationService;
            this.authorizationService = authorizationService;
        }

        [HttpGet]
        public IActionResult TestAuth()
        {
            return Ok(authorizationService.GetAuthorizeDefinitionEndpoints(typeof(Program)));
        }

        [HttpPost("SignIn")]
        public async Task<ApiResponse<TokenResponseDto>> SignIn([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            return await authenticationService.SignInAsync(loginDto, cancellationToken);
        }

        [HttpPost("SignOut")]
        public async Task<ApiResponse<NoContentDto>> SignOut([FromBody] RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken)
        {
            return await authenticationService.SignOutAsync(refreshTokenRequestDto, cancellationToken);
        }

        [HttpPost("RefreshToken")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Auth, ActionType = ActionType.Writing, Definition = "Get refresh token")]
        public async Task<ApiResponse<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto, CancellationToken cancellationToken)
        {
            return await authenticationService.RefreshTokenAsync(refreshTokenDto, cancellationToken);
        }

        [HttpPost("RegisterEndpoints")]
        public async Task<ApiResponse<MenuDto>> RegisterEndpoints(RegisterEndpointsDto? dto, CancellationToken cancellationToken)
        {
            return await authorizationService.RegisterAuthorizeDefinitionEndpoints(dto, typeof(Program), cancellationToken);
        }


    }
}
