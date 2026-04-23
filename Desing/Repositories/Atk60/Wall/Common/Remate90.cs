using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.Common
{
    public class Remate90 : BaseController
    {
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        private static long LastPanel = 0;
        private static string _codeName;
        internal static List<ModelRenderElement> setdListElement(long IsRemateX, long rest, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedElement(IsRemateX, rest, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        private static void SedElement(long IsRemateX, long rest, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            dataCordenadX = dataCordenadX + (30 - 2) + (IsRemateX / 10);

            ModelRenderElement elementWood = new ModelRenderElement();
            elementWood.ElementWood = "../../Content/DesignTools/Control/Cube.stl";
            elementWood.LongWood = rest / 10;
            elementWood.ParametFilter = rest;
            elementWood.CodeName = "Wood";
            elementWood.Filter = "Remate90";
            elementWood.x = dataCordenadX;
            elementWood.y = dataCordenadY;
            elementWood.z = 30;
            elementWood.heightWood = DataHeight;
            elementWood.XRotate = 90;
            ListRenderElement.Add(elementWood);


            int n = (int)((DataHeight + 299) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            var Elevation = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 45, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 135, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 225, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                    nHeight = nHeight + 270;
                    Elevation = Elevation + 270;
                }

            }
            if (restHeight > 0)
            {
                var RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 45, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 190, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                }
                if (RestTypeHeight == 1200)
                {
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 45, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                    CommonElement.SedUnionVerticalRegulable90(nHeight + 90, rest / 10, dataCordenadX, dataCordenadY, ListRenderElement);
                }
            }
        }

        private static int getRestTypeHeight(int restHeight)
        {
            if (restHeight > 0 && restHeight <= 1200)
            {
                return 1200;
            }
            if (restHeight > 1200 && restHeight <= 2400)
            {
                return 2400;
            }
            return 2700;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}