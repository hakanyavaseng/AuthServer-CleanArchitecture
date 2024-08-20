using AuthServer.Application.DTOs.AppUser;
using AuthServer.Domain.Entities;
using AutoMapper;

namespace AuthServer.Persistence.AutoMapper
{
    public class AuthServerMapperProfile : Profile
    {
        public AuthServerMapperProfile()
        {
            //AppUser
            CreateMap<CreateAppUserDto, AppUser>();
            CreateMap<AppUser, AppUserDto>();
        }
    }
}
