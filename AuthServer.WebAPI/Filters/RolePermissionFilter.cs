using AuthServer.Application.Attributes;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.WebAPI.Filters
{
    public class RolePermissionFilter : IAsyncActionFilter
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public RolePermissionFilter(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            ControllerActionDescriptor? descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            AuthorizeDefinitionAttribute? authorizeDefinitionAttribute = descriptor?.MethodInfo.GetCustomAttribute<AuthorizeDefinitionAttribute>();
            HttpMethodAttribute? httpMethodAttribute = descriptor?.MethodInfo.GetCustomAttribute<HttpMethodAttribute>();

            if (authorizeDefinitionAttribute == null)
            {
                await next();
                return;
            }
            var userId = context.HttpContext.User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var user = await _userManager.FindByNameAsync(userId);
            if (user != null)
            {
                IList<string> userRoles = await _userManager.GetRolesAsync(user);

                string endpointCode = $"{authorizeDefinitionAttribute.Menu}.{httpMethodAttribute?.HttpMethods.First() ?? HttpMethods.Get}.{Enum.GetName(authorizeDefinitionAttribute.ActionType)}.{descriptor?.MethodInfo.Name}";

                AuthEndpoint? endpoint = await _context.AuthEndpoints
                    .Include(e => e.Roles)
                    .FirstOrDefaultAsync(e => e.Code == endpointCode);

                if (endpoint != null)
                {
                    bool isAuthorized = endpoint.Roles.Any(r => userRoles.Contains(r.Name));
                    if (!isAuthorized)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
                else
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            await next();
        }
    }
}
