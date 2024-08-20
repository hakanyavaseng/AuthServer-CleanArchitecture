using AutoMapper;

namespace AuthServer.Persistence.Services.Common
{
    public class BaseService
    {
        public IMapper ObjectMapper => AutoMapper.ObjectMapper.Mapper;

    }
}
