using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse.Adapter.Console
{
    public class SseApplication : IApplication
    {
        private readonly IFintEventListener _listener;
        private readonly FintSseSettings _fintSseSettings;
        private readonly ILogger<SseApplication> _logger;
        private FintSseApplication _app { get; set; }
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        public SseApplication(
            IFintEventListener listener,
            IOptions<FintSseSettings> fintSseSettings,
            ILogger<SseApplication> logger)
        {
            _listener = listener;
            _fintSseSettings = fintSseSettings.Value;
            _logger = logger;
        }

        public void Run()
        {
            try
            {
                DisplayLogo();
                _app = new FintSseApplication(_listener, new OptionsWrapper<FintSseSettings>(_fintSseSettings));
                RegisterEventSourceListeners();

                System.Console.CancelKeyPress += OnExit;
                _closing.WaitOne();
            }
            catch (Exception e)
            {
                _logger.LogCritical("The program crashed with the following message {error}", e);
                CancelEventSourceListeners();
                RegisterEventSourceListeners();
            }
        }

        private void CancelEventSourceListeners()
        {
            _app.Disconnect();
        }

        private void RegisterEventSourceListeners()
        {
            foreach (var org in _fintSseSettings.Organizations)
            {
                _logger.LogInformation($"Adding listener for {org}.");

                try
                {
                    _app.Connect(org);
                }
                catch (Exception e)
                {
                    _logger.LogCritical("The program crashed with the following message {error}", e);
                    CancelEventSourceListeners();
                    RegisterEventSourceListeners();
                }
            }
        }

        private void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            CancelEventSourceListeners();
        }

        private void DisplayLogo()
        {
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;

            System.Console.WriteLine(
                "        FFFFFFFFFFFFFFFFFFFFFFIIIIIIIIIINNNNNNNN        NNNNNNNNTTTTTTTTTTTTTTTTTTTTTTT");
            System.Console.WriteLine(
                "        F::::::::::::::::::::FI::::::::IN:::::::N       N::::::NT:::::::::::::::::::::T");
            System.Console.WriteLine(
                "        F::::::::::::::::::::FI::::::::IN::::::::N      N::::::NT:::::::::::::::::::::T");
            System.Console.WriteLine(
                "        FF::::::FFFFFFFFF::::FII::::::IIN:::::::::N     N::::::NT:::::TT:::::::TT:::::T");
            System.Console.WriteLine(
                "          F:::::F       FFFFFF  I::::I  N::::::::::N    N::::::NTTTTTT  T:::::T  TTTTTT");
            System.Console.WriteLine(
                "          F:::::F               I::::I  N:::::::::::N   N::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F::::::FFFFFFFFFF     I::::I  N:::::::N::::N  N::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F:::::::::::::::F     I::::I  N::::::N N::::N N::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F:::::::::::::::F     I::::I  N::::::N  N::::N:::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F::::::FFFFFFFFFF     I::::I  N::::::N   N:::::::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F:::::F               I::::I  N::::::N    N::::::::::N        T:::::T        ");
            System.Console.WriteLine(
                "          F:::::F               I::::I  N::::::N     N:::::::::N        T:::::T        ");
            System.Console.WriteLine(
                "        FF:::::::FF           II::::::IIN::::::N      N::::::::N      TT:::::::TT      ");
            System.Console.WriteLine(
                "        F::::::::FF           I::::::::IN::::::N       N:::::::N      T:::::::::T      ");
            System.Console.WriteLine(
                "        F::::::::FF           I::::::::IN::::::N        N::::::N      T:::::::::T      ");
            System.Console.WriteLine(
                "        FFFFFFFFFFF           IIIIIIIIIINNNNNNNN         NNNNNNN      TTTTTTTTTTT      ");
            System.Console.WriteLine();

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("           Greetings from FINTLabs!");
            System.Console.ResetColor();

            System.Console.WriteLine();
            System.Console.WriteLine();
        }
    }
}