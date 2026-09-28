using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AutocadPlugin.Commands))]

namespace AutocadPlugin
{
    public class Commands
    {
        private readonly MVCApiService _apiService = new MVCApiService();

        private string FormatPluginError(System.Exception ex)
        {
            return PluginExceptionHelper.Format(ex, _apiService.BaseUrl);
        }

        [CommandMethod("MVCCONEXION")]
        public void AbrirFormulario()
        {
            PaletteHost.Show();
        }

        [CommandMethod("INSERTARBLOQUE")]
        public void InsertarBloque()
        {
            WriteStub("INSERTARBLOQUE", "Inserción de bloques pendiente de portar.");
        }

        [CommandMethod("DETECTARMUROS")]
        public void DetectarMuros()
        {
            WriteStub("DETECTARMUROS", "Detección de muros pendiente de portar.");
        }

        // GENERAR3D / REGENERAR3D viven en Wall3dCommand (misma API que Desing_2).

        [CommandMethod("CONFIGENCOFRADO")]
        public void ConfigurarEncofrado()
        {
            WriteStub("CONFIGENCOFRADO", "Configuración de encofrado pendiente de portar.");
        }

        [CommandMethod("LEERDISENOMVC")]
        public void LeerDisenoMvc()
        {
            PaletteHost.Show();
        }

        [CommandMethod("CREARDISENOMVC")]
        public void CrearDisenoMvc()
        {
            WriteStub("CREARDISENOMVC", "Alta de diseño MVC pendiente de portar.");
        }

        [CommandMethod("GUARDARDISENOMVC")]
        public void GuardarDisenoMvc()
        {
            WriteStub("GUARDARDISENOMVC", "Guardado de diseño MVC pendiente de portar.");
        }

        [CommandMethod("TANDEM_SELECCIONAR_LINEAS")]
        public void SeleccionarLineas()
        {
            WriteStub("TANDEM_SELECCIONAR_LINEAS", "Selección de líneas pendiente de portar.");
        }

        [CommandMethod("TANDEM_ANALIZAR_IMAGEN")]
        public void AnalizarImagen()
        {
            WriteStub("TANDEM_ANALIZAR_IMAGEN", "Análisis de imagen pendiente de portar.");
        }

        [CommandMethod("HOLA")]
        public void MostrarAyuda()
        {
            Editor ed = GetEditor();
            if (ed == null) return;

            ed.WriteMessage("\n=== Plugin AutoCAD 2026 - Tandem ===");
            ed.WriteMessage("\nComandos: TANDEM, MVCCONEXION, TANDEM_MURO2D, TANDEM_MURO3D,");
            ed.WriteMessage("\n          GENERAR3D, REGENERAR3D, TANDEM_ABRIRDISENO,");
            ed.WriteMessage("\n          LEERDISENOMVC,");
            ed.WriteMessage("\n          CREARDISENOMVC, GUARDARDISENOMVC,");
            ed.WriteMessage("\n          TANDEM_SELECCIONAR_LINEAS, TANDEM_ANALIZAR_IMAGEN,");
            ed.WriteMessage("\n          TANDEM_PROBAR_CONEXION, TANDEM_DEVICE_ID, TANDEM_CARGAR_MENU");
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
                ed.WriteMessage($"\n{FormatPluginError(ex)}\n");
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

        private void WriteStub(string command, string detail)
        {
            Editor ed = GetEditor();
            if (ed == null) return;
            ed.WriteMessage($"\n[{command}] {detail} Escribe TANDEM para la lista completa.\n");
        }

        private static Editor GetEditor()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
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
