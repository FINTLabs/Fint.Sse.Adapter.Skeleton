using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fint.Sse.Adapter.Models;
using Fint.Sse.Adapter.EventListeners;

namespace Fint.Sse.Adapter.Console
{
    public interface IApplication
    {
        void Run();
    }

    public class Application : IApplication
    {
        private readonly IFintEventListener _fintEventListener;
        private readonly ILogger<Application> _logger;
        private readonly AppSettings _appSettings;

        public Application(
            IFintEventListener fintEventListener, 
            IOptions<AppSettings> appSettings,
            ILogger<Application> logger)
        {
            _fintEventListener = fintEventListener;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public void Run()
        {
            var eventSources = new Dictionary<string, EventSource>();

            System.Console.WriteLine("\r\n___________.___ __________________\r\n\\_   _____/|   |\\      \\__    ___/\r\n |    __)  |   |/   |   \\|    |   \r\n |     \\   |   /    |    \\    |   \r\n \\___  /   |___\\____|__  /____|   \r\n     \\/                \\/         \r\n");
            System.Console.WriteLine("  Greetings from FINTLabs!");
            System.Console.WriteLine();

            RegisterEventSourceListeners(eventSources);

            ConsoleKey key;
            while ((key = System.Console.ReadKey().Key) != ConsoleKey.X)
            {
                switch (key)
                {
                    case ConsoleKey.C:
                        CancelEventSourceListeners(eventSources);
                        break;
                    case ConsoleKey.R:
                        RegisterEventSourceListeners(eventSources);
                        break;
                }
            }
        }

        private void CancelEventSourceListeners(Dictionary<string, EventSource> eventSources)
        {
            foreach (var item in eventSources.ToList())
            {
                item.Value.CancellationToken.Cancel();
                eventSources.Remove(item.Key);
                _logger.LogInformation($"Eventsource for {item.Key} is cancelled.");
            }
        }

        private void RegisterEventSourceListeners(Dictionary<string, EventSource> eventSources)
        {
            foreach (var org in _appSettings.Organizations.Split(","))
            {
                _logger.LogInformation($"Adding listener for {org}.");
                eventSources.Add(org, _fintEventListener.Listen(org));
            }
        }
    }
}