using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.AppUser;

namespace AuthServer.Application.Interfaces.Services;

public interface IUserService
{
    /// <summary>
    ///     Gets user by username or email or id
    /// </summary>
    /// <param name="userNameOrEmailOrId"></param>
    /// <returns></returns>
    public Task<ApiResponse<AppUserDto>> GetUserByUserNameOrEmailAsync(string userNameOrEmail);

    /// <summary>
    ///     Creates a new user with the given information
    /// </summary>
    /// <param name="createUserDto"></param>
    /// <returns></returns>
    public Task<ApiResponse<AppUserDto>> CreateUserAsync(CreateAppUserDto createUserDto);
}