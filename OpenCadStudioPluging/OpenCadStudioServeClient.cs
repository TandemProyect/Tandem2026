using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCadStudioPluging
{
    internal sealed class OpenCadStudioServeClient
    {
        private readonly string _executablePath;
        private readonly TimeSpan _startupTimeout;
        private readonly TimeSpan _requestTimeout;

        public OpenCadStudioServeClient(string executablePath, TimeSpan startupTimeout, TimeSpan requestTimeout)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
                throw new ArgumentException("Ruta del ejecutable OpenCADStudio invalida.", nameof(executablePath));
            if (startupTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(startupTimeout));
            if (requestTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(requestTimeout));

            _executablePath = executablePath.Trim();
            _startupTimeout = startupTimeout;
            _requestTimeout = requestTimeout;
        }

        public static OpenCadStudioServeClient CreateDefault()
        {
            return new OpenCadStudioServeClient(
                OpenCadStudioPluginEnvironment.ResolveOpenCadStudioExecutable(),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(20));
        }

        public async Task<OpenCadRunResponse> RunCadCommandAsync(string cadCommand, CancellationToken cancellationToken)
        {
            using (var host = StartHost())
            {
                await host.WaitForReadyAsync(_startupTimeout, cancellationToken).ConfigureAwait(false);

                var newResponse = await host.SendRequestAsync(OpenCadServeProtocol.BuildLegacyNewRequest(), _requestTimeout, cancellationToken)
                    .ConfigureAwait(false);
                if (!OpenCadServeProtocol.TryEnsureOkResponse(newResponse, out var newError))
                    throw new InvalidOperationException("OpenCADStudio rechazo la creacion de dibujo: " + newError);

                var runResponse = await host.SendRequestAsync(OpenCadServeProtocol.BuildLegacyRunRequest(cadCommand), _requestTimeout, cancellationToken)
                    .ConfigureAwait(false);
                if (!OpenCadServeProtocol.TryParseRunResponse(runResponse, out var parsed, out var parseError))
                    throw new InvalidOperationException("OpenCADStudio devolvio una respuesta run invalida: " + parseError);

                return parsed;
            }
        }

        private OpenCadStudioServeHost StartHost()
        {
            return OpenCadStudioServeHost.Start(_executablePath);
        }

        private sealed class OpenCadStudioServeHost : IDisposable
        {
            private readonly Process _process;

            private OpenCadStudioServeHost(Process process)
            {
                _process = process;
            }

            public static OpenCadStudioServeHost Start(string executablePath)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "--serve",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process
                {
                    StartInfo = startInfo
                };

                try
                {
                    if (!process.Start())
                        throw new InvalidOperationException("OpenCADStudio no inicio el proceso --serve.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("No se pudo iniciar OpenCADStudio con --serve. Ruta: " + executablePath, ex);
                }

                return new OpenCadStudioServeHost(process);
            }

            public async Task WaitForReadyAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
                var line = await ReadLineWithTimeoutAsync(_process.StandardOutput, timeout, cancellationToken, "banner ready")
                    .ConfigureAwait(false);
                if (!OpenCadServeProtocol.TryParseReadyBanner(line, out var error))
                    throw new InvalidOperationException("Banner invalido de OpenCADStudio --serve: " + error + ". Linea: " + line);
            }

            public async Task<string> SendRequestAsync(string jsonLine, TimeSpan timeout, CancellationToken cancellationToken)
            {
                await _process.StandardInput.WriteLineAsync(jsonLine).ConfigureAwait(false);
                await _process.StandardInput.FlushAsync().ConfigureAwait(false);
                return await ReadLineWithTimeoutAsync(_process.StandardOutput, timeout, cancellationToken, "respuesta JSON")
                    .ConfigureAwait(false);
            }

            public void Dispose()
            {
                try
                {
                    if (!_process.HasExited)
                        _process.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("No se pudo cerrar el proceso OpenCADStudio --serve: " + ex.Message);
                }
                finally
                {
                    _process.Dispose();
                }
            }

            private static async Task<string> ReadLineWithTimeoutAsync(
                StreamReader reader,
                TimeSpan timeout,
                CancellationToken cancellationToken,
                string stage)
            {
                var readTask = reader.ReadLineAsync();
                var delayTask = Task.Delay(timeout, cancellationToken);
                var completed = await Task.WhenAny(readTask, delayTask).ConfigureAwait(false);
                if (completed != readTask)
                    throw new TimeoutException("Timeout esperando " + stage + " de OpenCADStudio.");

                var line = await readTask.ConfigureAwait(false);
                if (line == null)
                    throw new InvalidOperationException("OpenCADStudio cerro salida estandar durante " + stage + ".");

                return line;
            }
        }
    }
}
