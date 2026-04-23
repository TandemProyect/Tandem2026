using System;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;
using ZwcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(ZwcadPlugin.MenuManager))]

namespace ZwcadPlugin
{
    /// <summary>
    /// Gestor de menús personalizado para el plugin ZWCAD 2026 - MVC
    /// </summary>
    public class MenuManager : IExtensionApplication
    {
        /// <summary>
        /// Se ejecuta al cargar la DLL
        /// </summary>
        public void Initialize()
        {
            try
            {
                Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    Editor ed = doc.Editor;
                    ed.WriteMessage("\n╔════════════════════════════════════════════════════════════╗");
                    ed.WriteMessage("\n║   Plugin ZWCAD 2026 - Conexión MVC cargado exitosamente  ║");
                    ed.WriteMessage("\n╚════════════════════════════════════════════════════════════╝");
                    ed.WriteMessage("\nEscribe 'HOLA' para ver los comandos disponibles.");
                    ed.WriteMessage("\nEscribe 'MENUMVC' para mostrar el menú contextual.\n");
                }

                // Crear el menú personalizado
                CrearMenu();
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error al inicializar el plugin:\n{ex.Message}",
                    "Error de Inicialización",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Se ejecuta al descargar la DLL
        /// </summary>
        public void Terminate()
        {
            try
            {
                // Limpiar recursos si es necesario
                EliminarMenu();

                Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    Editor ed = doc.Editor;
                    ed.WriteMessage("\nPlugin ZWCAD 2026 - MVC descargado.\n");
                }
            }
            catch (System.Exception)
            {
                // Silenciar errores al descargar
            }
        }

        /// <summary>
        /// Crea el menú personalizado en la barra de menús de ZWCAD
        /// </summary>
        private void CrearMenu()
        {
            try
            {
                // Obtener la aplicación COM de ZWCAD
                object acadApp = ZwcadApp.AcadApplication;
                if (acadApp == null) return;

                // Usar reflexión tardía (late binding) para acceder al modelo COM
                Type acadType = acadApp.GetType();

                // Obtener MenuBar
                object menuBar = acadType.InvokeMember("MenuBar",
                    System.Reflection.BindingFlags.GetProperty,
                    null, acadApp, null);

                if (menuBar == null) return;

                Type menuBarType = menuBar.GetType();

                // Verificar si ya existe el menú "MVC Plugin"
                object menuGroups = menuBarType.InvokeMember("Item",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, menuBar, new object[] { "MVC Plugin" });

                // Si el menú ya existe, eliminarlo primero
                if (menuGroups != null)
                {
                    EliminarMenu();
                }

                // Crear nuevo menú "MVC Plugin"
                object[] args = new object[] { "MVC Plugin" };
                object nuevoMenu = menuBarType.InvokeMember("Add",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, menuBar, args);

                if (nuevoMenu == null) return;

                Type menuType = nuevoMenu.GetType();

                // Agregar elementos del menú
                AgregarItemMenu(menuType, nuevoMenu, "Formulario Principal", "MVCCONEXION", "Abre el formulario de gestión");
                AgregarSeparador(menuType, nuevoMenu);
                AgregarItemMenu(menuType, nuevoMenu, "Insertar Bloque", "INSERTARBLOQUE", "Inserta un bloque desde el servidor");
                AgregarSeparador(menuType, nuevoMenu);
                AgregarItemMenu(menuType, nuevoMenu, "Leer Diseño", "LEERDISENOMVC", "Lee un diseño desde el servidor MVC");
                AgregarItemMenu(menuType, nuevoMenu, "Guardar Diseño", "GUARDARDISENOMVC", "Guarda el diseño actual en el servidor");
                AgregarSeparador(menuType, nuevoMenu);
                AgregarItemMenu(menuType, nuevoMenu, "Ayuda", "HOLA", "Muestra la ayuda de comandos");

                // Mostrar el menú en la barra
                menuBarType.InvokeMember("InsertInMenuBar",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, nuevoMenu, new object[] { Type.Missing });
            }
            catch (System.Exception ex)
            {
                // Si falla la creación del menú, no detener la carga del plugin
                Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    Editor ed = doc.Editor;
                    ed.WriteMessage($"\nAdvertencia: No se pudo crear el menú personalizado: {ex.Message}\n");
                }
            }
        }

        /// <summary>
        /// Agrega un item al menú
        /// </summary>
        private void AgregarItemMenu(Type menuType, object menu, string label, string macro, string helpString)
        {
            try
            {
                // Obtener la colección de items del menú
                Type itemType;

                // Crear el item con el label
                object[] itemArgs = new object[] { label };
                object menuItem = menuType.InvokeMember("AddMenuItem",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, menu, itemArgs);

                if (menuItem == null) return;

                itemType = menuItem.GetType();

                // Establecer el macro (comando)
                itemType.InvokeMember("Macro",
                    System.Reflection.BindingFlags.SetProperty,
                    null, menuItem, new object[] { macro + "\n" });

                // Establecer el texto de ayuda
                itemType.InvokeMember("HelpString",
                    System.Reflection.BindingFlags.SetProperty,
                    null, menuItem, new object[] { helpString });
            }
            catch (System.Exception)
            {
                // Ignorar errores al agregar items individuales
            }
        }

