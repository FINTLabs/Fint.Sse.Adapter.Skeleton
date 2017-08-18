using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventSource4Net;
using Fint.SSE.Adapter.Event;
using Fint.SSE.Adapter.Service;
using Fint.SSE.Adapter.SSE;
using Fint.SSE.Customcode.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Console
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            StartUp();

            IServiceCollection services = new ServiceCollection();
            var serviceProvider = ConfigureServices(services);

            var app = serviceProvider.GetRequiredService<IApplication>();
            app.Run();

            //var container = ContainerConfig.Configure();
            //using (var scope = container.BeginLifetimeScope())
            //{
            //    //var app = scope.Resolve<IApplication>();
            //    app.Run();
            //}
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging()
                .AddOptions()
                .Configure<ConfigurationOptions>(Configuration)
                .AddSingleton<IHttpService, HttpService>();

            //if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            //{
            //    services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            //    {
            //        var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

            //        var serviceBusConnectionString = Configuration["EventBusConnection"];
            //        var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

            //        return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
            //    });
            //}
            

            var container = new ContainerBuilder();

            container.RegisterType<Application>().As<IApplication>();

            container.RegisterType<HttpService>().As<IHttpService>();
            //container.RegisterType<ConfigurationOptions>();
            container.RegisterType<EventStatusService>().As<IEventStatusService>();
            container.RegisterType<EventHandlerService>().As<IEventHandlerService>();
            container.RegisterType<FintEventListener>().As<IFintEventListener>();

            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        public static void StartUp()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
    }


    public interface IApplication
    {
        void Run();
    }

    public static class ContainerConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            // All types that implements an interface are registered
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(x => x.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            // Register types without interface.
            builder.RegisterType<Application>().As<IApplication>();

            builder.RegisterType<HttpService>().As<IHttpService>();
            //builder.RegisterType<ConfigurationOptions>();
            builder.RegisterType<EventStatusService>().As<IEventStatusService>();
            builder.RegisterType<EventHandlerService>().As<IEventHandlerService>();
            builder.RegisterType<FintEventListener>().As<IFintEventListener>();


            return builder.Build();
        }
    }

    public class Application : IApplication
    {
        private readonly IFintEventListener _fintEventListener;
        private readonly ConfigurationOptions _configurationOptions;
        public Application(IFintEventListener fintEventListener, IOptions<ConfigurationOptions> configurationOptions)
        {
            _fintEventListener = fintEventListener;
            _configurationOptions = configurationOptions.Value;
            //IConfigurationOptions configService
            //_configurationOptions = configService;

            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.LiterateConsole()
            //    .WriteTo.RollingFile(_configurationOptions.LogLocation + "\\adapter-{Date}.txt",
            //                            retainedFileCountLimit: 31)
            //    .CreateLogger();
        }

        public void Run()
        {
            var eventSources = new Dictionary<string, EventSource>();

            foreach (var org in _configurationOptions.Organizations.Split(","))
            {
                eventSources.Add(org, _fintEventListener.Listen(org));
            }

            ConsoleKey key;
            while ((key = System.Console.ReadKey().Key) != ConsoleKey.X)
            {
                if (key == ConsoleKey.C)
                {
                    foreach (var item in eventSources)
                    {
                        item.Value.CancellationToken.Cancel();
                        System.Console.WriteLine($"Eventsource for {item.Key} is cancelled.");
                    }
                }
            }
        }
    }
}
