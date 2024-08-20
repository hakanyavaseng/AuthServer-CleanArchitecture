using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.DTOs.Roles;

namespace AuthServer.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<ApiResponse<List<RoleDto>>> GetRolesAsync();
        Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid roleId);
        Task<ApiResponse<NoContentDto>> CreateRoleAsync(string roleName);
        Task<ApiResponse<NoContentDto>> UpdateRoleAsync(string roleName);
        Task<ApiResponse<NoContentDto>> DeleteRoleAsync(string roleName);
        Task<ApiResponse<NoContentDto>> AssignRoleToUserAsync(AssignRoleToUserDto assignRoleToUserDto);
        Task<ApiResponse<NoContentDto>> AssignRoleToEndpoint(AssignRoleToEndpointDto assignRoleToEndpointDto);
        Task<ApiResponse<NoContentDto>> AssignRoleToEndpoints(AssignRoleToEndpointsDto assignRoleToEndpointsDto);
    }
}
