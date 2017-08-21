using System;
using System.Collections.Generic;
using EventSource4Net;
using Fint.SSE.Adapter.Service;
using Fint.SSE.Adapter.SSE;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Console
{
    public class Application : IApplication
    {
        private readonly IFintEventListener _fintEventListener;
        private readonly ILogger<Application> _logger;
        private readonly AppSettings _configurationOptions;
        public Application(
            IFintEventListener fintEventListener, 
            IOptions<AppSettings> configurationOptions,
            ILogger<Application> logger)
        {
            _fintEventListener = fintEventListener;
            _logger = logger;
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
                _logger.LogInformation($"Adding listener for {org}.");
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
                        _logger.LogInformation($"Eventsource for {item.Key} is cancelled.");
                    }
                }
            }
        }
    }
}