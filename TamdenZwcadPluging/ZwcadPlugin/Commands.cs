using System;
using System.Threading.Tasks;
using System.Windows.Interop;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwcadPlugin.Models;
using ZwcadPlugin.UI.Views;
using ZwSoft.ZwCAD.Runtime;
using ZwcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;


[assembly: CommandClass(typeof(ZwcadPlugin.Commands))]

namespace ZwcadPlugin
{
    public class Commands
    {
        private readonly MVCApiService _apiService;

        public Commands()
        {
            _apiService = new MVCApiService();
        }

        /// <summary>
        /// Comando principal que abre el formulario de conexión MVC
        /// </summary>
        [CommandMethod("MVCCONEXION")]
        public void AbrirFormulario()
        {
            Document doc = null;
            Editor ed = null;

            try
            {
                doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                ed = doc.Editor;
                ed.WriteMessage("\n=== Plugin ZWCAD 2026 - Tandem Muros/Encofrado ===");
                ed.WriteMessage("\nAbriendo ventana principal...\n");

                var window = new MainWindow();
                window.SetOwnerHandle(ZwcadApp.MainWindow.Handle);
                window.ShowDialog();
            }
            catch (System.Exception ex)
            {
                if (ed != null)
                    ed.WriteMessage($"\nError al abrir ventana: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Comando para insertar un bloque desde el servidor
        /// </summary>
        [CommandMethod("INSERTARBLOQUE")]
        public void InsertarBloque()
        {
            Document doc = null;
            Editor ed = null;

            try
            {
                doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                ed = doc.Editor;
                ed.WriteMessage("\n=== Insertar Bloque desde Servidor ===");
                ed.WriteMessage("\nAbriendo ventana principal...\n");

                var window = new MainWindow();
                window.SetOwnerHandle(ZwcadApp.MainWindow.Handle);
                window.ShowDialog();
            }
            catch (System.Exception ex)
            {
                if (ed != null)
                    ed.WriteMessage($"\nError: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Comando para leer un diseÃ±o desde el servidor
        /// </summary>
        [CommandMethod("LEERDISENOMVC")]
        public void LeerDisenoMVC()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Leer DiseÃ±o desde Servidor MVC ===\n");

                // Solicitar ID del diseÃ±o
                PromptIntegerOptions pio = new PromptIntegerOptions("\nIngresa el ID del diseÃ±o: ");
                pio.AllowNegative = false;
                pio.AllowZero = false;
                PromptIntegerResult pir = ed.GetInteger(pio);

                if (pir.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\nOperaciÃ³n cancelada.\n");
                    return;
                }

                int disenoId = pir.Value;
                ed.WriteMessage($"\nObteniendo diseÃ±o con ID {disenoId}...\n");

                // Ejecutar operaciÃ³n asÃ­ncrona con ConfigureAwait para evitar bloqueos
                Task<DisenoDTO> task = _apiService.ObtenerDisenoAsync(disenoId);
                DisenoDTO diseno = task.ConfigureAwait(false).GetAwaiter().GetResult();

                if (diseno != null)
                {
                    // Mostrar informaciÃ³n
                    ed.WriteMessage($"\n--- InformaciÃ³n del DiseÃ±o ---");
                    ed.WriteMessage($"\nID: {diseno.Id}");
                    ed.WriteMessage($"\nNombre: {diseno.Nombre}");
                    ed.WriteMessage($"\nDescripciÃ³n: {diseno.Descripcion}");
                    ed.WriteMessage($"\nUsuario: {diseno.Usuario}");
                    ed.WriteMessage($"\nFecha CreaciÃ³n: {diseno.FechaCreacion}");
                    ed.WriteMessage($"\nFecha ModificaciÃ³n: {diseno.FechaModificacion}");
                    ed.WriteMessage($"\nEntidades: {diseno.Entidades?.Count ?? 0}");
                    ed.WriteMessage($"\nBloques: {diseno.Bloques?.Count ?? 0}");
                    ed.WriteMessage($"\nLayers: {diseno.Layers?.Count ?? 0}");
                    ed.WriteMessage("\n--- Fin de la informaciÃ³n ---\n");
                }
                else
                {
                    ed.WriteMessage("\nNo se pudo obtener el diseÃ±o.\n");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError al leer diseÃ±o: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Comando para guardar el diseÃ±o actual en el servidor
        /// </summary>
        [CommandMethod("GUARDARDISENOMVC")]
        public void GuardarDisenoMVC()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Guardar DiseÃ±o en Servidor MVC ===\n");

                // Solicitar nombre del diseÃ±o
                PromptStringOptions psoNombre = new PromptStringOptions("\nIngresa el nombre del diseÃ±o: ");
                psoNombre.AllowSpaces = true;
                PromptResult prNombre = ed.GetString(psoNombre);

                if (prNombre.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\nOperaciÃ³n cancelada.\n");
                    return;
                }

                string nombre = prNombre.StringResult;

                // Solicitar descripciÃ³n (opcional)
                PromptStringOptions psoDesc = new PromptStringOptions("\nIngresa una descripciÃ³n (opcional): ");
                psoDesc.AllowSpaces = true;
                PromptResult prDesc = ed.GetString(psoDesc);
                string descripcion = prDesc.Status == PromptStatus.OK ? prDesc.StringResult : "";

                ed.WriteMessage("\nExtrayendo datos del dibujo...\n");

                // Extraer datos del dibujo actual
                var diseno = new DisenoDTO
                {
                    Nombre = nombre,
                    Descripcion = descripcion,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    Usuario = ZwcadHelper.ObtenerUsuarioActual(),
                    Entidades = ZwcadHelper.ExtraerEntidades(db),
                    Bloques = ZwcadHelper.ExtraerBloques(db),
                    Layers = ZwcadHelper.ExtraerLayers(db)
                };

                ed.WriteMessage($"\nEntidades extraÃ­das: {diseno.Entidades.Count}");
                ed.WriteMessage($"\nBloques extraÃ­dos: {diseno.Bloques.Count}");
                ed.WriteMessage($"\nLayers extraÃ­dos: {diseno.Layers.Count}");
                ed.WriteMessage("\nEnviando al servidor...\n");

                // Enviar al servidor con ConfigureAwait para evitar bloqueos
                Task<DisenoDTO> task = _apiService.CrearDisenoAsync(diseno);
                DisenoDTO disenoGuardado = task.ConfigureAwait(false).GetAwaiter().GetResult();

                ed.WriteMessage($"\nÂ¡Ã‰xito! DiseÃ±o guardado con ID: {disenoGuardado.Id}\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError al guardar diseÃ±o: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Comando de ayuda que muestra informaciÃ³n bÃ¡sica
        /// </summary>
        [CommandMethod("HOLA")]
        public void MostrarAyuda()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;

            ed.WriteMessage("\nâ•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—");
            ed.WriteMessage("\nâ•‘     Plugin ZWCAD 2026 - ConexiÃ³n MVC                      â•‘");
            ed.WriteMessage("\nâ•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
            ed.WriteMessage("\n");
            ed.WriteMessage("\nComandos disponibles:");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  MVCCONEXION       - Abre el formulario principal");
            ed.WriteMessage("\n                      (GestiÃ³n de bloques y diseÃ±os)");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  INSERTARBLOQUE    - Inserta un bloque desde el servidor");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  LEERDISENOMVC     - Lee un diseÃ±o desde el servidor");
            ed.WriteMessage("\n                      (Especifica el ID del diseÃ±o)");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  GUARDARDISENOMVC  - Guarda el diseÃ±o actual en el servidor");
            ed.WriteMessage("\n                      (Extrae todas las entidades del dibujo)");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  HOLA              - Muestra esta ayuda");
            ed.WriteMessage("\n");
            ed.WriteMessage("\nâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
            ed.WriteMessage("\nServidor MVC: http://ccvallecano-002-site1.rtempurl.com/");
            ed.WriteMessage("\nâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€\n");
        }

        /// <summary>
        /// Comando de inicializaciÃ³n que se ejecuta automÃ¡ticamente
        /// </summary>
        [CommandMethod("MVCPLUGIN_INIT", CommandFlags.Session)]
        public void InicializarPlugin()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Editor ed = doc.Editor;
                ed.WriteMessage("\nâ•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—");
                ed.WriteMessage("\nâ•‘   Plugin ZWCAD 2026 - ConexiÃ³n MVC cargado exitosamente  â•‘");
                ed.WriteMessage("\nâ•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                ed.WriteMessage("\n");
                ed.WriteMessage("\nEscribe 'HOLA' para ver los comandos disponibles.\n");
            }
        }
    }
}

