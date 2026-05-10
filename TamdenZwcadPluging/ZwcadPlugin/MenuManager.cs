using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

[assembly: ZwSoft.ZwCAD.Runtime.ExtensionApplication(typeof(ZwcadPlugin.MenuManager))]
[assembly: ZwSoft.ZwCAD.Runtime.CommandClass(typeof(ZwcadPlugin.MenuManager))]

namespace ZwcadPlugin
{
    /// <summary>
    /// Punto de entrada del plugin. Registra comandos ZWCAD.
    /// El menu Tandem2026.cui se carga manualmente con MENULOAD.
    /// </summary>
    public class MenuManager : ZwSoft.ZwCAD.Runtime.IExtensionApplication
    {
        public void Initialize()
        {
            try
            {
                Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                doc?.Editor.WriteMessage(
                    "\nTandem 2026 cargado. Use MENULOAD para cargar MNU\\Tandem2026.cui\n");
            }
            catch { }
        }

        public void Terminate() { }

        [ZwSoft.ZwCAD.Runtime.CommandMethod("TANDEM")]
        public void MostrarComandos()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            ed.WriteMessage("\n--- Tandem 2026 ---");
            ed.WriteMessage("\n  MVCCONEXION               Panel principal");
            ed.WriteMessage("\n  DETECTARMUROS             Detecta muros en la planta 2D");
            ed.WriteMessage("\n  GENERAR3D                 Genera solidos 3D");
            ed.WriteMessage("\n  REGENERAR3D               Borra y regenera solidos 3D");
            ed.WriteMessage("\n  CONFIGENCOFRADO           Configura el sistema de encofrado");
            ed.WriteMessage("\n  LEERDISENOMVC             Lee un diseno desde el servidor");
            ed.WriteMessage("\n  CREARDISENOMVC            Crea un diseno nuevo y lo guarda en MVC");
            ed.WriteMessage("\n  GUARDARDISENOMVC          Guarda el diseno en el servidor");
            ed.WriteMessage("\n  TANDEM_SELECCIONAR_LINEAS Selecciona lineas y polilineas\n");
        }
    }
}