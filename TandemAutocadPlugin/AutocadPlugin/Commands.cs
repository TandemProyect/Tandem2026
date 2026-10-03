using System;
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

        [CommandMethod("MVCCONEXION", CommandFlags.Session)]
        public void AbrirFormulario()
        {
            PaletteHost.Show();
        }

        [CommandMethod("INSERTARBLOQUE", CommandFlags.Session)]
        public void InsertarBloque()
        {
            PaletteHost.ShowBlocks();
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

        [CommandMethod("LEERDISENOMVC", CommandFlags.Session)]
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
            ed.WriteMessage("\n          GENERAR3D, REGENERAR3D, TANDEM_ENCOFRAR, TANDEM_SALVAR, TANDEM_ABRIRDISENO, INSERTARBLOQUE, TANDEM_INSERTBLOQUE, TANDEM_CAMBIARBLOQUE,");
            ed.WriteMessage("\n          TANDEM_LOCAL, TANDEM_PRODUCCION, TANDEM_SERVIDOR,");
            ed.WriteMessage("\n          TANDEM_PROBAR_CONEXION, TANDEM_DEVICE_ID, TANDEM_CARGAR_MENU, ATDESING, UnAtdesing");
            ed.WriteMessage($"\nServidor MVC: {MvcServerSettings.CurrentLabel()} — {MvcServerSettings.CurrentUrl()}");
            ed.WriteMessage($"\nDLL: {System.Reflection.Assembly.GetExecutingAssembly().Location}\n");
        }

        [CommandMethod("TANDEM_PROBAR_CONEXION")]
        public void ProbarConexion()
        {
            Editor ed = GetEditor();
            if (ed == null) return;

            ed.WriteMessage("\n=== Tandem: Prueba de conexión MVC ===");
            ed.WriteMessage($"\nServidor: {MvcServerSettings.CurrentLabel()}");
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

        [CommandMethod("TANDEM_LOCAL", CommandFlags.Session)]
        public void UsarServidorLocal()
        {
            MvcServerSettings.SetLocal();
            SwitchServer("local (localhost:44384). Arranca Desing en IIS Express.");
        }

        [CommandMethod("TANDEM_PRODUCCION", CommandFlags.Session)]
        public void UsarServidorProduccion()
        {
            MvcServerSettings.SetProduction();
            SwitchServer("producción (tdesing.net).");
        }

        [CommandMethod("TANDEM_SERVIDOR")]
        public void MostrarServidor()
        {
            Editor ed = GetEditor();
            if (ed == null) return;
            ed.WriteMessage($"\nServidor MVC: {MvcServerSettings.CurrentLabel()}");
            ed.WriteMessage($"\nURL: {MvcServerSettings.CurrentUrl()}");
            ed.WriteMessage("\nTANDEM_LOCAL = IIS Express. TANDEM_PRODUCCION = tdesing.net.\n");
        }

        private void SwitchServer(string where)
        {
            Editor ed = GetEditor();
            ed?.WriteMessage($"\n[Tandem] Cambiado a {where} Recargando paletas…\n");
            PaletteHost.ReconnectToCurrentServer();
        }

        [CommandMethod("ATDESING", CommandFlags.Session)]
        public void InstallAtDesing()
        {
            PaletteHost.InstallLibrary();
        }

        [CommandMethod("UnAtdesing", CommandFlags.Session)]
        public void ResetDeveloperLocal()
        {
            Editor ed = GetEditor();
            if (ed != null)
            {
                var confirm = ed.GetString("\n[Tandem] Desinstala TDesing (menú, arranque automático, sesión y biblioteca). Escribe SI para confirmar: ");
                if (confirm.Status != PromptStatus.OK
                    || !string.Equals((confirm.StringResult ?? "").Trim(), "SI", StringComparison.OrdinalIgnoreCase))
                {
                    ed.WriteMessage("\n[Tandem] Reset cancelado.\n");
                    return;
                }
            }

            var report = PaletteHost.ResetDeveloperLocalState();
            ed = GetEditor();
            if (ed == null) return;
            ed.WriteMessage("\n[Tandem] Reset de desarrollador:\n" + report);
            ed.WriteMessage("[Tandem] Desinstalado. Cierra AutoCAD. NETLOAD para cargar el plugin. ATDESING para volver a instalar la biblioteca.\n");
        }

        [CommandMethod("TANDEM_DEVICE_ID")]
        public void MostrarDeviceId()
        {
            Editor ed = GetEditor();
            if (ed == null) return;
            ed.WriteMessage($"\nDeviceId actual: {PluginDeviceId.Current()}");
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
    }
}
