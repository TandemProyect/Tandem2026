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
            catch (System.Exception ex)
            {
                var j = ex.Message;
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

                // ⭐ DETECCIÓN DE ESQUINAS TIPO L ⭐
                // US-697 — la altura del muro proviene del formulario del cliente (default 2700)
                var detector = new LCornerDetector();
                var deteccionEsquinas = detector.DetectarEsquinasL(seleccion.Lineas, seleccion.AlturaMuroMm);

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
                var lineas = await imageService.AnalizarImagenAsync(imagenBytes, file.ContentType);

                System.Diagnostics.Debug.WriteLine($"[ImageAnalysis] {lineas.Count} líneas extraídas de la imagen");

                var detector = new LCornerDetector();
                var resultado = detector.DetectarEsquinasL(lineas);
                resultado.Mensaje = $"Imagen analizada: {lineas.Count} líneas detectadas. {resultado.Mensaje}";

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
    }
}