using AuthServer.Application.Attributes;
using AuthServer.Application.Consts;
using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Roles;
using AuthServer.Application.Enums;
using AuthServer.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Reading,
        Definition = "Get All Roles")]
    public async Task<ApiResponse<List<RoleDto>>> GetRoles()
    {
        return await _roleService.GetRolesAsync();
    }

    [HttpGet("{roleId}")]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Reading,
        Definition = "Get Role By Id")]
    public async Task<ApiResponse<RoleDto>> GetRoleById(string roleId)
    {
        return await _roleService.GetRoleByIdAsync(Guid.Parse(roleId));
    }

    [HttpPost]
    public async Task<ApiResponse<NoContentDto>> CreateRole([FromBody] string roleName)
    {
        return await _roleService.CreateRoleAsync(roleName);
    }

    [HttpPut("{roleName}")]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Updating,
        Definition = "Update Role")]
    public async Task<ApiResponse<NoContentDto>> UpdateRole(string roleName)
    {
        return await _roleService.UpdateRoleAsync(roleName);
    }

    [HttpDelete("{roleName}")]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Deleting,
        Definition = "Delete Role")]
    public async Task<ApiResponse<NoContentDto>> DeleteRole(string roleName)
    {
        return await _roleService.DeleteRoleAsync(roleName);
    }

    [HttpPost("[action]")]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Writing,
        Definition = "Assign Role To User")]
    public async Task<ApiResponse<NoContentDto>> AssignRoleToUser([FromBody] AssignRoleToUserDto assignRoleToUserDto)
    {
        return await _roleService.AssignRoleToUserAsync(assignRoleToUserDto);
    }

    [HttpPost("[action]")]
    [AuthorizeDefinition(Menu = AuthorizeDefinitionConsts.Roles, ActionType = ActionType.Writing,
        Definition = "Assign Role To Endpoint")]
    public async Task<ApiResponse<NoContentDto>> AssignRoleToEndpoint(
        [FromBody] AssignRoleToEndpointDto assignRoleToEndpointDto)
    {
        return await _roleService.AssignRoleToEndpoint(assignRoleToEndpointDto);
    }

    [HttpPost("[action]")]
    public async Task<ApiResponse<NoContentDto>> AssignRoleToEndpoints(
        [FromBody] AssignRoleToEndpointsDto assignRoleToEndpointsDto)
    {
        return await _roleService.AssignRoleToEndpoints(assignRoleToEndpointsDto);
    }
}