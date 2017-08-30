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
                SseThreadInterval = Convert.ToInt32(TimeSpan.FromSeconds(15).TotalMilliseconds),
                AllowConcurrentConnections = true,
                Organizations = new []{"pwfa.no"},
                SseEndpoint = "https://play-with-fint-adapter.felleskomponent.no/provider/sse"
            };

            _app = new FintSseApplication(_listener, new OptionsWrapper<FintSseSettings>(_appSettings));

            RegisterEventSourceListeners();

            DisplayLogo();
            //RegisterEventSourceListeners(eventSources);

            ConsoleKey key;
            while ((key = System.Console.ReadKey().Key) != ConsoleKey.X)
            {
                switch (key)
                {
                    case ConsoleKey.C:
                        CancelEventSourceListeners();
                        break;
                    case ConsoleKey.R:
                        //RegisterEventSourceListeners(eventSources);
                        break;
                }
            }

        }

        private void CancelEventSourceListeners()
        {
            _app.Disconnect();
        }

        private void RegisterEventSourceListeners()
        {
            foreach (var org in _appSettings.Organizations)
            {
                _logger.LogInformation($"Adding listener for {org}.");

                _app.Connect(org);
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
