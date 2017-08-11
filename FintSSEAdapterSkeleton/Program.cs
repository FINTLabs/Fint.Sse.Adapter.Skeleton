using Autofac;
using EventSource4Net;
using Fint.SSE.Adapter;
using Fint.SSE.Customcode.Service;
using Serilog;
using System;
using System.Collections.Generic;
using Fint.SSE.Adapter.Event;
using Fint.SSE.Adapter.Service;
using Fint.SSE.Adapter.SSE;

namespace Fint.SSE
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ContainerConfig.Configure();
            using (var scope = container.BeginLifetimeScope())
            {
                var app = scope.Resolve<IApplication>();
                app.Run();
            }
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

            builder.RegisterType<HttpService>();
            builder.RegisterType<ConfigService>();
            builder.RegisterType<EventStatusService>();
            builder.RegisterType<EventHandlerService>();
            builder.RegisterType<FintEventListener>();


            return builder.Build();
        }
    }

    public class Application : IApplication
    {
        private ConfigService _configService;
        public Application(ConfigService configService)
        {
            _configService = configService;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(_configService.LogLocation + "\\adapter-{Date}.txt",
                                        retainedFileCountLimit: 31)
                .CreateLogger();
        }

        public void Run()
        {
            var eventSources = new Dictionary<string, EventSource>();

            foreach (var org in _configService.Organizations)
            {
                eventSources.Add(org, new FintEventListener().Listen(org));
            }

            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.X)
            {
                if (key == ConsoleKey.C)
                {
                    foreach (var item in eventSources)
                    {
                        item.Value.CancellationToken.Cancel();
                        Console.WriteLine($"Eventsource for {item.Key} is cancelled.");
                    }
                }
            }
        }
    }
}
