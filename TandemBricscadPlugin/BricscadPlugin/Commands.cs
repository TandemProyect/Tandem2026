using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Teigha.Runtime;
using CadApp = Bricscad.ApplicationServices.Application;

[assembly: CommandClass(typeof(BricscadPlugin.Commands))]

namespace BricscadPlugin
{
    public class Commands
    {
        private readonly MVCApiService _apiService = new MVCApiService();

        [CommandMethod("MVCCONEXION")]
        public void AbrirFormulario()
        {
            PaletteHost.Show();
        }

        [CommandMethod("HOLA")]
        public void MostrarAyuda()
        {
            Editor ed = GetEditor();
            if (ed == null) return;

            ed.WriteMessage("\n=== Plugin BricsCAD V26 - Tandem ===");
            ed.WriteMessage("\nComandos: TANDEM, MVCCONEXION, TANDEM_MURO2D, TANDEM_MURO3D,");
            ed.WriteMessage("\n          GENERAR3D, REGENERAR3D, TANDEM_PROBAR_CONEXION,");
            ed.WriteMessage("\n          TANDEM_DEVICE_ID, TANDEM_CARGAR_MENU");
            ed.WriteMessage($"\nServidor MVC: {_apiService.BaseUrl}");
            ed.WriteMessage("\nVariable opcional: TANDEM_MVC_BASE_URL\n");
        }

        [CommandMethod("TANDEM_PROBAR_CONEXION")]
        public void ProbarConexion()
        {
            Editor ed = GetEditor();
            if (ed == null) return;

            ed.WriteMessage("\n=== Tandem: Prueba de conexión MVC ===");
            ed.WriteMessage($"\nURL base: {_apiService.BaseUrl}");
            try
            {
                string result = Task.Run(() => _apiService.ProbarConexionAsync()).Result;
                ed.WriteMessage($"\nConexión OK — {result}\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n{PluginExceptionHelper.Format(ex, _apiService.BaseUrl)}\n");
            }
        }

        [CommandMethod("TANDEM_DEVICE_ID")]
        public void MostrarDeviceId()
        {
            Editor ed = GetEditor();
            if (ed == null) return;
            ed.WriteMessage($"\nDeviceId actual: {ObtenerDeviceId()}");
            ed.WriteMessage($"\nMachineName: {Environment.MachineName}\n");
        }

        private static Editor GetEditor()
        {
            Document doc = CadApp.DocumentManager.MdiActiveDocument;
            return doc?.Editor;
        }

        private static string ObtenerDeviceId()
        {
            var seed = $"{Environment.MachineName}|{Environment.UserName}|{Environment.UserDomainName}|{Environment.OSVersion}|{ObtenerMachineGuid()}";
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string ObtenerMachineGuid()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid")?.ToString() ?? "NO_GUID";
                }
            }
            catch
            {
                return "NO_GUID";
            }
        }
    }
}
