using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

            //System.Console.WriteLine("\r\n___________.___ __________________\r\n\\_   _____/|   |\\      \\__    ___/\r\n |    __)  |   |/   |   \\|    |   \r\n |     \\   |   /    |    \\    |   \r\n \\___  /   |___\\____|__  /____|   \r\n     \\/                \\/         \r\n");
            //System.Console.WriteLine("  Greetings from FINTLabs!");
            //System.Console.WriteLine();
            DisplayLogo();

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
                var eventSource = eventSources.Single(es => es.Key == item.Key);
                eventSource.Value.CancellationToken.Cancel();
                eventSources.Remove(item.Key);
                _logger.LogInformation($"Eventsource for {item.Key} is cancelled.");
            }
        }

        private void RegisterEventSourceListeners(Dictionary<string, EventSource> eventSources)
        {
            foreach (var org in _appSettings.Organizations.Split(","))
            {
                _logger.LogInformation($"Adding listener for {org}.");

                //var eventSource = _fintEventListener.Listen(org);

                //eventSources.Add(org, eventSource);
            }
        }

        private void DisplayLogo()
        {
			System.Console.WriteLine();
			System.Console.WriteLine();
			System.Console.ForegroundColor = ConsoleColor.Red;

			System.Console.WriteLine("        FFFFFFFFFFFFFFFFFFFFFFIIIIIIIIIINNNNNNNN        NNNNNNNNTTTTTTTTTTTTTTTTTTTTTTT");
			System.Console.WriteLine("        F::::::::::::::::::::FI::::::::IN:::::::N       N::::::NT:::::::::::::::::::::T");
			System.Console.WriteLine("        F::::::::::::::::::::FI::::::::IN::::::::N      N::::::NT:::::::::::::::::::::T");
			System.Console.WriteLine("        FF::::::FFFFFFFFF::::FII::::::IIN:::::::::N     N::::::NT:::::TT:::::::TT:::::T");
			System.Console.WriteLine("          F:::::F       FFFFFF  I::::I  N::::::::::N    N::::::NTTTTTT  T:::::T  TTTTTT");
			System.Console.WriteLine("          F:::::F               I::::I  N:::::::::::N   N::::::N        T:::::T        ");
			System.Console.WriteLine("          F::::::FFFFFFFFFF     I::::I  N:::::::N::::N  N::::::N        T:::::T        ");
			System.Console.WriteLine("          F:::::::::::::::F     I::::I  N::::::N N::::N N::::::N        T:::::T        ");
			System.Console.WriteLine("          F:::::::::::::::F     I::::I  N::::::N  N::::N:::::::N        T:::::T        ");
			System.Console.WriteLine("          F::::::FFFFFFFFFF     I::::I  N::::::N   N:::::::::::N        T:::::T        ");
			System.Console.WriteLine("          F:::::F               I::::I  N::::::N    N::::::::::N        T:::::T        ");
			System.Console.WriteLine("          F:::::F               I::::I  N::::::N     N:::::::::N        T:::::T        ");
			System.Console.WriteLine("        FF:::::::FF           II::::::IIN::::::N      N::::::::N      TT:::::::TT      ");
			System.Console.WriteLine("        F::::::::FF           I::::::::IN::::::N       N:::::::N      T:::::::::T      ");
			System.Console.WriteLine("        F::::::::FF           I::::::::IN::::::N        N::::::N      T:::::::::T      ");
			System.Console.WriteLine("        FFFFFFFFFFF           IIIIIIIIIINNNNNNNN         NNNNNNN      TTTTTTTTTTT      ");
			System.Console.WriteLine();

            System.Console.ForegroundColor = ConsoleColor.Yellow;
			System.Console.WriteLine("           Greetings from FINTLabs!");
            System.Console.ResetColor();

			System.Console.WriteLine();
			System.Console.WriteLine();

		}
    }
}