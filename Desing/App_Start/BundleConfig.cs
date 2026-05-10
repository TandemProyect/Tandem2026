using System.Web.Optimization;

namespace Desing
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                      "~/Scripts/jquery-3.4.1.js",
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/datatables/Buttons-2.0.1/js/dataTables.buttons.min.js",
                      "~/Scripts/datatables/JSZip-2.5.0/jszip.min.js",
                      "~/Scripts/datatables/pdfmake-0.1.36/pdfmake.min.js",
                      "~/Scripts/datatables/pdfmake-0.1.36/vfs_fonts.js",
                      "~/Scripts/datatables/Buttons-2.0.1/js/buttons.html5.min.js",
                      "~/Scripts/datatables/Buttons-2.0.1/js/buttons.print.min.js",
                      "~/Scripts/datatables/DataTables-1.11.3/js/dataTables.autoFill.min.js"
                      ));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));



            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/SiteView.css",
                      "~/Content/bootstrap.css",
                      "~/Content/Siteunited.css",
                      "~/Content/datatables/Buttons-2.0.1/js/css/buttons.dataTables.min.css",
                      "~/Content/datatables/DataTables-1.11.3/css/jquery.dataTables.min.css",
                      "~/Content/datatables/DataTables-1.11.3/css/css/autoFill.dataTables.min.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/assets/vendor").Include(
                         "~/assets/vendor/animate.css/animate.css",
                         "~/assets/vendor/aos/aos.css",
                         "~/assets/vendor/bootstrap/css/bootstrap.css",
                         "~/assets/vendor/bootstrap-icons/bootstrap-icons.css",
                         "~/assets/vendor/glightbox/css/glightbox.css",
                         "~/assets/vendor/swiper/swiper-bundle.min.css",
                         "~/assets/css/style.css",
                         "~/assets/vendor/animate.css/animate.min.css",
                         "~/assets/vendor/boxicons/css/boxicons.css"));

            bundles.Add(new ScriptBundle("~/bundles/DesignTools").Include(
                "~/Scripts/jquery-3.5.1.js",
                "~/Scripts/Design/3D/Design_3d_Setup.js",
                "~/Scripts/Design/3D/Design-3d-three.js",
                "~/Scripts/Design/3D/Design-3d-cameras.js",
                "~/Scripts/Three/background/dat.gui.min.js",
                "~/Scripts/Three/Three/three.js",
                "~/Scripts/Three/Three/OrbitControls.js",
                "~/Scripts/Three/Three/tween.umd.js",
                "~/Scripts/Three/background/Sky.js",
                "~/Scripts/Three/background/InfiniteGridHelper.js",
                "~/Scripts/Three/STLLoader.js",
                "~/Scripts/Three/Projector.js",
                "~/Scripts/Three/Three/OBJLoader.js",
                //Environment
                "~/Scripts/Design/3D/Design_3d_Environment.js",
                //Selection
                "~/Scripts/Design/3D/Selection/Design_3d_Selection.js",
                "~/Scripts/Design/3D/Selection/DesignOnDblclick.js",
                "~/Scripts/Design/3D/Selection/DesignMouseMove.js",
                "~/Scripts/Design/3D/Selection/DesignMouseMoveControl.js",
                //Action
                "~/Scripts/Design/3D/Design_3d_AddControl.js",
                "~/Scripts/Design/3D/Design_GetAndSet.js",
                "~/Scripts/Design/3D/Design_3d_Object.js",
                "~/Scripts/Design/3D/Wall/Design_3d_InsertWall.js",
                "~/Scripts/Design/3D/Wall/Design_3d_InsertCorner.js",
                "~/Scripts/Design/3D/Design_3d_Action.js",
                "~/Scripts/Design/3D/Design_3d_Action_EditForm.js",
                "~/Scripts/Design/Test/Test_Athos.js",
                "~/Scripts/Design/3D/Design_3d_MaterialList.js",
                "~/Scripts/Design/SaveDesign/SaveDesign.js",
                "~/Scripts/Design/3D/DrawAtk60/Design_DrawAtk60.js",
                "~/Scripts/Design/3D/Design_3d_MaterialExportCad.js",
                //Dim
                "~/Scripts/Design/3D/Design_3d_Object_Dim.js",
                //Insert Wall
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallWall_0.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallWall_90.js",
                //Insert Corner
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall10.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall30.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall50.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall60.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall70.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall20.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall80.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWall40.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallCornerWallX.js",
                "~/Scripts/Design/3D/Selection/MouseMove/AddWallParallels.js",
                //Validate
                "~/Scripts/Design/3D/Validate/Design_3d_Validate.js",
                 //Edit Corner
                 "~/Scripts/Design/3D/EditCorner/Design_3d_EditCorner_10.js",
                "~/Scripts/Design/3D/EditCorner/Design_3d_EditCorner_30.js",
                "~/Scripts/Design/3D/EditCorner/Design_3d_EditCorner_50.js",
                "~/Scripts/Design/3D/EditCorner/Design_3d_EditCorner_70.js",
                //ViewAndOpacity
                "~/Scripts/Design/3D/Design_3d_ViewOROpacity.js",
                //Conection
                "~/Scripts/Design/3D/WallConnection/WallConnection.js",
                "~/Scripts/Design/3D/WallConnection/CreateConection/CreateConection_90_00_L_70.js",
                //Help
                "~/Scripts/Design/3D/HelpDesing/HelpAction.js"
                ));
        }
    }
}
