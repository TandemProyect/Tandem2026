using System;
using System.Collections.Generic;
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
        /// Comando para seleccionar líneas y polilíneas y enviarlas al servidor MVC
        /// </summary>
        [CommandMethod("TANDEM_SELECCIONAR_LINEAS")]
        public void SeleccionarLineas()
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Database db = doc.Database;
            Editor ed = doc.Editor;

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

                // Enviar al servidor de forma asíncrona
                ed.WriteMessage($"\n🔴 DEBUG: Llamando a _apiService.EnviarLineasSeleccionadasAsync...\n");
                Task<ApiResponse<DeteccionEsquinasLDTO>> task = _apiService.EnviarLineasSeleccionadasAsync(seleccionDTO);
                ed.WriteMessage($"\n🔴 DEBUG: Esperando respuesta del servidor...\n");
                ApiResponse<DeteccionEsquinasLDTO> respuesta = task.ConfigureAwait(false).GetAwaiter().GetResult();
                ed.WriteMessage($"\n🔴 DEBUG: Respuesta recibida. Éxito: {respuesta.Exito}\n");

                if (respuesta.Exito)
                {
                    ed.WriteMessage($"\n✅ Éxito: {respuesta.Mensaje}");

                    // Procesar esquinas L detectadas
                    if (respuesta.Datos != null && respuesta.Datos.PuntosADibujar != null && respuesta.Datos.PuntosADibujar.Count > 0)
                    {
                        ed.WriteMessage($"\n\n=== Esquinas L Detectadas: {respuesta.Datos.TotalEsquinasDetectadas} ===");
                        ed.WriteMessage($"\nDibujando {respuesta.Datos.PuntosADibujar.Count} puntos de referencia...\n");

                        // Dibujar círculos en ZWCAD para marcar los puntos de conexión
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            int puntosDibujados = 0;
                            double radioCirculo = 50.0; // Radio del círculo (ajustar según escala del dibujo)

                            foreach (var punto in respuesta.Datos.PuntosADibujar)
                            {
                                // Crear un círculo en ZWCAD
                                Point3d center = new Point3d(punto.X, punto.Y, punto.Z);
                                Circle circulo = new Circle(center, Vector3d.ZAxis, radioCirculo);

                                // Configurar color según el tipo de punto
                                circulo.Layer = "0"; // Layer por defecto

                                circulo.ColorIndex = punto.ColorIndex;

                                // Agregar el círculo al dibujo
                                btr.AppendEntity(circulo);
                                tr.AddNewlyCreatedDBObject(circulo, true);

                                puntosDibujados++;
                                string tipoColor = punto.TipoPunto?.ToLower() ?? "azul";
                                ed.WriteMessage($"\n  Círculo {puntosDibujados} ({tipoColor}): Centro ({punto.X:F2}, {punto.Y:F2}, {punto.Z:F2}), Radio: {radioCirculo}");
                            }

                            tr.Commit();
                            ed.WriteMessage($"\n\n✅ {puntosDibujados} círculos dibujados correctamente (azul=interior, rojo=exterior)");
                        }

                        // Mostrar información de cada esquina
                        if (respuesta.Datos.Esquinas != null && respuesta.Datos.Esquinas.Count > 0)
                        {
                            ed.WriteMessage($"\n\n--- Detalles de Esquinas ---");
                            for (int i = 0; i < respuesta.Datos.Esquinas.Count; i++)
                            {
                                var esquina = respuesta.Datos.Esquinas[i];
                                ed.WriteMessage($"\nEsquina {i + 1}:");
                                ed.WriteMessage($"\n  Vértice: ({esquina.Vertice.X:F2}, {esquina.Vertice.Y:F2})");
                                ed.WriteMessage($"\n  Ángulo: {esquina.Angulo:F2}°");
                                ed.WriteMessage($"\n  Líneas involucradas: [{esquina.IndiceLinea1}, {esquina.IndiceLinea2}]");
                            }
                        }
                    }
                    else
                    {
                        ed.WriteMessage($"\n⚠️ No se detectaron esquinas tipo L en la selección.");
                    }
                }
                else
                {
                    ed.WriteMessage($"\n❌ Error: {respuesta.Mensaje}");
                }

                ed.WriteMessage("\n=== Proceso Completado ===\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n❌ Error: {ex.Message}");
                ed.WriteMessage($"\nDetalles: {ex.StackTrace}\n");
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

