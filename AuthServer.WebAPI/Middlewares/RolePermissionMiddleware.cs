using System.Reflection;
using AuthServer.Application.Attributes;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.WebAPI.Middlewares
{
    public class RolePermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public RolePermissionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Create a scope to resolve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Extract route values
                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    await _next(context);
                    return;
                }

                var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (actionDescriptor == null)
                {
                    await _next(context);
                    return;
                }

                var authorizeDefinitionAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AuthorizeDefinitionAttribute>();
                var httpMethodAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<HttpMethodAttribute>();

                if (authorizeDefinitionAttribute == null)
                {
                    await _next(context);
                    return;
                }

                var userId = context.User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var user = await userManager.FindByNameAsync(userId);
                if (user != null)
                {
                    IList<string> userRoles = await userManager.GetRolesAsync(user);

                    string endpointCode = $"{authorizeDefinitionAttribute.Menu}.{httpMethodAttribute?.HttpMethods.First() ?? "GET"}.{Enum.GetName(authorizeDefinitionAttribute.ActionType)}.{actionDescriptor.MethodInfo.Name}";

                    var endpointRecord = await dbContext.AuthEndpoints
                        .Include(e => e.Roles)
                        .FirstOrDefaultAsync(e => e.Code == endpointCode);

                    if (endpointRecord != null)
                    {
                        bool isAuthorized = endpointRecord.Roles.Any(r => userRoles.Contains(r.Name));
                        if (!isAuthorized)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
                }

                await _next(context);
            }
        }
    }
}
