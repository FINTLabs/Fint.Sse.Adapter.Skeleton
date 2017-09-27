using System.IO;
using System.Net.Http;
using Fint.Sse.Adapter.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Fint.Sse.Adapter.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // run app
            serviceProvider.GetService<SseApplication>().Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // add logging
            serviceCollection.AddLogging(loggingBuilder => loggingBuilder
                //.AddConsole()
                .AddDebug()
                .AddSerilog(dispose: true)
            );

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build();

            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            serviceCollection.Configure<FintSseSettings>(configuration.GetSection("FintSseSettings"));

            ConfigureJson();
            ConfigureLogging(configuration);
            ConfigureConsole(configuration);

            // add services with config
            serviceCollection.AddOAuthTokenService(configuration.GetSection("OAuthTokenService"));

            // add services            
            serviceCollection.AddTransient<ITokenService, TokenService>();
            serviceCollection.AddTransient<HttpClient>();
            serviceCollection.AddTransient<EventSource>();
            serviceCollection.AddTransient<IHttpService, HttpService>();
            serviceCollection.AddTransient<IEventStatusService, EventStatusService>();
            serviceCollection.AddTransient<IEventHandlerService, EventHandlerService>();
            serviceCollection.AddTransient<IEventHandler, FintEventHandler>();
            serviceCollection.AddSingleton<IFintEventListener, FintEventListener>();
            serviceCollection.AddTransient<IPwfaService, PwfaService>();

            // add app
            serviceCollection.AddTransient<SseApplication>();
        }

        private static void ConfigureJson()
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter {CamelCaseText = false});
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                return settings;
            });
        }

        private static void ConfigureLogging(IConfigurationRoot configuration)
        {
            string logLocation = configuration.GetSection("Configuration:LogLocation").Value;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(logLocation + Path.DirectorySeparatorChar + "adapter-{Date}.txt",
                    retainedFileCountLimit: 31)
                .CreateLogger();
        }

        private static void ConfigureConsole(IConfigurationRoot configuration)
        {
            System.Console.Title = configuration.GetSection("Configuration:ConsoleTitle").Value;
        }
    }
}