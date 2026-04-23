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
    }
}