using AuthServer.Application.Enums;
using AuthServer.Domain.Entities;
using AuthServer.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.WebAPI.Middlewares;

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

            var httpRequestPath = context.Request.Path.Value.Split('/');

            var controller = httpRequestPath[2];
            var action = httpRequestPath[3];
            var httpMethod = context.Request.Method;

            var actionType = httpMethod switch
            {
                "GET" => ActionType.Reading.ToString(),
                "POST" => ActionType.Writing.ToString(),
                "PUT" => ActionType.Updating.ToString(),
                "DELETE" => ActionType.Deleting.ToString()
            };
            var endpointCode = $"{controller}.{httpMethod}.{actionType}.{action}";

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
                    var userRoles = await userManager.GetRolesAsync(user);
                    var endpointRecord = await dbContext.AuthEndpoints
                        .Include(e => e.Roles)
                        .FirstOrDefaultAsync(e => e.Code == endpointCode);

                    if (endpointRecord != null)
                    {
                        var isAuthorized = endpointRecord.Roles.Any(r => userRoles.Contains(r.Name));
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