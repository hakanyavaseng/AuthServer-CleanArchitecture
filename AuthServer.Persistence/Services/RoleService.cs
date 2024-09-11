using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Roles;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Contexts;
using AuthServer.Persistence.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Persistence.Services;

public class RoleService : BaseService, IRoleService
{
    private readonly AppDbContext _context;
    private readonly RoleManager<AppRole> _roleManager;

    private readonly UserManager<AppUser> _userManager;
    //private readonly IMapper _mapper;

    public RoleService(RoleManager<AppRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync()
    {
        var roles = await Task.Run(() =>
            _roleManager.Roles.Select(x => new RoleDto { Id = x.Id, Name = x.Name }).ToList());

        return ApiResponse<List<RoleDto>>.Success(roles, StatusCodes.Status200OK);
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid roleId)
    {
        var role = await Task.Run(() =>
            _roleManager.Roles.Where(x => x.Id == roleId).Select(x => new RoleDto { Id = x.Id, Name = x.Name })
                .FirstOrDefault());
        if (role != null)
            return ApiResponse<RoleDto>.Success(role, StatusCodes.Status200OK);
        return ApiResponse<RoleDto>.Fail(L["EntityNotFound", "Role"], StatusCodes.Status404NotFound);
    }

    public async Task<ApiResponse<NoContentDto>> CreateRoleAsync(string roleName)
    {
        var result = await _roleManager.CreateAsync(new AppRole { Name = roleName });
        if (result.Succeeded)
            return ApiResponse<NoContentDto>.Success(StatusCodes.Status201Created);
        return ApiResponse<NoContentDto>.Fail(new ErrorDto(result.Errors.Select(x => x.Description).ToList()),
            StatusCodes.Status400BadRequest);
    }

    public async Task<ApiResponse<NoContentDto>> UpdateRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null)
        {
            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
            return ApiResponse<NoContentDto>.Fail(new ErrorDto(result.Errors.Select(x => x.Description).ToList()),
                StatusCodes.Status400BadRequest);
        }

        return ApiResponse<NoContentDto>.Fail(L["EntityNotFound", "Role"], StatusCodes.Status404NotFound);
    }

    public async Task<ApiResponse<NoContentDto>> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null)
        {
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
            return ApiResponse<NoContentDto>.Fail(new ErrorDto(result.Errors.Select(x => x.Description).ToList()),
                StatusCodes.Status400BadRequest);
        }

        return ApiResponse<NoContentDto>.Fail(L["EntityNotFound", "Role"], StatusCodes.Status404NotFound);
    }

    public async Task<ApiResponse<NoContentDto>> AssignRoleToUserAsync(AssignRoleToUserDto assignRoleToUserDto)
    {
        var user = await _context.Users.FindAsync(assignRoleToUserDto.UserId) ?? throw new Exception(L["EntityNotFound", "User"]);

        foreach (var userRole in assignRoleToUserDto.RoleIds)
        {
            var role = await _roleManager.FindByIdAsync(userRole.ToString()) ?? throw new Exception(L["EntityNotFound", "User"]);

            if (!await _userManager.IsInRoleAsync(user, role.Name))
            {
                var result = await _userManager.AddToRoleAsync(user, role.Name);
                if (!result.Succeeded)
                    return ApiResponse<NoContentDto>.Fail(
                        new ErrorDto(result.Errors.Select(x => x.Description).ToList()),
                        StatusCodes.Status400BadRequest);
            }
        }

        return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ApiResponse<NoContentDto>> AssignRoleToEndpoint(AssignRoleToEndpointDto assignRoleToEndpointDto)
    {
        var endpoint = await _context.AuthEndpoints
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Code == assignRoleToEndpointDto.Code);

        if (endpoint == null)
            return ApiResponse<NoContentDto>.Fail(L["EntityNotFound", "Endpoint"], StatusCodes.Status404NotFound);

        foreach (var roleName in assignRoleToEndpointDto.Roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return ApiResponse<NoContentDto>.Fail(L["EntityNotFound", "Role"], StatusCodes.Status404NotFound);

            if (!endpoint.Roles.Any(r => r.Id == role.Id)) endpoint.Roles.Add(role);
        }

        _context.AuthEndpoints.Update(endpoint);

        return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ApiResponse<NoContentDto>> AssignRoleToEndpoints(
        AssignRoleToEndpointsDto assignRoleToEndpointsDto)
    {
        foreach (var endpointCode in assignRoleToEndpointsDto.EndpointCodes)
            await AssignRoleToEndpoint(new AssignRoleToEndpointDto
                { Code = endpointCode, Roles = assignRoleToEndpointsDto.Roles });

        await _context.SaveChangesAsync();
        return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }
}