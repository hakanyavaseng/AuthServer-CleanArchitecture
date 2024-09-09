using System.Reflection;
using AuthServer.Application.Attributes;
using AuthServer.Application.Enums;
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                string[] httpRequestPath = context.Request.Path.Value.Split('/');
                
                string? controller = httpRequestPath[2];
                string? action = httpRequestPath[3];
                string? httpMethod = context.Request.Method;

                string actionType = httpMethod switch
                {
                    "GET" => ActionType.Reading.ToString(),
                    "POST" => ActionType.Writing.ToString(),
                    "PUT" => ActionType.Updating.ToString(),
                    "DELETE" => ActionType.Deleting.ToString()
                };
                string endpointCode = $"{controller}.{httpMethod}.{actionType}.{action}";
                
                var isAuthEndpoint = dbContext.AuthEndpoints.Any(x => x.Code == endpointCode);

                if (isAuthEndpoint)
                {
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
                }
                await _next(context);
            }
        }
    }
}