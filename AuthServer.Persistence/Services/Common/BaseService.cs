using AuthServer.Domain.Localization;
using AutoMapper;
using Microsoft.Extensions.Localization;

namespace AuthServer.Persistence.Services.Common;

public class BaseService
{
    private readonly Lazy<IMapper> _lazyObjectMapper = new(() => AutoMapper.ObjectMapper.Mapper);
    private Lazy<IStringLocalizer<SharedResource>> _lazyLocalizer;

    public BaseService()
    {
        _lazyLocalizer = new Lazy<IStringLocalizer<SharedResource>>(() =>
            throw new InvalidOperationException("Localizer has not been set."));
    }

    public IStringLocalizer<SharedResource> Localizer
    {
        set
        {
            if (_lazyLocalizer.IsValueCreated)
                throw new InvalidOperationException("Localizer can only be set once.");
            _lazyLocalizer = new Lazy<IStringLocalizer<SharedResource>>(() => value);
        }
    }

    //Public properties
    public IMapper ObjectMapper => _lazyObjectMapper.Value;
    public IStringLocalizer<SharedResource> L => _lazyLocalizer.Value;
}