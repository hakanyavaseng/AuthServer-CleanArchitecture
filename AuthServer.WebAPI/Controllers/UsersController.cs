using AuthServer.Application.Attributes;
using AuthServer.Application.Consts;
using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.AppUser;
using AuthServer.Application.Enums;
using AuthServer.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Users, ActionType = ActionType.Writing, Definition = "Create user")]
        public async Task<ApiResponse<AppUserDto>> Create([FromBody] CreateAppUserDto createAppUserDto)
        {
            return await _userService.CreateUserAsync(createAppUserDto);
        }

        [HttpGet("{userNameOrEmail}")]
        public async Task<ApiResponse<AppUserDto>> Get(string userNameOrEmail)
        {
            return await _userService.GetUserByUserNameOrEmailAsync(userNameOrEmail);
        }
    }
}
