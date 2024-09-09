using AuthServer.Application.DTOs.Auth;
using AuthServer.Application.Interfaces.Services;

namespace AuthServer.WebAPI.Extensions;

public static class RegisterAuthorizeDefinitionEndpointsExtension
{
    
    public static async Task RegisterAuthorizeDefinitionEndpointsAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        List<MenuDto> authEndpoints = authorizationService.GetAuthorizeDefinitionEndpoints(typeof(Program));

        var registerEndpointDto = new RegisterEndpointsDto
        {
            Menus = authEndpoints
        };

        await authorizationService.RegisterAuthorizeDefinitionEndpointAsync(
            registerEndpointDto,
            typeof(Program),
            CancellationToken.None);
    }
}