        /// <summary>
        /// Agrega un separador al menú
        /// </summary>
        private void AgregarSeparador(Type menuType, object menu)
        {
            try
            {
                menuType.InvokeMember("AddSeparator",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, menu, new object[] { Type.Missing });
            }
            catch (System.Exception)
            {
                // Ignorar errores al agregar separadores
            }
        }

        /// <summary>
        /// Elimina el menú personalizado
        /// </summary>
        private void EliminarMenu()
        {
            try
            {
                object acadApp = ZwcadApp.AcadApplication;
                if (acadApp == null) return;

                Type acadType = acadApp.GetType();

                object menuBar = acadType.InvokeMember("MenuBar",
                    System.Reflection.BindingFlags.GetProperty,
                    null, acadApp, null);

                if (menuBar == null) return;

                Type menuBarType = menuBar.GetType();

                // Intentar obtener el menú
                try
                {
                    object menu = menuBarType.InvokeMember("Item",
                        System.Reflection.BindingFlags.InvokeMethod,
                        null, menuBar, new object[] { "MVC Plugin" });

                    if (menu != null)
                    {
                        // Eliminar el menú
                        Type menuType = menu.GetType();
                        menuType.InvokeMember("Delete",
                            System.Reflection.BindingFlags.InvokeMethod,
                            null, menu, null);
                    }
                }
                catch (System.Exception)
                {
                    // El menú no existe, no hacer nada
                }
            }
            catch (System.Exception)
            {
                // Ignorar errores al eliminar
            }
        }

        /// <summary>
        /// Comando para mostrar el menú contextual manualmente
        /// </summary>
        [CommandMethod("MENUMVC")]
        public void MostrarMenuContextual()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;

            ed.WriteMessage("\n╔════════════════════════════════════════════════════════════╗");
            ed.WriteMessage("\n║              MENÚ MVC PLUGIN - ZWCAD 2026                 ║");
            ed.WriteMessage("\n╚════════════════════════════════════════════════════════════╝");
            ed.WriteMessage("\n");
            ed.WriteMessage("\nOpciones del menú:");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  1. Formulario Principal   → MVCCONEXION");
            ed.WriteMessage("\n     Abre el formulario de gestión de bloques y diseños");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  2. Insertar Bloque        → INSERTARBLOQUE");
            ed.WriteMessage("\n     Inserta un bloque desde el servidor");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  3. Leer Diseño            → LEERDISENOMVC");
            ed.WriteMessage("\n     Lee un diseño desde el servidor MVC");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  4. Guardar Diseño         → GUARDARDISENOMVC");
            ed.WriteMessage("\n     Guarda el diseño actual en el servidor");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n  5. Ayuda                  → HOLA");
            ed.WriteMessage("\n     Muestra la ayuda de comandos disponibles");
            ed.WriteMessage("\n");
            ed.WriteMessage("\n──────────────────────────────────────────────────────────────");
            ed.WriteMessage("\nTambién puedes acceder al menú desde:");
            ed.WriteMessage("\n• La barra de menús: 'MVC Plugin'");
            ed.WriteMessage("\n• Escribiendo los comandos directamente");
            ed.WriteMessage("\n──────────────────────────────────────────────────────────────\n");

            // Solicitar opción
            PromptIntegerOptions pio = new PromptIntegerOptions("\nSelecciona una opción (1-5) o ESC para cancelar: ");
            pio.AllowNegative = false;
            pio.AllowZero = false;
            pio.LowerLimit = 1;
            pio.UpperLimit = 5;
            PromptIntegerResult pir = ed.GetInteger(pio);

            if (pir.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nOperación cancelada.\n");
                return;
            }

            // Ejecutar el comando correspondiente
            string comando = "";
            switch (pir.Value)
            {
                case 1:
                    comando = "MVCCONEXION";
                    break;
                case 2:
                    comando = "INSERTARBLOQUE";
                    break;
                case 3:
                    comando = "LEERDISENOMVC";
                    break;
                case 4:
                    comando = "GUARDARDISENOMVC";
                    break;
                case 5:
                    comando = "HOLA";
                    break;
            }

            if (!string.IsNullOrEmpty(comando))
            {
                ed.WriteMessage($"\nEjecutando comando: {comando}\n");
                doc.SendStringToExecute(comando + "\n", true, false, false);
            }
        }
    }
}
