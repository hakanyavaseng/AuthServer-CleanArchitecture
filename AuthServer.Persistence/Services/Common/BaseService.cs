using AuthServer.Domain.Localization;
using AutoMapper;
using Microsoft.Extensions.Localization;
using System;
using AuthServer.Application.Helpers;

namespace AuthServer.Persistence.Services.Common
{
    public class BaseService
    {
        private readonly Lazy<IMapper> _lazyObjectMapper;
        private Lazy<IStringLocalizer<SharedResource>> _lazyLocalizer;

        public BaseService()
        {
            _lazyObjectMapper = new Lazy<IMapper>(() => ServiceLocator.GetService<IMapper>());
            _lazyLocalizer = new Lazy<IStringLocalizer<SharedResource>>(() => ServiceLocator.GetService<IStringLocalizer<SharedResource>>());
        }

        // Public properties
        public IMapper ObjectMapper => _lazyObjectMapper.Value;
        public IStringLocalizer<SharedResource> L => _lazyLocalizer.Value;
    }
}