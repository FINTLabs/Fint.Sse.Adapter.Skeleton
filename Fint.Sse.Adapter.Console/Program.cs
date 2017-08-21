using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fint.SSE.Adapter.Event;
using Fint.SSE.Adapter.Service;
using Fint.SSE.Adapter.SSE;
using Fint.SSE.Customcode.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fint.Sse.Adapter.Console
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
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
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddDebug());
            serviceCollection.AddLogging();

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));
            ConfigureConsole(configuration);

            // add services
            serviceCollection.AddTransient<IHttpService, HttpService>();
            serviceCollection.AddTransient<IEventStatusService, EventStatusService>();
            serviceCollection.AddTransient<IEventHandlerService, EventHandlerService>();
            serviceCollection.AddTransient<IFintEventListener, FintEventListener>();

            // add app
            serviceCollection.AddTransient<Application>();
        }

        private static void ConfigureConsole(IConfigurationRoot configuration)
        {
            System.Console.Title = configuration.GetSection("Configuration:ConsoleTitle").Value;
        }
    }
}
