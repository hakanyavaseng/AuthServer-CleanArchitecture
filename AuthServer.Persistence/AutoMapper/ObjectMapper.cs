using AutoMapper;

namespace AuthServer.Persistence.AutoMapper;

public class ObjectMapper
{
    private static readonly Lazy<IMapper> mapper = new(() =>
    {
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<AuthServerMapperProfile>(); });

        return config.CreateMapper();
    });

    public static IMapper Mapper => mapper.Value;
}