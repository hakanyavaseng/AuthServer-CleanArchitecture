using AutoMapper;

namespace AuthServer.Persistence.AutoMapper
{
    public class ObjectMapper
    {
        public static IMapper Mapper => mapper.Value;

        private static readonly Lazy<IMapper> mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AuthServerMapperProfile>();
            });

            return config.CreateMapper();
        });
    }
}
