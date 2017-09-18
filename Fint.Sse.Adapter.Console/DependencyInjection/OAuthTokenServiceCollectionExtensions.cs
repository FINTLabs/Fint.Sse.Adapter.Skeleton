using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fint.Sse.Adapter.Console
{
    public static class OAuthTokenServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthTokenService(this IServiceCollection collection, IConfiguration config)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (config == null) throw new ArgumentNullException(nameof(config));

            collection.Configure<OAuthTokenServiceOptions>(config);
            return collection.AddSingleton<IOAuthTokenService, OAuthTokenService>();
        }
    }
}