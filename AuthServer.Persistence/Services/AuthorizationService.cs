using System.Reflection;
using AuthServer.Application.Attributes;
using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Contexts;
using AuthServer.Persistence.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Persistence.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly AppDbContext _context;

    public AuthorizationService(AppDbContext context)
    {
        _context = context;
    }

    public List<MenuDto> GetAuthorizeDefinitionEndpoints(Type type)
    {
        List<MenuDto> menus = new();

        var assembly = Assembly.GetAssembly(type);
        if (assembly == null) return menus;

        var controllers = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ControllerBase)));

        foreach (var controller in controllers)
        {
            var actions = controller.GetMethods()
                .Where(m => m.IsDefined(typeof(AuthorizeDefinitionAttribute)));

            foreach (var action in actions)
            {
                var authorizeDefinitionAttribute = action.GetCustomAttribute<AuthorizeDefinitionAttribute>();
                var httpMethodAttribute = action.GetCustomAttribute<HttpMethodAttribute>();

                if (authorizeDefinitionAttribute == null) continue;

                var menu = menus.FirstOrDefault(m => m.Name == authorizeDefinitionAttribute.Menu);

                if (menu == null)
                {
                    menu = new MenuDto { Name = authorizeDefinitionAttribute.Menu };
                    menus.Add(menu);
                }

                ActionDto actionDto = new()
                {
                    ActionType = Enum.GetName(authorizeDefinitionAttribute.ActionType),
                    Definition = authorizeDefinitionAttribute.Definition,
                    HttpType = httpMethodAttribute?.HttpMethods.First() ?? HttpMethods.Get,
                    Code =
                        $"{authorizeDefinitionAttribute.Menu}.{httpMethodAttribute?.HttpMethods.First() ?? HttpMethods.Get}.{Enum.GetName(authorizeDefinitionAttribute.ActionType)}.{action.Name}"
                };

                menu.Actions.Add(actionDto);
            }
        }

        return menus;
    }

    public async Task<ApiResponse<MenuDto>> RegisterAuthorizeDefinitionEndpointAsync(
        RegisterEndpointsDto registerEndpointsDto, Type type, CancellationToken cancellationToken)
    {
        var menus = GetAuthorizeDefinitionEndpoints(type);
        await SaveEndpointsAsync(menus);

        if (registerEndpointsDto != null) await SaveEndpointsAsync(registerEndpointsDto.Menus);

        return ApiResponse<MenuDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ApiResponse<NoContentDto>> SaveEndpointsAsync(List<MenuDto> menus)
    {
        List<Guid> removedEndpointIds = new();

        // Get all current menus and their endpoints from the database
        var allMenus = await _context.AuthMenus.Include(m => m.AuthEndpoints).ToListAsync();

        // Check for removed endpoints and mark them for deletion
        foreach (var dbMenu in allMenus)
        {
            var updatedMenuDto = menus.FirstOrDefault(m => m.Name == dbMenu.Name);

            if (updatedMenuDto != null)
            {
                // Check for removed endpoints within the same menu
                foreach (var dbEndpoint in dbMenu.AuthEndpoints)
                    if (!updatedMenuDto.Actions.Any(a => a.Code == dbEndpoint.Code))
                        removedEndpointIds.Add(dbEndpoint.Id);
            }
            else
            {
                // If the entire menu was deleted, mark all its endpoints for deletion
                foreach (var dbEndpoint in dbMenu.AuthEndpoints) removedEndpointIds.Add(dbEndpoint.Id);
            }
        }

        // Remove the endpoints that have been marked for deletion
        var endpointsToRemove =
            await _context.AuthEndpoints.Where(e => removedEndpointIds.Contains(e.Id)).ToListAsync();
        _context.AuthEndpoints.RemoveRange(endpointsToRemove);

        // Add or update endpoints from the provided menu DTOs
        foreach (var menuDto in menus)
        {
            // Find the menu or create a new one
            var menu = await _context.AuthMenus.Include(m => m.AuthEndpoints)
                .FirstOrDefaultAsync(m => m.Name == menuDto.Name);

            if (menu == null)
            {
                // If the menu doesn't exist, create it and add it to the context
                menu = new AuthMenu
                    { Id = Guid.NewGuid(), Name = menuDto.Name, AuthEndpoints = new List<AuthEndpoint>() };
                _context.AuthMenus.Add(menu);
            }

            // Process each action within the menu
            foreach (var actionDto in menuDto.Actions)
            {
                // Check if the endpoint already exists
                var existingEndpoint = menu.AuthEndpoints.FirstOrDefault(e => e.Code == actionDto.Code);

                if (existingEndpoint == null)
                {
                    // If the endpoint doesn't exist, create and add it
                    var newEndpoint = new AuthEndpoint
                    {
                        Id = Guid.NewGuid(),
                        ActionType = actionDto.ActionType,
                        HttpType = actionDto.HttpType,
                        Definition = actionDto.Definition,
                        Code = actionDto.Code,
                        Menu = menu,
                        Roles = new List<AppRole>()
                    };
                    _context.AuthEndpoints.Add(newEndpoint);
                }
                else
                {
                    if (existingEndpoint.ActionType != actionDto.ActionType ||
                        existingEndpoint.HttpType != actionDto.HttpType ||
                        existingEndpoint.Definition != actionDto.Definition)
                    {
                        existingEndpoint.ActionType = actionDto.ActionType;
                        existingEndpoint.HttpType = actionDto.HttpType;
                        existingEndpoint.Definition = actionDto.Definition;

                        _context.AuthEndpoints.Update(existingEndpoint);
                    }
                }
            }
        }

        // Save all changes to the database
        await _context.SaveChangesAsync();

        return ApiResponse<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }
}