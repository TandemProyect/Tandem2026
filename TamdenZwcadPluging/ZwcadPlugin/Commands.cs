using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using ZwcadPlugin.Models;
using ZwcadPlugin.UI.Views;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
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
                if (!ValidarAccesoPlugin(ed)) return;
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
                if (!ValidarAccesoPlugin(ed)) return;
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
            if (!ValidarAccesoPlugin(ed)) return;

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
            if (!ValidarAccesoPlugin(ed)) return;

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
        /// Comando para seleccionar líneas y polilíneas y enviarlas al servidor MVC
        /// </summary>
        [CommandMethod("TANDEM_SELECCIONAR_LINEAS")]
        public void SeleccionarLineas()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Database db = doc.Database;
            Editor ed = doc.Editor;
            if (!ValidarAccesoPlugin(ed)) return;

            try
            {
                ed.WriteMessage("\n=== Seleccionar Líneas y Polilíneas ===");
                ed.WriteMessage("\nSeleccione todos los objetos del dibujo y presione [INTRO]...\n");

                // Solicitar selección de objetos al usuario
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nSeleccione objetos (líneas, polilíneas): ";
                pso.AllowDuplicates = false;

                PromptSelectionResult psr = ed.GetSelection(pso);

                if (psr.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\nOperación cancelada.\n");
                    return;
                }

                // Listas para almacenar líneas y polilíneas
                List<LineaDTO> lineas = new List<LineaDTO>();
                int totalLineas = 0;
                int totalPolilineas = 0;

                // Procesar selección
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    SelectionSet ss = psr.Value;
                    ed.WriteMessage($"\n{ss.Count} objetos seleccionados. Procesando...\n");

                    foreach (SelectedObject so in ss)
                    {
                        if (so == null) continue;

                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent == null) continue;

                        // Procesar Líneas
                        if (ent is Line linea)
                        {
                            lineas.Add(new LineaDTO
                            {
                                Tipo = "Line",
                                InicioX = linea.StartPoint.X,
                                InicioY = linea.StartPoint.Y,
                                InicioZ = linea.StartPoint.Z,
                                FinX = linea.EndPoint.X,
                                FinY = linea.EndPoint.Y,
                                FinZ = linea.EndPoint.Z,
                                Layer = linea.Layer,
                                Color = linea.Color.ToString(),
                                Longitud = linea.Length,
                                Vertices = null
                            });
                            totalLineas++;
                        }

                        // Procesar Polilíneas
                        else if (ent is Polyline pline)
                        {
                            List<PuntoDTO> vertices = new List<PuntoDTO>();

                            // Extraer todos los vértices de la polilínea
                            for (int i = 0; i < pline.NumberOfVertices; i++)
                            {
                                Point3d pt = pline.GetPoint3dAt(i);
                                vertices.Add(new PuntoDTO
                                {
                                    X = pt.X,
                                    Y = pt.Y,
                                    Z = pt.Z
                                });
                            }

                            // Para polilíneas, usar el primer y último vértice como inicio/fin
                            Point3d inicio = pline.GetPoint3dAt(0);
                            Point3d fin = pline.GetPoint3dAt(pline.NumberOfVertices - 1);

                            lineas.Add(new LineaDTO
                            {
                                Tipo = "Polyline",
                                InicioX = inicio.X,
                                InicioY = inicio.Y,
                                InicioZ = inicio.Z,
                                FinX = fin.X,
                                FinY = fin.Y,
                                FinZ = fin.Z,
                                Layer = pline.Layer,
                                Color = pline.Color.ToString(),
                                Longitud = pline.Length,
                                Vertices = vertices
                            });
                            totalPolilineas++;
                        }
                    }

                    tr.Commit();
                }

                // Mostrar resumen
                ed.WriteMessage($"\n--- Resumen de Selección ---");
                ed.WriteMessage($"\nLíneas encontradas: {totalLineas}");
                ed.WriteMessage($"\nPolilíneas encontradas: {totalPolilineas}");
                ed.WriteMessage($"\nTotal de geometría válida: {lineas.Count}");

                if (lineas.Count == 0)
                {
                    ed.WriteMessage("\n❌ No se encontraron líneas ni polilíneas en la selección.\n");
                    return;
                }

                // Preparar datos para enviar al servidor MVC
                var seleccionDTO = new SeleccionLineasDTO
                {
                    Lineas = lineas,
                    TotalSeleccionados = lineas.Count,
                    TotalLineas = totalLineas,
                    TotalPolilineas = totalPolilineas,
                    FechaSeleccion = DateTime.Now,
                    Usuario = Environment.UserName
                };

                ed.WriteMessage("\nEnviando datos al servidor MVC...\n");

                // 🔴 LOG CRÍTICO antes de enviar
                ed.WriteMessage($"\n🔴 DEBUG: Preparando envío de {lineas.Count} líneas al servidor...");
                ed.WriteMessage($"\n🔴 DEBUG: Usuario: {seleccionDTO.Usuario}");
                ed.WriteMessage($"\n🔴 DEBUG: Fecha: {seleccionDTO.FechaSeleccion}");

                // ============================================================
                // US-697 — FLUJO 2-PASOS:
                //   1ª llamada: detección con altura por defecto (2700 mm).
                //               Devuelve los conteos de esquinas L y muros rectos.
                //   2ª llamada: si el usuario cambia la altura en el formulario,
                //               se vuelve a llamar con la nueva altura para
                //               recalcular las extrusiones de capa ModelDesing.
                // ============================================================
                ed.WriteMessage("\n🔴 DEBUG: 1ª llamada (altura default 2700mm) para obtener conteos...\n");
                seleccionDTO.AlturaMuroMm = 2700;

                Task<ApiResponse<DeteccionEsquinasLDTO>> task = _apiService.EnviarLineasSeleccionadasAsync(seleccionDTO);
                ApiResponse<DeteccionEsquinasLDTO> respuesta = task.ConfigureAwait(false).GetAwaiter().GetResult();
                ed.WriteMessage($"\n🔴 DEBUG: Respuesta recibida. Éxito: {respuesta.Exito}\n");

                if (!respuesta.Exito || respuesta.Datos == null)
                {
                    ed.WriteMessage($"\n❌ Error: {respuesta.Mensaje}");
                    return;
                }

                int totalEsquinas = respuesta.Datos.TotalEsquinasDetectadas;
                int totalMuros    = respuesta.Datos.TotalMurosRectos;
                ed.WriteMessage($"\n📊 Detectados: {totalEsquinas} esquina(s) L, {totalMuros} muro(s) recto(s).\n");

                // Mostrar formulario con resumen + input de altura
                using (var formCfg = new FormularioConfigMuros(totalEsquinas, totalMuros, 2.70m))
                {
                    if (formCfg.ShowDialog() != DialogResult.OK)
                    {
                        ed.WriteMessage("\n⚠️ Operación cancelada por el usuario.\n");
                        return;
                    }

                    decimal alturaM = formCfg.AlturaMuroMetros;
                    double  alturaMm = (double)(alturaM * 1000m);
                    ed.WriteMessage($"\n📐 Altura confirmada: {alturaM:F2} m ({alturaMm} mm)\n");

                    // Si la altura difiere de la del primer cálculo → reconsultar
                    if (Math.Abs(alturaMm - 2700.0) > 0.01)
                    {
                        ed.WriteMessage("\n🔴 DEBUG: 2ª llamada con la altura del formulario...\n");
                        seleccionDTO.AlturaMuroMm = alturaMm;
                        Task<ApiResponse<DeteccionEsquinasLDTO>> task2 = _apiService.EnviarLineasSeleccionadasAsync(seleccionDTO);
                        respuesta = task2.ConfigureAwait(false).GetAwaiter().GetResult();
                        if (!respuesta.Exito || respuesta.Datos == null)
                        {
                            ed.WriteMessage($"\n❌ Error en 2ª llamada: {respuesta.Mensaje}");
                            return;
                        }
                    }
                }

                ed.WriteMessage($"\n✅ {respuesta.Mensaje}");
                DibujarResultado(doc, ed, respuesta.Datos);

                ed.WriteMessage("\n=== Proceso Completado ===\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n❌ Error: {ex.Message}");
                ed.WriteMessage($"\nDetalles: {ex.StackTrace}\n");
            }
        }

        private void DibujarResultado(Document doc, Editor ed, DeteccionEsquinasLDTO datos)
        {
            if (datos == null) return;
            Database db = doc.Database;

            if (datos.PuntosADibujar != null && datos.PuntosADibujar.Count > 0)
            {
                ed.WriteMessage($"\n\n=== Esquinas L Detectadas: {datos.TotalEsquinasDetectadas} ===");
                ed.WriteMessage($"\nDibujando {datos.PuntosADibujar.Count} puntos...\n");

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    int n = 0;
                    const double RADIO_DEFAULT      = 40.0;  // 20% del original (200 mm)
                    const double LADO_SEMI_DEFAULT  = 20.0;  // 20% del original (100 mm)
                    foreach (var punto in datos.PuntosADibujar)
                    {
                        // US-688 T1: ramificar por Forma — "Cuadrado" o "Circulo" (default)
                        if (punto.Forma == "Cuadrado")
                        {
                            double s = punto.Tamano > 0 ? punto.Tamano : LADO_SEMI_DEFAULT;
                            var cuadrado = new Polyline();
                            cuadrado.Layer = "0";
                            cuadrado.ColorIndex = punto.ColorIndex;
                            cuadrado.AddVertexAt(0, new Point2d(punto.X - s, punto.Y - s), 0, 0, 0);
                            cuadrado.AddVertexAt(1, new Point2d(punto.X + s, punto.Y - s), 0, 0, 0);
                            cuadrado.AddVertexAt(2, new Point2d(punto.X + s, punto.Y + s), 0, 0, 0);
                            cuadrado.AddVertexAt(3, new Point2d(punto.X - s, punto.Y + s), 0, 0, 0);
                            cuadrado.Closed = true;
                            btr.AppendEntity(cuadrado);
                            tr.AddNewlyCreatedDBObject(cuadrado, true);
                        }
                        else
                        {
                            double r = punto.Tamano > 0 ? punto.Tamano : RADIO_DEFAULT;
                            var circulo = new Circle(new Point3d(punto.X, punto.Y, punto.Z), Vector3d.ZAxis, r);
                            circulo.Layer = "0";
                            circulo.ColorIndex = punto.ColorIndex;
                            btr.AppendEntity(circulo);
                            tr.AddNewlyCreatedDBObject(circulo, true);
                        }
                        n++;
                    }
                    tr.Commit();
                    ed.WriteMessage($"\n✅ {n} marcadores dibujados (círculos + cuadrados)");
                }
            }

            if (datos.PolilineasADibujar != null && datos.PolilineasADibujar.Count > 0)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                    foreach (var nombreCapa in new[] { "ObjetoDB2d", "ModelDesing" })
                    {
                        if (!lt.Has(nombreCapa))
                        {
                            var capa = new LayerTableRecord { Name = nombreCapa };
                            lt.Add(capa);
                            tr.AddNewlyCreatedDBObject(capa, true);
                        }
                    }
                    int n = 0;
                    foreach (var poly in datos.PolilineasADibujar)
                    {
                        if (poly.Vertices == null || poly.Vertices.Count < 2) continue;
                        var lwp = new Polyline();
                        lwp.Layer = poly.Capa;
                        for (int i = 0; i < poly.Vertices.Count; i++)
                        {
                            var v = poly.Vertices[i];
                            lwp.AddVertexAt(i, new Point2d(v.X, v.Y), 0, 0, 0);
                        }
                        lwp.Closed = poly.Cerrada;
                        if (poly.AlturaExtrusion > 0) lwp.Thickness = poly.AlturaExtrusion;
                        btr.AppendEntity(lwp);
                        tr.AddNewlyCreatedDBObject(lwp, true);
                        n++;
                    }
                    tr.Commit();
                    ed.WriteMessage($"\n✅ {n} polilínea(s) dibujadas");
                }
            }

            if (datos.PuntosADibujar == null || datos.PuntosADibujar.Count == 0)
                ed.WriteMessage("\n⚠️ No se detectaron esquinas tipo L.");
        }

        private bool ValidarAccesoPlugin(Editor ed)
        {
            try
            {
                var request = new PluginAuthRequestDTO
                {
                    DeviceId = ObtenerDeviceId(),
                    MachineName = Environment.MachineName,
                    UsuarioWindows = Environment.UserName,
                    AspNetUserId = Environment.GetEnvironmentVariable("TANDEM_ASPNET_USER_ID"),
                    PluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
                };

                var response = Task.Run(() => _apiService.ValidarEquipoPluginAsync(request)).Result;
                if (response == null || !response.Exito || response.Datos == null)
                {
                    ed.WriteMessage("\n❌ No fue posible validar la licencia del equipo. Verifica conectividad/API.");
                    return false;
                }

                if (!response.Datos.Permitido)
                {
                    ed.WriteMessage("\n⛔ Plugin bloqueado para este equipo.");
                    ed.WriteMessage($"\nEstado: {response.Datos.Estado}");
                    ed.WriteMessage($"\nMotivo: {response.Datos.Motivo}\n");
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n❌ Error de autorización de plugin: {ex.Message}\n");
                return false;
            }
        }

        private static string ObtenerDeviceId()
        {
            var seed = $"{Environment.MachineName}|{Environment.UserName}|{Environment.UserDomainName}|{Environment.OSVersion}|{ObtenerMachineGuid()}";
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
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

        /// <summary>
        /// Comando de ayuda que muestra informaciÃ³n bÃ¡sica
        /// </summary>
        [CommandMethod("HOLA")]
        public void MostrarAyuda()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;
            if (!ValidarAccesoPlugin(ed)) return;

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
            ed.WriteMessage("\n  TANDEM_SELECCIONAR_LINEAS - Selecciona líneas y polilíneas");
            ed.WriteMessage("\n                              (En desarrollo)");
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
        /// <summary>
        /// Comando para analizar una imagen de plano y detectar esquinas L via GPT-4o
        /// </summary>
        [CommandMethod("TANDEM_ANALIZAR_IMAGEN")]
        public void AnalizarImagen()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Tandem: Analizar Imagen de Plano ===");

                string rutaImagen = null;
                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.Title = "Seleccionar imagen del plano";
                    dlg.Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                    dlg.FilterIndex = 1;
                    if (dlg.ShowDialog() == DialogResult.OK)
                        rutaImagen = dlg.FileName;
                }

                if (string.IsNullOrEmpty(rutaImagen))
                {
                    ed.WriteMessage("\nOperación cancelada.\n");
                    return;
                }

                ed.WriteMessage($"\nImagen seleccionada: {Path.GetFileName(rutaImagen)}");
                ed.WriteMessage("\nEnviando al servidor MVC para análisis con GPT-4o...");

                byte[] imagenBytes = File.ReadAllBytes(rutaImagen);
                var respuesta = Task.Run(() =>
                    _apiService.AnalizarImagenAsync(imagenBytes, Path.GetFileName(rutaImagen))
                ).Result;

                if (!respuesta.Exito)
                {
                    ed.WriteMessage($"\n❌ Error: {respuesta.Mensaje}\n");
                    return;
                }

                ed.WriteMessage($"\n✅ {respuesta.Mensaje}");

                // Reutilizar el mismo flujo de dibujo que TANDEM_SELECCIONAR_LINEAS
                DibujarResultado(doc, ed, respuesta.Datos);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n❌ Error: {ex.Message}\n");
            }
        }

        [CommandMethod("MVCPLUGIN_INIT", CommandFlags.Session)]
        public void InicializarPlugin()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Editor ed = doc.Editor;
                if (!ValidarAccesoPlugin(ed))
                {
                    ed.WriteMessage("\nPlugin cargado en modo bloqueado por autorización de equipo.\n");
                    return;
                }
                ed.WriteMessage("\nâ•”â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•—");
                ed.WriteMessage("\nâ•‘   Plugin ZWCAD 2026 - ConexiÃ³n MVC cargado exitosamente  â•‘");
                ed.WriteMessage("\nâ•šâ•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                ed.WriteMessage("\n");
                ed.WriteMessage("\nEscribe 'HOLA' para ver los comandos disponibles.\n");
            }
        }

        /// <summary>
        /// Muestra el DeviceId actual para alta en administración.
        /// </summary>
        [CommandMethod("TANDEM_DEVICE_ID")]
        public void MostrarDeviceId()
        {
            var doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;
            ed.WriteMessage($"\nDeviceId actual: {ObtenerDeviceId()}\n");
            ed.WriteMessage($"\nMachineName: {Environment.MachineName}\n");
        }
    }
}

