﻿using AuthServer.Application.DTOs.ApiResponses;
using AuthServer.Application.DTOs.Auth;

namespace AuthServer.Application.Interfaces.Services
{
    public interface IAuthorizationService
    {
        List<MenuDto> GetAuthorizeDefinitionEndpoints(Type assemblyType);
        Task<ApiResponse<NoContentDto>> RegisterAuthorizeDefinitionEndpoints(Type type, CancellationToken cancellationToken);
        Task<ApiResponse<NoContentDto>> SaveEndpointsAsync(List<MenuDto> menus);
    }
}
