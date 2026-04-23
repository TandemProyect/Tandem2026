using DataTables.Mvc;
using Desing.Models;
using System;
using System.Data.Entity;

using System.Linq;
using System.Linq.Dynamic.Core;

using System.Web.Mvc;


namespace Desing.Controllers
{
    public class HelpDesingController : BaseController
    {
        public ActionResult Principal()
        {

            return PartialView("_Principal");
        }
        public ActionResult MenuHelp()
        {
            return PartialView("_MenuHelp");
        }

        public JsonResult ListHelp([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<ModelHelp> query = from help in db.TSql_Help
                                              join helpGrup in db.TSql_HelpGrup on help.LinkHelpGroup equals helpGrup.IdObject into helpGrupJoin
                                              from helpGrup in helpGrupJoin.DefaultIfEmpty()
                                              select new ModelHelp
                                              {
                                                  SysObjectID = help.SysObjectID,
                                                  AddHelp = help.AddHelp,
                                                  AddHelpContent = help.AddHelpContent,
                                                  LinkVideo = help.LinkVideo,
                                                  AddIcon = helpGrup.AddIcon,
                                                  AddHelpGrup = helpGrup.AddHelpGrup,
                                                  AddImage = help.AddImage
                                              };

                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AddHelp.ToString().Contains(value) ||
                                             p.AddHelpContent.ToString().Contains(value)
                    );
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "AddHelp":
                            orderColumn = "AddHelp";
                            break;
                        case "AddHelpContent":
                            orderColumn = "AddHelpContent";
                            break;
                        default:
                            orderColumn = "AddHelp";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "AddHelp" ? "AddHelp" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                // Rights

                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",
                    AddImage = p.AddImage,
                    SysObjectID = p.SysObjectID,
                    AddHelp = p.AddHelp,
                    AddHelpContent = p.AddHelpContent,
                    TextCode = p.LinkVideo,
                    TextLabel = p.AddIcon,
                    NumberHigh = p.AddHelpGrup,
                    LinkVideo = "<a title='Tile' href='" + Url.Content("~/Employee/Edit/" + p.SysObjectID) + "' class=\"btn btn-default btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
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