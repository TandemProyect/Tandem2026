using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCadStudioPluging
{
    internal static class Program
    {
        private const string ShowUiEventName = @"Local\Tandem2026OpenCadHostShowUi";

        private static EventWaitHandle _showUiEvent;
        private static RegisteredWaitHandle _showUiRegistration;

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return MainAsync(args ?? Array.Empty<string>()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error OpenCadStudioPluging: " + ex.Message);
                return 1;
            }
        }

        private static async Task<int> MainAsync(string[] args)
        {
            var command = args.Length > 0 ? (args[0] ?? "").Trim().ToLowerInvariant() : "help";
            switch (command)
            {
                case "tandem":
                case "connect-ui":
                    return RunConnectUi();
                case "help":
                    PrintHelp();
                    return 0;
                default:
                    return await RunBridgeCommandAsync(args).ConfigureAwait(false);
            }
        }

        private static int RunConnectUi()
        {
            _showUiEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowUiEventName);
            using (var uiMutex = new Mutex(true, @"Local\Tandem2026OpenCadHost", out var createdNew))
            {
                if (!createdNew)
                {
                    try { _showUiEvent.Set(); } catch { }
                    return 0;
                }

                OpenCadPaletteWindow.PrepareNativeLoader();
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnLastWindowClose
                };
                OpenCadPaletteHost.Show();
                RegisterShowUiSignalHandler();
                try
                {
                    return app.Run();
                }
                finally
                {
                    UnregisterShowUiSignalHandler();
                    OpenCadPaletteHost.CloseAll();
                }
            }
        }

        private static Task<int> RunBridgeCommandAsync(string[] args)
        {
            try
            {
                return Task.FromResult(BridgeCommandRunner.Run(args));
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                PrintHelp();
                return Task.FromResult(2);
            }
        }

        private static void RegisterShowUiSignalHandler()
        {
            _showUiRegistration = ThreadPool.RegisterWaitForSingleObject(
                _showUiEvent,
                (_, __) =>
                {
                    try
                    {
                        var dispatcher = Application.Current?.Dispatcher;
                        if (dispatcher == null)
                            return;
                        dispatcher.BeginInvoke((Action)(() => OpenCadPaletteHost.ReopenUi()));
                    }
                    catch
                    {
                    }
                },
                null,
                Timeout.Infinite,
                false);
        }

        private static void UnregisterShowUiSignalHandler()
        {
            try { _showUiRegistration?.Unregister(null); } catch { }
            _showUiRegistration = null;
            try { _showUiEvent?.Dispose(); } catch { }
            _showUiEvent = null;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("OpenCadStudioPluging - comandos:");
            Console.WriteLine("  tandem | connect-ui             Abre login + barras MVC (Tandem 2026)");
            Console.WriteLine("  ping | connect                  Ping al MVC activo");
            Console.WriteLine("  show-server                     Muestra URL MVC actual");
            Console.WriteLine("  set-local                       Usa https://localhost:44384/");
            Console.WriteLine("  set-production                  Usa https://tdesing.net/");
            Console.WriteLine("  ocs-smoke                       Crea un dibujo y ejecuta una linea");
            Console.WriteLine("  ocs-run <comando-cad>           Ejecuta comando CAD en OpenCADStudio --serve");
            Console.WriteLine("  ui-run <comando-cad>            Ejecuta comando CAD en la ventana activa de OpenCADStudio");
            Console.WriteLine("  wall-step <e> [r=0|1] <p1;...;pn> [s1;...;sn] Calcula adiciones Wall2D vía Desing (C#)");
            Console.WriteLine("  ocs-insert-line x1 y1 x2 y2     Inserta una linea con coordenadas en WCS");
            Console.WriteLine();
            Console.WriteLine("Variable opcional: TANDEM_OPENCADSTUDIO_PATH (ruta al ejecutable OpenCADStudio).");
        }
    }
}
