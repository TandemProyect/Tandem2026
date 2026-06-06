using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Desing.Models;
using System.Linq;
using System.Threading.Tasks;
using Desing.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Desing.Controllers
{
    public class DesignToolsAutocadController : BaseController
    {
        public ActionResult _SaveDwgFiles(string IdDesign, string NameDesign, IEnumerable<ImportBlock> ListMaterialExport)
        {
            try
            {
                string path = Server.MapPath("~/LibraryBlock/");

                if (Directory.Exists(path))
                {

                }
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
                var Name = NameDesign + "_" + Guid.NewGuid().ToString("N");
                var file = path + Name + ".dxf";
                DxfDocument doc = new DxfDocument();
                doc.Save(file);
                DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file);
                // netDxf is only compatible with AutoCad2000 and higher DXF versions
                if (dxfVersion < DxfVersion.AutoCad2000)
                {
                    return null;
                }
                DxfDocument loaded = DxfDocument.Load(file);
                foreach (var iten in ListMaterialExport)
                {
                    SendATK_Element(iten, doc);
                }
                doc.Save(file);
                //var j = BeginDownload(filed);
                var FileToDonload = "~/LibraryBlock/" + Name + ".dxf";
                var NameFile = Name + ".dxf";
                var fileContents = System.IO.File.ReadAllText(Server.MapPath(FileToDonload));
                return Json(new { data = true, ListMaterialExport, IsOk = true, fileContents, NameFile });
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public ActionResult DownloadFile(string filename)
        {
            WebClient webClient = new WebClient();
            byte[] myDataBuffer = webClient.DownloadData(filename);
            // Display the downloaded data.
            string download = Encoding.ASCII.GetString(myDataBuffer);
            Uri uri = new Uri(@"c:\atenco\myfile.dxf");
            webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " + "Windows NT 5.2; .NET CLR 1.0.3705;)");
            String newFile = filename;
            webClient.DownloadFileAsync(uri, @newFile);
            return null;
        }


        private void SendATK_Element(ImportBlock iten, DxfDocument doc)
        {
            string ATK_Panel = "";
            Block block = null;
            if (iten.NameBlock == "ATK_Braket")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Braket.dxf");
                block = new Block("ATK_Braket");
            }
            if (iten.NameBlock == "ATK_Panel27x30")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x30.dxf");
                block = new Block("ATK_Panel27x30");
            }
            if (iten.NameBlock == "ATK_Panel27x90")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x90.dxf");
                block = new Block("ATK_Panel27x90");
            }
            if (iten.NameBlock == "ATK_Panel27x75R")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x75R.dxf");
                block = new Block("ATK_Panel27x75R");
            }
            if (iten.NameBlock == "ATK_Union10443020")
            {
                return;

            }

            DxfDocument _AllEntityBlock = DxfDocument.Load(ATK_Panel);
            foreach (netDxf.Entities.Face3D Face3D in _AllEntityBlock.Entities.Faces3D) { netDxf.Entities.Face3D copy = (netDxf.Entities.Face3D)Face3D.Clone(); block.Entities.Add(copy); }
            foreach (netDxf.Entities.Polyline2D Polyline2D in _AllEntityBlock.Entities.Polylines2D) { netDxf.Entities.Polyline2D copyPolyline2D = (netDxf.Entities.Polyline2D)Polyline2D.Clone(); block.Entities.Add(copyPolyline2D); }
            foreach (netDxf.Entities.Circle Circle in _AllEntityBlock.Entities.Circles) { netDxf.Entities.Circle copyCircle = (netDxf.Entities.Circle)Circle.Clone(); block.Entities.Add(copyCircle); }
            foreach (netDxf.Entities.Point Point in _AllEntityBlock.Entities.Points)
            {
                netDxf.Entities.Point copyPoint = (netDxf.Entities.Point)Point.Clone();
                block.Entities.Add(copyPoint);
            }
            iten.z = (Convert.ToDouble(iten.z) * -1).ToString();
            switch (iten.Rotate_X)
            {
                case "270":
                    //iten.x = (Convert.ToDouble(iten.x) * -1).ToString();
                    iten.Rotate_X = "90";
                    break;
                case "90":
                    //iten.x = (Convert.ToDouble(iten.x) * -1).ToString();
                    iten.Rotate_X = "270";
                    break;
                case "0":
                    iten.Rotate_X = "0";
                    break;
                case "180":
                    iten.Rotate_X = "180";
                    break;
            }
            Insert insert = new Insert(block, new Vector3(Convert.ToDouble(iten.x) / 100, Convert.ToDouble(iten.z) / 100, (Convert.ToDouble(iten.y) / 100)));

            iten.Rotate_Z = (Convert.ToDouble(iten.Rotate_Z) * -1).ToString();
            insert.Rotation = Convert.ToDouble(iten.Rotate_X);
            insert.Layer = new Layer("ATK_Panel");
            //insert.Layer.Color.Index = 4;
            doc.Entities.Add(insert);
        }

        /// <summary>
        /// Procesa líneas y polilíneas enviadas desde ZWCAD
        /// </summary>
        /// <param name="seleccion">Datos de las líneas y polilíneas seleccionadas</param>
        /// <returns>Respuesta JSON con el resultado del procesamiento</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ProcesarLineasZwcad(SeleccionLineasDTO seleccion)
        {
            try
            {
                // 🔴 LOG CRÍTICO: Verificar que el endpoint se está ejecutando
                string logInicio = $"🔴🔴🔴 ENDPOINT LLAMADO: ProcesarLineasZwcad - {DateTime.Now:HH:mm:ss} 🔴🔴🔴";
                System.Diagnostics.Debug.WriteLine(logInicio);
                System.Console.WriteLine(logInicio);

                // Validar datos recibidos
                if (seleccion == null || seleccion.Lineas == null || seleccion.Lineas.Count == 0)
                {
                    string logError = "❌ ERROR: No se recibieron líneas para procesar";
                    System.Diagnostics.Debug.WriteLine(logError);
                    System.Console.WriteLine(logError);

                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "No se recibieron líneas para procesar",
                        Datos = null
                    });
                }

                // Log de información recibida
                System.Diagnostics.Debug.WriteLine($"=== Procesando líneas desde ZWCAD ===");
                System.Diagnostics.Debug.WriteLine($"Total líneas: {seleccion.TotalLineas}");
                System.Diagnostics.Debug.WriteLine($"Total polilíneas: {seleccion.TotalPolilineas}");
                System.Diagnostics.Debug.WriteLine($"Usuario: {seleccion.Usuario}");
                System.Diagnostics.Debug.WriteLine($"Fecha: {seleccion.FechaSeleccion}");

                // Procesar las líneas (estadísticas básicas)
                var estadisticas = new
                {
                    TotalProcesadas = seleccion.Lineas.Count,
                    Lineas = seleccion.Lineas.Where(l => l.Tipo == "Line").Count(),
                    Polilineas = seleccion.Lineas.Where(l => l.Tipo == "Polyline").Count(),
                    LongitudTotal = seleccion.Lineas.Sum(l => l.Longitud),
                    Layers = seleccion.Lineas.Select(l => l.Layer).Distinct().ToList(),
                    FechaProcesamiento = DateTime.Now
                };

                // Normalizar geometría de entrada para evitar residuos decimales
                // que generan conexiones/paneles espurios en el detector.
                var lineasNormalizadas = NormalizarLineasEntrada(seleccion.Lineas);

                // ⭐ DETECCIÓN DE ESQUINAS TIPO L ⭐
                // US-697 — la altura del muro proviene del formulario del cliente (default 2700)
                var detector = new LCornerDetector();
                var deteccionEsquinas = detector.DetectarEsquinasL(lineasNormalizadas, seleccion.AlturaMuroMm);

                System.Diagnostics.Debug.WriteLine($"=== Detección de Esquinas L ===");
                System.Diagnostics.Debug.WriteLine($"Esquinas detectadas: {deteccionEsquinas.TotalEsquinasDetectadas}");
                System.Diagnostics.Debug.WriteLine($"Puntos a dibujar: {deteccionEsquinas.PuntosADibujar.Count}");

                // Agregar información detallada de cada esquina al log
                for (int i = 0; i < deteccionEsquinas.Esquinas.Count; i++)
                {
                    var esquina = deteccionEsquinas.Esquinas[i];
                    System.Diagnostics.Debug.WriteLine($"  Esquina {i + 1}: Vértice ({esquina.Vertice.X:F2}, {esquina.Vertice.Y:F2}) - Ángulo: {esquina.Angulo:F2}° - Líneas: [{esquina.IndiceLinea1}, {esquina.IndiceLinea2}]");
                }

                // Guardar en sesión para uso posterior
                Session["UltimaSeleccionLineas"] = seleccion;
                Session["ResultadoProcesamiento"] = estadisticas;
                Session["EsquinasDetectadas"] = deteccionEsquinas;

                System.Diagnostics.Debug.WriteLine($"Procesamiento completado: {estadisticas.TotalProcesadas} geometrías");

                // Devolver respuesta con información de esquinas y puntos a dibujar
                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = true,
                    Mensaje = $"Se procesaron {estadisticas.TotalProcesadas} geometrías ({estadisticas.Lineas} líneas, {estadisticas.Polilineas} polilíneas). {deteccionEsquinas.Mensaje}",
                    Datos = deteccionEsquinas
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesando líneas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = false,
                    Mensaje = $"Error al procesar líneas: {ex.Message}",
                    Datos = null
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> DetectarEsquinasImagen()
        {
            try
            {
                if (Request.Files.Count == 0)
                    return Json(new ApiResponse<DeteccionEsquinasLDTO> { Exito = false, Mensaje = "No se recibió ninguna imagen" });

                var file = Request.Files[0];
                var imagenBytes = new byte[file.ContentLength];
                file.InputStream.Read(imagenBytes, 0, file.ContentLength);

                var imageService = new ImageAnalysisService();
                var (lineasSimples, espesorMuro, alturaMuro) = await imageService.AnalizarImagenAsync(imagenBytes, file.ContentType);

                System.Diagnostics.Debug.WriteLine($"[ImageAnalysis] {lineasSimples.Count} líneas extraídas, espesor: {espesorMuro}, altura: {alturaMuro}");

                if (lineasSimples == null || lineasSimples.Count == 0)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "No se detectaron líneas de muro en la imagen. Asegúrate de que los muros estén dibujados con trazo claro y sin demasiado ruido.",
                        Datos = null
                    });
                }

                if (espesorMuro == null)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "Falta la cota de espesor. Añade una etiqueta de espesor junto a un muro usando E/e (ej: E 0,30 o e=0.30) y vuelve a intentarlo.",
                        Datos = null
                    });
                }

                // Modo boceto: preparación suave (cotas/vertices ya vienen ordenados; no fusionar ni filtrar).
                var lineasEje = SketchWallBuilder.PrepararLineasEjeBoceto(lineasSimples);
                if (lineasEje.Count == 0)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "Tras filtrar cotas y ruido no quedaron tramos de muro. Revisa que el boceto tenga solo líneas gruesas de perímetro.",
                        Datos = null
                    });
                }

                // Fase 1: solo perímetro exterior en planta (espesor/altura en pasos posteriores).
                var resultado = SketchWallBuilder.ConstruirBocetoSoloEje(lineasEje);
                resultado.LineasEje = lineasEje;

                SketchWallBuilder.ObtenerBoundsPublico(lineasEje, out _, out _, out double maxX, out double maxY);
                resultado.Mensaje =
                    $"Imagen analizada ({file.FileName}): {lineasEje.Count} tramos, " +
                    $"{maxX / 1000.0:0.##}×{maxY / 1000.0:0.##} m. " +
                    $"E={espesorMuro.Value * 1000:0} mm. " +
                    resultado.Mensaje;

                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = true,
                    Mensaje = resultado.Mensaje,
                    Datos = resultado
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = false,
                    Mensaje = $"Error al analizar imagen: {ex.Message}",
                    Datos = null
                });
            }
        }

        /// <summary>
        /// Valida si un equipo puede ejecutar el plugin según la tabla de autorización de dispositivos.
        /// Espera tabla dbo.TSql_PluginDeviceAuth con columna DeviceId y (opcionalmente) LinAspNetUsert.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ValidarEquipoPlugin(PluginAuthRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = false,
                    Mensaje = "Solicitud inválida: DeviceId requerido.",
                    Datos = new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "SolicitudInvalida",
                        Motivo = "DeviceId requerido",
                        DeviceId = request?.DeviceId
                    }
                });
            }

            try
            {
                var result = ValidarEquipoEnSql(request);
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = true,
                    Mensaje = result.Permitido ? "Equipo autorizado." : "Equipo no autorizado.",
                    Datos = result
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = false,
                    Mensaje = $"Error validando equipo: {ex.Message}",
                    Datos = new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "Error",
                        Motivo = ex.Message,
                        DeviceId = request.DeviceId
                    }
                });
            }
        }

        private PluginAuthResultDTO ValidarEquipoEnSql(PluginAuthRequestDTO request)
        {
            var cnn = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cnn))
                throw new InvalidOperationException("ConnectionString IdentityConnection no configurada.");

            using (var cn = new SqlConnection(cnn))
            {
                cn.Open();

                if (!ExisteTabla(cn, "dbo", "TSql_PluginDeviceAuth"))
                {
                    return new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "TablaNoExiste",
                        Motivo = "No existe dbo.TSql_PluginDeviceAuth. Ejecuta primero el script SQL.",
                        DeviceId = request.DeviceId
                    };
                }

                var columnas = ObtenerColumnas(cn, "dbo", "TSql_PluginDeviceAuth");
                if (!columnas.Contains("DeviceId"))
                {
                    return new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "EsquemaInvalido",
                        Motivo = "La tabla de autorización no tiene columna DeviceId.",
                        DeviceId = request.DeviceId
                    };
                }

                string userColumn = null;
                foreach (var c in new[] { "LinAspNetUsert", "AspNetUserId", "UserId" })
                {
                    if (columnas.Contains(c))
                    {
                        userColumn = c;
                        break;
                    }
                }

                var sql = "SELECT TOP 1 * FROM dbo.TSql_PluginDeviceAuth WHERE DeviceId = @DeviceId";
                if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                    sql += $" AND ({userColumn} = @AspNetUserId OR {userColumn} IS NULL OR {userColumn} = '')";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", request.DeviceId);
                    if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                        cmd.Parameters.AddWithValue("@AspNetUserId", request.AspNetUserId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            return new PluginAuthResultDTO
                            {
                                Permitido = false,
                                Estado = "NoRegistrado",
                                Motivo = "Equipo no registrado/autorizado.",
                                DeviceId = request.DeviceId
                            };
                        }

                        bool permitido = true;
                        string estado = "Activo";
                        string motivo = "OK";

                        if (TieneColumna(rd, "Allowed") && rd["Allowed"] != DBNull.Value)
                            permitido = Convert.ToBoolean(rd["Allowed"]);
                        else if (TieneColumna(rd, "IsActive") && rd["IsActive"] != DBNull.Value)
                            permitido = Convert.ToBoolean(rd["IsActive"]);

                        if (TieneColumna(rd, "IsRevoked") && rd["IsRevoked"] != DBNull.Value && Convert.ToBoolean(rd["IsRevoked"]))
                        {
                            permitido = false;
                            estado = "Revocado";
                            motivo = "Equipo revocado por administración.";
                        }

                        if (TieneColumna(rd, "AttIsDeleted") && rd["AttIsDeleted"] != DBNull.Value && Convert.ToBoolean(rd["AttIsDeleted"]))
                        {
                            permitido = false;
                            estado = "Eliminado";
                            motivo = "Registro de equipo marcado como eliminado.";
                        }

                        if (TieneColumna(rd, "Estado") && rd["Estado"] != DBNull.Value)
                        {
                            var estadoRaw = rd["Estado"].ToString();
                            if (!string.IsNullOrWhiteSpace(estadoRaw))
                            {
                                estado = estadoRaw;
                                var s = estadoRaw.Trim().ToLowerInvariant();
                                if (s == "2" || s == "3" || s.Contains("revoc") || s.Contains("bloq") || s.Contains("inactiv"))
                                {
                                    permitido = false;
                                    motivo = "Estado de equipo bloqueado/revocado.";
                                }
                            }
                        }

                        if (!permitido && motivo == "OK")
                            motivo = "Equipo deshabilitado por política.";

                        rd.Close();
                        ActualizarHeartbeat(cn, request, columnas, userColumn);

                        return new PluginAuthResultDTO
                        {
                            Permitido = permitido,
                            Estado = estado,
                            Motivo = motivo,
                            DeviceId = request.DeviceId
                        };
                    }
                }
            }
        }

        private static void ActualizarHeartbeat(SqlConnection cn, PluginAuthRequestDTO request, HashSet<string> columnas, string userColumn)
        {
            var setParts = new List<string>();
            if (columnas.Contains("LastCheckUtc")) setParts.Add("LastCheckUtc = @NowUtc");
            if (columnas.Contains("AttLastModification")) setParts.Add("AttLastModification = @NowUtc");
            if (columnas.Contains("MachineName")) setParts.Add("MachineName = @MachineName");
            if (columnas.Contains("UsuarioWindows")) setParts.Add("UsuarioWindows = @UsuarioWindows");
            if (columnas.Contains("PluginVersion")) setParts.Add("PluginVersion = @PluginVersion");
            if (setParts.Count == 0) return;

            var sql = $"UPDATE dbo.TSql_PluginDeviceAuth SET {string.Join(", ", setParts)} WHERE DeviceId = @DeviceId";
            if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                sql += $" AND ({userColumn} = @AspNetUserId OR {userColumn} IS NULL OR {userColumn} = '')";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@NowUtc", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@DeviceId", request.DeviceId);
                cmd.Parameters.AddWithValue("@MachineName", (object)(request.MachineName ?? string.Empty));
                cmd.Parameters.AddWithValue("@UsuarioWindows", (object)(request.UsuarioWindows ?? string.Empty));
                cmd.Parameters.AddWithValue("@PluginVersion", (object)(request.PluginVersion ?? string.Empty));
                if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                    cmd.Parameters.AddWithValue("@AspNetUserId", request.AspNetUserId);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool ExisteTabla(SqlConnection cn, string schema, string table)
        {
            const string sql = @"
SELECT COUNT(1)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = @Schema
  AND TABLE_NAME = @Table
  AND TABLE_TYPE = 'BASE TABLE'";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Schema", schema);
                cmd.Parameters.AddWithValue("@Table", table);
                var n = Convert.ToInt32(cmd.ExecuteScalar());
                return n > 0;
            }
        }

        private static HashSet<string> ObtenerColumnas(SqlConnection cn, string schema, string table)
        {
            const string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @Schema
  AND TABLE_NAME = @Table";

            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Schema", schema);
                cmd.Parameters.AddWithValue("@Table", table);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        cols.Add(rd.GetString(0));
                }
            }
            return cols;
        }

        private static bool TieneColumna(SqlDataReader rd, string columnName)
        {
            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (string.Equals(rd.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private sealed class OffsetLineWork
        {
            public int SourceIndex { get; set; }
            public LineaDTO CenterLine { get; set; }
            public LineaDTO ExteriorLine { get; set; }
            public LineaDTO InteriorLine { get; set; }
        }

        /// <summary>
        /// Usa las líneas detectadas como cara EXTERIOR (cota de referencia)
        /// y genera la cara INTERIOR desplazada E hacia el centro del conjunto.
        /// </summary>
        private static List<LineaDTO> ExpandirLineasCentroACaras(List<LineaDTO> lineasCentro, double espesorMuroMetros)
        {
            var resultado = new List<LineaDTO>();
            if (lineasCentro == null || lineasCentro.Count == 0)
                return resultado;

            // E total hacia adentro desde la cara exterior.
            double espesorMm = espesorMuroMetros * 1000.0;
            var trabajos = new List<OffsetLineWork>();
            var centroide = CalcularCentroide(lineasCentro);

            for (int index = 0; index < lineasCentro.Count; index++)
            {
                var linea = lineasCentro[index];
                double dx = linea.FinX - linea.InicioX;
                double dy = linea.FinY - linea.InicioY;
                double longitud = Math.Sqrt((dx * dx) + (dy * dy));
                if (longitud < 0.001)
                    continue;

                // Normal perpendicular (izquierda del vector dirección)
                double nx = -dy / longitud;
                double ny = dx / longitud;

                // Elegimos la normal que apunta hacia el interior (más cerca del centroide).
                double mx = (linea.InicioX + linea.FinX) / 2.0;
                double my = (linea.InicioY + linea.FinY) / 2.0;
                double c1x = mx + (nx * espesorMm);
                double c1y = my + (ny * espesorMm);
                double c2x = mx - (nx * espesorMm);
                double c2y = my - (ny * espesorMm);
                double d1 = Distancia2d(c1x, c1y, centroide.X, centroide.Y);
                double d2 = Distancia2d(c2x, c2y, centroide.X, centroide.Y);

                double ox = d1 <= d2 ? nx * espesorMm : -nx * espesorMm;
                double oy = d1 <= d2 ? ny * espesorMm : -ny * espesorMm;

                trabajos.Add(new OffsetLineWork
                {
                    SourceIndex = index,
                    CenterLine = linea,
                    ExteriorLine = new LineaDTO
                    {
                        Tipo = "Line",
                        InicioX = linea.InicioX,
                        InicioY = linea.InicioY,
                        InicioZ = linea.InicioZ,
                        FinX = linea.FinX,
                        FinY = linea.FinY,
                        FinZ = linea.FinZ,
                        Layer = "ObjetoDB2d",
                        Color = "8",
                        Longitud = longitud,
                        Vertices = null
                    },
                    InteriorLine = new LineaDTO
                    {
                        Tipo = "Line",
                        InicioX = linea.InicioX + ox,
                        InicioY = linea.InicioY + oy,
                        InicioZ = linea.InicioZ,
                        FinX = linea.FinX + ox,
                        FinY = linea.FinY + oy,
                        FinZ = linea.FinZ,
                        Layer = "ObjetoDB2d",
                        Color = "8",
                        Longitud = longitud,
                        Vertices = null
                    }
                });
            }

            AjustarEncuentrosInteriores(trabajos, lineasCentro.Count);

            foreach (var t in trabajos)
            {
                SnapLinea(t.ExteriorLine);
                SnapLinea(t.InteriorLine);
                t.ExteriorLine.Longitud = Distancia2d(t.ExteriorLine.InicioX, t.ExteriorLine.InicioY, t.ExteriorLine.FinX, t.ExteriorLine.FinY);
                t.InteriorLine.Longitud = Distancia2d(t.InteriorLine.InicioX, t.InteriorLine.InicioY, t.InteriorLine.FinX, t.InteriorLine.FinY);
                resultado.Add(t.ExteriorLine);
                resultado.Add(t.InteriorLine);
            }

            return resultado;
        }

        private static void AjustarEncuentrosInteriores(List<OffsetLineWork> trabajos, int totalLineasCentro)
        {
            if (trabajos == null || trabajos.Count == 0 || totalLineasCentro < 2)
                return;

            const double TOLERANCIA_ENCUENTRO_MM = 200.0;
            var map = new Dictionary<string, OffsetLineWork>();
            foreach (var t in trabajos)
                map[$"{t.SourceIndex}"] = t;

            for (int i = 0; i < totalLineasCentro; i++)
            {
                for (int j = i + 1; j < totalLineasCentro; j++)
                {
                    var ci = map.ContainsKey($"{i}") ? map[$"{i}"].CenterLine : null;
                    var cj = map.ContainsKey($"{j}") ? map[$"{j}"].CenterLine : null;
                    if (ci == null || cj == null)
                        continue;

                    if (!TryGetSharedEndpoint(ci, cj, TOLERANCIA_ENCUENTRO_MM, out bool iEsInicio, out bool jEsInicio))
                        continue;

                    if (!map.TryGetValue($"{i}", out var wi) || !map.TryGetValue($"{j}", out var wj))
                        continue;

                    if (!TryIntersectInfiniteLines(wi.InteriorLine, wj.InteriorLine, out double ix, out double iy))
                        continue;

                    AplicarPuntoExtremo(wi.InteriorLine, iEsInicio, ix, iy);
                    AplicarPuntoExtremo(wj.InteriorLine, jEsInicio, ix, iy);
                }
            }
        }

        private static PuntoDTO CalcularCentroide(List<LineaDTO> lineas)
        {
            double sx = 0;
            double sy = 0;
            int n = 0;

            foreach (var l in lineas)
            {
                sx += l.InicioX + l.FinX;
                sy += l.InicioY + l.FinY;
                n += 2;
            }

            if (n == 0)
                return new PuntoDTO { X = 0, Y = 0, Z = 0 };

            return new PuntoDTO { X = sx / n, Y = sy / n, Z = 0 };
        }

        private static bool TryGetSharedEndpoint(LineaDTO a, LineaDTO b, double tol, out bool aEsInicio, out bool bEsInicio)
        {
            aEsInicio = true;
            bEsInicio = true;
            double best = double.MaxValue;
            bool ok = false;

            double d;

            d = Distancia2d(a.InicioX, a.InicioY, b.InicioX, b.InicioY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = true;
                bEsInicio = true;
                ok = true;
            }

            d = Distancia2d(a.InicioX, a.InicioY, b.FinX, b.FinY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = true;
                bEsInicio = false;
                ok = true;
            }

            d = Distancia2d(a.FinX, a.FinY, b.InicioX, b.InicioY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = false;
                bEsInicio = true;
                ok = true;
            }

            d = Distancia2d(a.FinX, a.FinY, b.FinX, b.FinY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = false;
                bEsInicio = false;
                ok = true;
            }

            return ok;
        }

        private static bool TryIntersectInfiniteLines(LineaDTO a, LineaDTO b, out double ix, out double iy)
        {
            ix = 0;
            iy = 0;

            double x1 = a.InicioX, y1 = a.InicioY, x2 = a.FinX, y2 = a.FinY;
            double x3 = b.InicioX, y3 = b.InicioY, x4 = b.FinX, y4 = b.FinY;

            double den = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            if (Math.Abs(den) < 1e-9)
                return false;

            ix = (((x1 * y2) - (y1 * x2)) * (x3 - x4) - (x1 - x2) * ((x3 * y4) - (y3 * x4))) / den;
            iy = (((x1 * y2) - (y1 * x2)) * (y3 - y4) - (y1 - y2) * ((x3 * y4) - (y3 * x4))) / den;
            return true;
        }

        private static void AplicarPuntoExtremo(LineaDTO l, bool extremoInicio, double x, double y)
        {
            if (extremoInicio)
            {
                l.InicioX = x;
                l.InicioY = y;
            }
            else
            {
                l.FinX = x;
                l.FinY = y;
            }
        }

        private static double Distancia2d(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// Normaliza líneas/polilíneas de entrada con snap a milímetro entero.
        /// Se aplica antes del detector para estabilizar encuentros entre extremos.
        /// </summary>
        private static List<LineaDTO> NormalizarLineasEntrada(List<LineaDTO> lineas)
        {
            var resultado = new List<LineaDTO>();
            if (lineas == null || lineas.Count == 0)
                return resultado;

            foreach (var origen in lineas)
            {
                if (origen == null)
                    continue;

                var copia = new LineaDTO
                {
                    Tipo = origen.Tipo,
                    InicioX = Snap(origen.InicioX),
                    InicioY = Snap(origen.InicioY),
                    InicioZ = origen.InicioZ,
                    FinX = Snap(origen.FinX),
                    FinY = Snap(origen.FinY),
                    FinZ = origen.FinZ,
                    Layer = origen.Layer,
                    Color = origen.Color,
                    Vertices = null
                };

                if (origen.Vertices != null && origen.Vertices.Count > 0)
                {
                    copia.Vertices = origen.Vertices
                        .Select(v => new PuntoDTO
                        {
                            X = Snap(v.X),
                            Y = Snap(v.Y),
                            Z = v.Z,
                            TipoPunto = v.TipoPunto,
                            ColorIndex = v.ColorIndex,
                            Forma = v.Forma,
                            Tamano = v.Tamano
                        })
                        .ToList();
                }

                if (copia.Vertices != null && copia.Vertices.Count >= 2)
                {
                    var inicio = copia.Vertices[0];
                    var fin = copia.Vertices[copia.Vertices.Count - 1];
                    copia.InicioX = inicio.X;
                    copia.InicioY = inicio.Y;
                    copia.InicioZ = inicio.Z;
                    copia.FinX = fin.X;
                    copia.FinY = fin.Y;
                    copia.FinZ = fin.Z;
                }

                copia.Longitud = Distancia2d(copia.InicioX, copia.InicioY, copia.FinX, copia.FinY);
                resultado.Add(copia);
            }

            return resultado;
        }

        /// <summary>
        /// Redondea coordenadas para evitar residuos numéricos del cálculo geométrico.
        /// </summary>
        private static void SnapLinea(LineaDTO linea)
        {
            if (linea == null) return;
            linea.InicioX = Snap(linea.InicioX);
            linea.InicioY = Snap(linea.InicioY);
            linea.FinX = Snap(linea.FinX);
            linea.FinY = Snap(linea.FinY);
        }

        private static double Snap(double value)
        {
            // Snap a milímetro entero para eliminar residuos acumulados de intersecciones.
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Agrega al resultado las líneas detectadas en imagen como polilíneas 2D
        /// para que el cliente ZWCAD las dibuje y el usuario visualice exactamente
        /// qué geometría interpretó el analizador.
        /// </summary>
        private static void AgregarLineasDetectadasDesdeImagen(
            DeteccionEsquinasLDTO resultado,
            List<LineaDTO> lineasSimples)
        {
            if (resultado == null || lineasSimples == null || lineasSimples.Count == 0)
                return;

            // Si el flujo común ya generó polilíneas de salida, evitamos duplicar geometría.
            if (resultado.PolilineasADibujar != null && resultado.PolilineasADibujar.Count > 0)
                return;

            if (resultado.PolilineasADibujar == null)
                resultado.PolilineasADibujar = new List<PolilineaDTO>();

            foreach (var linea in lineasSimples)
            {
                var vertices = new List<PuntoDTO>();

                if (linea.Vertices != null && linea.Vertices.Count >= 2)
                {
                    foreach (var v in linea.Vertices)
                    {
                        vertices.Add(new PuntoDTO
                        {
                            X = v.X,
                            Y = v.Y,
                            Z = v.Z
                        });
                    }
                }
                else
                {
                    vertices.Add(new PuntoDTO
                    {
                        X = linea.InicioX,
                        Y = linea.InicioY,
                        Z = linea.InicioZ
                    });
                    vertices.Add(new PuntoDTO
                    {
                        X = linea.FinX,
                        Y = linea.FinY,
                        Z = linea.FinZ
                    });
                }

                if (vertices.Count < 2)
                    continue;

                resultado.PolilineasADibujar.Add(new PolilineaDTO
                {
                    Vertices = vertices,
                    Cerrada = false,
                    Capa = "ObjetoDB2d",
                    ColorIndex = 8,
                    AlturaExtrusion = 0
                });
            }
        }
    }
}