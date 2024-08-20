using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.AppUser;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Persistence.Services
{
    public class UserService(UserManager<AppUser> userManager, ITokenService tokenService) : BaseService, IUserService
    {
        public async Task<ApiResponse<AppUserDto>> CreateUserAsync(CreateAppUserDto createUserDto)
        {
            ArgumentNullException.ThrowIfNull(createUserDto);

            AppUser? appUser = ObjectMapper.Map<AppUser>(createUserDto);
            IdentityResult identityResult = await userManager.CreateAsync(appUser, createUserDto.Password);

            if (!identityResult.Succeeded)
                return ApiResponse<AppUserDto>.Fail(
                    new ErrorDto(identityResult.Errors.Select(x => x.Description).ToList()),
                    StatusCodes.Status400BadRequest);
            else
                return ApiResponse<AppUserDto>.Success(ObjectMapper.Map<AppUserDto>(appUser), StatusCodes.Status200OK);
        }

        public async Task<ApiResponse<AppUserDto>> GetUserByUserNameOrEmailAsync(string userNameOrEmail)
        {
            if (string.IsNullOrEmpty(userNameOrEmail))
                throw new Exception("Username or email cannot be null or empty");

            AppUser? appUser = (await userManager.FindByEmailAsync(userNameOrEmail)
                                ?? await userManager.FindByNameAsync(userNameOrEmail));
            return appUser is null
                ? ApiResponse<AppUserDto>.Fail("User not found", StatusCodes.Status404NotFound)
                : ApiResponse<AppUserDto>.Success(ObjectMapper.Map<AppUserDto>(appUser), StatusCodes.Status200OK);
        }

    }
}
