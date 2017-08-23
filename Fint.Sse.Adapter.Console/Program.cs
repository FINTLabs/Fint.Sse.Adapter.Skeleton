using System.IO;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.Services;
using Fint.Sse.Adapter.EventListeners;
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
            serviceProvider.GetService<Application>().Run();
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
                .Build();

            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));

            ConfigureJson();
            ConfigureLogging(configuration);
            ConfigureConsole(configuration);

            // add services
            serviceCollection.AddTransient<IHttpService, HttpService>();
            serviceCollection.AddTransient<IEventStatusService, EventStatusService>();
            serviceCollection.AddTransient<IEventHandlerService, EventHandlerService>();
            serviceCollection.AddTransient<IFintEventListener, FintEventListener>();
            serviceCollection.AddTransient<IPwfaService, PwfaService>();

            // add app
            serviceCollection.AddTransient<Application>();
        }

        private static void ConfigureJson()
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
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
                .WriteTo.RollingFile(logLocation + "\\adapter-{Date}.txt",
                    retainedFileCountLimit: 31)
                .CreateLogger();
        }

        private static void ConfigureConsole(IConfigurationRoot configuration)
        {
            System.Console.Title = configuration.GetSection("Configuration:ConsoleTitle").Value;
        }
    }
}
