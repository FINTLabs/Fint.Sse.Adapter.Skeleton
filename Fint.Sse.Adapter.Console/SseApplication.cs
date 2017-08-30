using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Console
{
    public class SseApplication : IApplication
    {
        private readonly IFintEventListener _listener;
        private  FintSseSettings _appSettings;
        private readonly ILogger<SseApplication> _logger;
        public FintSseApplication _app { get; set; }

        public SseApplication(
            IFintEventListener listener,
            IOptions<FintSseSettings> appSettings,
            ILogger<SseApplication> logger)
        {
            _listener = listener;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public void Run()
        {
            _appSettings = new FintSseSettings
            {
                SseThreadInterval = Convert.ToInt32(TimeSpan.FromSeconds(20).TotalMilliseconds),
                AllowConcurrentConnections = true,
                Organizations = new []{"pwfa.no"},
                SseEndpoint = "https://play-with-fint-adapter.felleskomponent.no/provider/sse"
            };

            _app = new FintSseApplication(_listener, new OptionsWrapper<FintSseSettings>(_appSettings));

            RegisterEventSourceListeners();
            
            System.Console.WriteLine("\r\n___________.___ __________________\r\n\\_   _____/|   |\\      \\__    ___/\r\n |    __)  |   |/   |   \\|    |   \r\n |     \\   |   /    |    \\    |   \r\n \\___  /   |___\\____|__  /____|   \r\n     \\/                \\/         \r\n");

            //RegisterEventSourceListeners(eventSources);

            ConsoleKey key;
            while ((key = System.Console.ReadKey().Key) != ConsoleKey.X)
            {
                switch (key)
                {
                    case ConsoleKey.C:
                        //CancelEventSourceListeners(eventSources);
                        break;
                    case ConsoleKey.R:
                        //RegisterEventSourceListeners(eventSources);
                        break;
                }
            }

        }

        private void RegisterEventSourceListeners()
        {
            foreach (var org in _appSettings.Organizations)
            {
                _logger.LogInformation($"Adding listener for {org}.");

                _app.Connect(org);
            }
        }
    }
}
