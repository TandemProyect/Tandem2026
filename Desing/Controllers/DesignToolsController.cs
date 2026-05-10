using DAL;
using DataTables.Mvc;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Web.Mvc;
using static SendMail.Models;
namespace Desing.Controllers
{
    public class DesignToolsController : BaseController
    {
        List<ModelLeven> ListData = new List<ModelLeven>();
        public object ListRenderElement { get; private set; }
        public object ListRenderElementData { get; private set; }
        public int coordinateEndWall_X = 0;
        public int coordinateEndWall_Y = 0;
        public ActionResult _MaterialListGrup(string id, IEnumerable<TemporalList> list)
        {
            try
            {
                if (list == null)
                {
                    return Json(new { data = false, list, IsOk = false });
                };

                //using (var trans = db.Database.BeginTransaction())
                //{

                List<TemporalList> ListData = new List<TemporalList>();
                foreach (var iten in list)
                {
                    if (iten.AtkCode == "Atk60_")
                    {
                        continue;
                    }
                    if (iten.AtkCode == "Atk60_null")
                    {
                        continue;
                    }
                    if (iten.AtkCode.Substring(6) != "")
                    {
                        TemporalList Code = new TemporalList
                        {
                            AtkCode = iten.AtkCode.Substring(6),
                            AtkGrup = iten.AtkGrup,
                        };
                        ListData.Add(Code);
                    }
                }
                //trans.Commit();
                //}
                Session["SessionListMaterial"] = ListData;
                return Json(new { data = true, list, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult _MaterialList(long id, IEnumerable<TemporalList> list)
        {
            try
            {
                if (list == null)
                {
                    return Json(new { data = false, list, IsOk = false });
                };

                //using (var trans = db.Database.BeginTransaction())
                //{
                TSql_Design Design = db.TSql_Design.FirstOrDefault(x => x.SysObjectID == id);
                TSql_Employee Employe = db.TSql_Employee.FirstOrDefault(x => x.LinAspNetUsert == Design.LinCreatedBy);
                Session["IdDesing"] = id;
                Session["AddNameDesing"] = Design.AttLabel;
                Session["user"] = Employe.AttName;



                List<TemporalList> ListData = new List<TemporalList>();
                foreach (var iten in list)
                {
                    if (iten.AtkCode == "Atk60_")
                    {
                        continue;
                    }
                    if (iten.AtkCode == "Atk60_null")
                    {
                        continue;
                    }
                    if (iten.AtkCode.Substring(6) != "")
                    {
                        TemporalList Code = new TemporalList
                        {
                            AtkCode = iten.AtkCode.Substring(6),
                            AtkGrup = iten.AtkGrup,
                        };
                        ListData.Add(Code);
                    }
                }
                //trans.Commit();
                //}
                Session["SessionListMaterial"] = ListData;
                return Json(new { data = true, list, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult Cut()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult MaterialList()
        {
            try
            {
                ViewBag.ListData = "";
                return PartialView("_MaterialList");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Form_DesignEdit()
        {
            try
            {
                ViewBag.ListData = "";
                return PartialView("~/Views/DesignTools/Form/_Form_DesignEdit.cshtml");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Form_DesignMenu()
        {
            try
            {
                ViewBag.ListData = "";
                return PartialView("~/Views/DesignTools/Form/_Form_DesignMenu.cshtml");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }


        public ActionResult Form_DesignGeneral()
        {
            try
            {
                ViewBag.ListData = "";
                return PartialView("~/Views/DesignTools/Form/_Form_DesignGeneral.cshtml");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }



        public JsonResult ListMaterialJsom([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                List<TemporalList> ListData = new List<TemporalList>();
                ListData = (List<TemporalList>)Session["SessionListMaterial"];

                //testear errorres
                //foreach (var iten in ListData)
                //{
                //    var j = db.Tsql_Master_Articles.FirstOrDefault(x => x.TextCode == iten.AtkCode);
                //    if (j == null)
                //    {
                //        var jj = 1;
                //    }
                //}



                string UserId = User.Identity.GetUserId();
                var MaterialToSend = 10000;
                var queryList = from t in ListData
                                join m in db.Tsql_Master_Articles on t.AtkCode equals m.TextCode into mGroup
                                from m in mGroup.DefaultIfEmpty()
                                select new ListMaterial
                                {
                                    TextCode = m.AddAtenkoCode,
                                    TextLabel = m.TextLabel,
                                    NumberWeight = (double)m.NumberWeight,
                                    NumberMts2 = (double)m.NumberMts2,
                                    Quantity = 1
                                };

                var q = queryList.ToList();

                var queryable = q.AsQueryable();

                IQueryable<ListMaterial> query = queryable
                            .GroupBy(x => new { x.TextCode, x.TextLabel })
                            .Select(x => new ListMaterial
                            {
                                TextCode = x.Key.TextCode,
                                TextLabel = x.Key.TextLabel,
                                NumberWeight = x.Min(y => y.NumberWeight),
                                TotalWeight = x.Min(y => y.NumberWeight) * x.Sum(y => 1),
                                NumberMts2 = x.Min(y => y.NumberMts2),
                                TotalNumberMts2 = x.Min(y => y.NumberMts2) * x.Sum(y => 1),
                                Quantity = x.Sum(y => 1)
                            });

                var totalCount = query.Count();
                var TotalElement = query.Sum(n => n.Quantity);
                var TotalMts2 = query.Sum(n => n.TotalNumberMts2);
                var TotalWeight = query.Sum(n => n.TotalWeight);
                long IdDesing = (long)Session["IdDesing"];
                var AddNameDesing = Session["AddNameDesing"];
                var UserName = Session["user"];
                SedMailDesingModel SendMailModel = new SedMailDesingModel
                {
                    TotalElement = TotalElement,
                    TotalWeight = TotalWeight,
                    IdDesing = IdDesing,
                    AddNameDesing = (string)AddNameDesing,
                    UserName = (string)UserName,
                    Type = " Nuevo "
                };
                TSql_TotalListsDesing TotalListsDesing = db.TSql_TotalListsDesing.FirstOrDefault(x => x.LinkDesing == IdDesing);
                var CheckTotalWeight = 0.0;
                if (TotalListsDesing != null)
                {
                    CheckTotalWeight = TotalListsDesing.AddTotalWeight;
                }
                if (TotalListsDesing == null)
                {
                    TSql_TotalListsDesing totalListsDesing = new TSql_TotalListsDesing
                    {
                        AddTotalArticles = TotalElement,
                        AddTotalWeight = (long)TotalWeight,
                        AddTotalM2 = 0,
                        LinkDesing = IdDesing,
                        AddNumberSendMail = 1,
                        LinkMadeBy = UserId,
                        AddDateMade = DateTime.UtcNow,
                        AddChangeBy = UserId,
                        AddLastDateChange = DateTime.UtcNow,
                    };
                    db.TSql_TotalListsDesing.Add(totalListsDesing);
                    db.SaveChanges();
                    TempData.Clear();
                    if (TotalWeight > MaterialToSend)
                    {
                        SendMail(SendMailModel);
                    }
                }
                else
                {
                    if (CheckTotalWeight < TotalWeight)
                    {
                        db.TSql_TotalListsDesing.Remove(TotalListsDesing);
                        TSql_TotalListsDesing totalListsDesing = new TSql_TotalListsDesing
                        {
                            AddTotalArticles = TotalElement,
                            AddTotalWeight = (long)TotalWeight,
                            AddTotalM2 = 0,
                            LinkDesing = IdDesing,
                            AddNumberSendMail = 1,
                            LinkMadeBy = UserId,
                            AddDateMade = DateTime.UtcNow,
                            AddChangeBy = UserId,
                            AddLastDateChange = DateTime.UtcNow,
                        };
                        db.TSql_TotalListsDesing.Add(totalListsDesing);
                        db.SaveChanges();
                        TempData.Clear();
                        SendMailModel.Type = " Actualización ";
                        SendMail(SendMailModel);
                    }

                }

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.TextCode.Contains(value) ||
                                             p.TextLabel.Contains(value) ||
                                             p.Quantity.ToString().Contains(value) ||
                                             p.NumberWeight.ToString().Contains(value) ||
                                             p.NumberMts2.ToString().Contains(value) ||
                                             p.TotalWeight.ToString().Contains(value) ||
                                             p.TotalNumberMts2.ToString().Contains(value)

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
                        case "TextCode":
                            orderColumn = "TextCode";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "Quantity":
                            orderColumn = "Quantity";
                            break;
                        case "NumberWeight":
                            orderColumn = "NumberWeight";
                            break;
                        case "NumberMts2":
                            orderColumn = "NumberMts2";
                            break;
                        case "TotalWeight":
                            orderColumn = "TotalWeight";
                            break;
                        case "TotalNumberMts2":
                            orderColumn = "TotalNumberMts2";
                            break;
                        default:
                            orderColumn = "TextCode";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "TextCode" ? "TextCode" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);


                bool allowEdit = true;
                bool allowDelete = true;
                var data = query.ToList().Select(p => new
                {
                    TotalElement = TotalElement.ToString("0"),
                    TotalMts2 = TotalMts2.ToString("0.00"),
                    TTotalWeight = TotalWeight.ToString("0.00"),
                    emptyColumn = "",
                    TextCode = p.TextCode,
                    TextLabel = p.TextLabel,
                    Quantity = p.Quantity.ToString("0"),
                    NumberWeight = p.NumberWeight.ToString("0.00"),
                    NumberMts2 = p.NumberMts2.ToString("0.00"),
                    TotalWeight = p.TotalWeight.ToString("0.00"),
                    TotalNumberMts2 = p.TotalNumberMts2.ToString("0.00"),
                    allowEdit = allowEdit,
                    allowDelete = allowDelete,
                    buttonSelect = "<a id=BtnV_" + p.TextCode + " title='Seleccionar artículo y oculta el resto' href='javascript:SelectArtiquel(" + p.TextCode + ");'                                class=\"btn btn-danger btn-xs\"><span title=\"HOLA\" class=\"fas fa-solid fa-check\" aria-hidden=\"true\"></span></a>",
                    buttonSelectOpacity = "<a id=Btn_" + p.TextCode + " title='Selecionar artículo y poner opaciadad del 50% al resto' href='javascript:SelectOpacityArtiquel(" + p.TextCode + ");'   class=\"btn btn-warning btn-xs\"><span class=\"fas fa-solid fa-check\" aria-hidden=\"true\"></span></a>",
                    buttonView = "<a id=Btn_" + p.TextCode + " title='Visualizar o no el artículo' href='javascript:OpacityArtiquel(" + p.TextCode + ");'                                             class=\"btn btn-danger btn-xs\"><span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>",
                    buttonViewOpacity = "<a id=BtnV_" + p.TextCode + " title='Opacidad al 50% o no el cartículo' href='javascript:ViewArtiquel(" + p.TextCode + ");'                                  class=\"btn btn-warning btn-xs\"><span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>",
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private void SendMail(SedMailDesingModel sendMailModel)
        {

            if (ModelState.IsValid)
            {
                MailModel Model = new MailModel();
                Model.To = "juan.godoy@vscad.com";
                //Model.To = "gerardo.ansin@atenko.com.uy";
                Model.From = "admin@atenko.net";
                Model.Subject = sendMailModel.Type + " E diseño : " + sendMailModel.IdDesing + " con nombre : " + sendMailModel.AddNameDesing + " Supero en peso";

                Model.Body = @"E diseño : " + sendMailModel.IdDesing + " con nombre : " + sendMailModel.AddNameDesing + " Supero en peso "
                    + "el diseño fue realizado por " + sendMailModel.UserName + " su peso es de: " + sendMailModel.TotalWeight + " y tiene " + sendMailModel.TotalElement + " de articulos";

                MailMessage mail = new MailMessage();
                mail.To.Add(Model.To);
                mail.From = new MailAddress(Model.From);
                mail.Subject = Model.Subject;
                string Body = Model.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "mail5005.smarterasp.net";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("admin@atenko.net", "AngelyJuan01@");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }

        }

        public ActionResult MaterialListGrup()
        {
            try
            {
                ViewBag.ListData = "";
                return PartialView("_MaterialListGrup");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public JsonResult ListMaterialGrupJsom([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                List<TemporalList> ListData = new List<TemporalList>();
                ListData = (List<TemporalList>)Session["SessionListMaterial"];
                string UserId = User.Identity.GetUserId();

                var queryList = from t in ListData
                                join m in db.Tsql_Master_Articles on t.AtkCode equals m.TextCode into mGroup
                                from m in mGroup.DefaultIfEmpty()
                                select new ListMaterial
                                {
                                    AddGrup = t.AtkGrup,
                                    TextCode = t.AtkCode,
                                    TextLabel = m.TextLabel,
                                    NumberWeight = (double)m.NumberWeight,
                                    NumberMts2 = (double)m.NumberMts2,
                                    Quantity = 1
                                };



                var q = queryList.ToList();
                var queryable = q.AsQueryable();


                IQueryable<ListMaterial> query = queryable
                           .GroupBy(x => new { x.AddGrup })
                           .Select(x => new ListMaterial
                           {
                               AddGrup = x.Key.AddGrup,
                               TotalWeight = x.Min(y => y.NumberWeight) * x.Sum(y => 1),
                               TotalNumberMts2 = x.Min(y => y.NumberMts2) * x.Sum(y => 1),
                               Quantity = x.Sum(y => 1)
                           });

                var totalCount = query.Count();



                // Execute the LINQ query
                var result = query.ToList();
                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AddGrup.Contains(value) ||
                                             p.Quantity.ToString().Contains(value) ||
                                             p.TotalWeight.ToString().Contains(value) ||
                                             p.TotalNumberMts2.ToString().Contains(value)
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
                        case "AddGrup":
                            orderColumn = "AddGrup";
                            break;
                        case "TotalWeight":
                            orderColumn = "TotalWeight";
                            break;
                        case "TotalNumberMts2":
                            orderColumn = "TotalNumberMts2";
                            break;
                        case "Quantity":
                            orderColumn = "Quantity";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "AddGrup" ? "AddGrup" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",
                    AddGrup = p.AddGrup,
                    TotalWeight = p.TotalWeight.ToString("0.00"),
                    TotalNumberMts2 = p.TotalNumberMts2.ToString("0.00"),
                    Quantity = p.Quantity.ToString("0"),
                    //buttonView = "<a id=BtnShow_" + p.AddGrup + " title='Ocular' href='javascript:ShwoGrup();' class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye-slash\" aria-hidden=\"true\"></span></a>",
                    buttonView = "<a id=BtnShow_" + p.AddGrup + " title='Ocular' href='javascript:ShwoGrup(" + p.AddGrup + ");' class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye-slash\" aria-hidden=\"true\"></span></a>",
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult MaterialListGrupDetail(long Id)
        {
            try
            {
                ViewBag.Grup = Id;
                TempData.Clear();
                TempData["GrupListMaterial"] = Id;
                ViewBag.ListData = "";
                return PartialView("_MaterialListGrupDetail");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        public JsonResult ListMaterialGrupJsomDetails([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var Id = TempData["GrupListMaterial"].ToString();
                TempData.Clear();
                List<TemporalList> ListData = new List<TemporalList>();
                ListData = (List<TemporalList>)Session["SessionListMaterial"];
                var contl = ListData.Count();

                var LisDataWhere = ListData.Where(p => p.AtkGrup.Contains(Id));
                var contl2 = LisDataWhere.Count();

                string UserId = User.Identity.GetUserId();
                var queryList = from temporalList in LisDataWhere
                                join masterArticle in db.Tsql_Master_Articles on temporalList.AtkCode equals masterArticle.TextCode into temp
                                from t in temp.DefaultIfEmpty()
                                select new ListMaterial
                                {
                                    TextCode = t.TextCode,
                                    TextLabel = t.TextLabel,
                                    NumberWeight = (double)t.NumberWeight,
                                    NumberMts2 = (double)t.NumberMts2,
                                    Quantity = 1
                                };
                var q = queryList.ToList();
                var queryable = q.AsQueryable();

                IQueryable<ListMaterial> query = queryable
                            .GroupBy(x => new { x.TextCode, x.TextLabel })
                            .Select(x => new ListMaterial
                            {
                                TextCode = x.Key.TextCode,
                                TextLabel = x.Key.TextLabel,
                                NumberWeight = x.Min(y => y.NumberWeight),
                                TotalWeight = x.Min(y => y.NumberWeight) * x.Sum(y => 1),
                                NumberMts2 = x.Min(y => y.NumberMts2),
                                TotalNumberMts2 = x.Min(y => y.NumberMts2) * x.Sum(y => 1),
                                Quantity = x.Sum(y => 1)
                            });

                var totalCount = query.Count();

                var TotalElement = query.Sum(n => n.Quantity);
                var TotalMts2 = query.Sum(n => n.TotalNumberMts2);
                var TotalWeight = query.Sum(n => n.TotalWeight);

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.TextCode.Contains(value) ||
                                             p.TextLabel.Contains(value) ||
                                             p.Quantity.ToString().Contains(value) ||
                                             p.NumberWeight.ToString().Contains(value) ||
                                             p.NumberMts2.ToString().Contains(value) ||
                                             p.TotalWeight.ToString().Contains(value) ||
                                             p.TotalNumberMts2.ToString().Contains(value)

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
                        case "TextCode":
                            orderColumn = "TextCode";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "Quantity":
                            orderColumn = "Quantity";
                            break;
                        case "NumberWeight":
                            orderColumn = "NumberWeight";
                            break;
                        case "NumberMts2":
                            orderColumn = "NumberMts2";
                            break;
                        case "TotalWeight":
                            orderColumn = "TotalWeight";
                            break;
                        case "TotalNumberMts2":
                            orderColumn = "TotalNumberMts2";
                            break;
                        default:
                            orderColumn = "TextCode";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "TextCode" ? "TextCode" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "name asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                // Rights
                bool allowEdit = true;
                bool allowDelete = true;
                var data = query.ToList().Select(p => new
                {
                    TotalElement = TotalElement.ToString("0"),
                    TotalMts2 = TotalMts2.ToString("0.00"),
                    TTotalWeight = TotalWeight.ToString("0.00"),
                    emptyColumn = "",
                    TextCode = p.TextCode,
                    TextLabel = p.TextLabel,
                    Quantity = p.Quantity.ToString("0"),
                    NumberWeight = p.NumberWeight.ToString("0.00"),
                    NumberMts2 = p.NumberMts2.ToString("0.00"),
                    TotalWeight = p.TotalWeight.ToString("0.00"),
                    TotalNumberMts2 = p.TotalNumberMts2.ToString("0.00"),
                    allowEdit = allowEdit,
                    allowDelete = allowDelete,
                    buttonEdit = "<a id=Btn_" + p.TextCode + " title='Visualizar articulo' href='javascript:OpacityArtiquel(" + p.TextCode + ");' class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>",
                    buttonView = "<a id=BtnV_" + p.TextCode + " title='Ocular'             href='javascript:ViewArtiquel(" + p.TextCode + ");'    class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye-slash\" aria-hidden=\"true\"></span></a>",
                    button = "<a id=Btn3_" + p.TextCode + " title='Visualizar articulo'     href='javascript:OpacityArtiquel(" + p.TextCode + ");' class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye\" aria-hidden=\"true\"></span></a>",
                    button1 = "<a id=BtnV4_" + p.TextCode + " title='Ocular'                href='javascript:ViewArtiquel(" + p.TextCode + ");'    class=\"btn btn-default btn-xs\"><span class=\"fas fa-eye-slash\" aria-hidden=\"true\"></span></a>",
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }




        public ActionResult _Delete(long Id)
        {
            try
            {
                Session["IDDesign"] = Id;
                return Json(new { data = Id, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }
        public ActionResult Delete()
        {
            try
            {
                var Id = Session["IDDesign"];
                Session["IDDesign"] = null;
                TSql_Design DesignEntity = db.TSql_Design.Find(Id);
                return PartialView("_Delete", DesignEntity);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                string UserId = User.Identity.GetUserId();
                TSql_Design DesignEntity = db.TSql_Design.Find(id);
                DesignEntity.AttIsDeleted = true;
                DesignEntity.LinModifiedBy = UserId;
                DesignEntity.AttChange = DateTime.UtcNow;
                DesignEntity.SysUpdateNumber++;
                db.SaveChanges();
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = "Eliminar diseño";
                TempData["ToastMessage"] = "El diseño " + DesignEntity.AttLabel + " a sido eliminado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }
        //Edit
        public ActionResult _Edit(long Id)
        {
            try
            {
                Session["IDDesign"] = Id;
                return Json(new { data = true, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }
        public ActionResult Edit()
        {
            try
            {
                var Id = Session["IDDesign"];
                Session["IDDesign"] = null;
                TSql_Design DesignEntity = db.TSql_Design.Find(Id);
                ConfigurarVistaCreateEditParaEdicion(DesignEntity);
                return PartialView("_CreateEdit");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        // POST: Group/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TSql_Design model)
        {
            try
            {
                if (model.AttLabel == null)
                {
                    ModelState.AddModelError(nameof(model.AttLabel), "Insertar nombre del diseño");
                }
                if (ModelState.IsValid)
                {
                    string UserId = User.Identity.GetUserId();
                    TSql_Design Design = db.TSql_Design.FirstOrDefault(x => x.SysObjectID == model.SysObjectID);
                    Design.AttLabel = model.AttLabel;
                    Design.AttDescription = model.AttDescription;
                    Design.LinModifiedBy = User.Identity.GetUserId();
                    Design.SysUpdateNumber = Design.SysUpdateNumber + 1;
                    db.SaveChanges();
                    TempData.Clear();
                    TempData["ToastType"] = "Act";
                    TempData["ToastTitle"] = "Crear diseño";
                    TempData["ToastMessage"] = "El diseño " + model.AttLabel + " a sido creado correctamente";
                    return RedirectToAction("Index");
                }
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }





        public ActionResult Create()
        {
            try
            {
                ConfigurarVistaCreateEditParaAlta();
                return PartialView("_CreateEdit");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [AllowAnonymous]
        public ActionResult CreateFromZwcad(string deviceId, int? created = null)
        {
            try
            {
                var auth = ValidarDispositivoZwcad(deviceId);
                if (!auth.IsValid)
                {
                    return new HttpStatusCodeResult(403, "Equipo no autorizado para crear diseños.");
                }

                ConfigurarVistaCreateEditParaAlta();
                ViewBag.ZwcadMode = true;
                ViewBag.ZwcadDeviceId = auth.DeviceId;
                ViewBag.ZwcadCreated = (created ?? 0) == 1;
                return View();
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult _Create()
        {
            try
            {
                return Json(new { data = true, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        // POST: Group/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create(ModelDesign3d model, string zwcadDeviceId = null)
        {
            try
            {
                if (model.DesignName == null)
                {
                    ModelState.AddModelError(nameof(model.DesignName), "Insertar nombre del diseño");
                }
                if (ModelState.IsValid)
                {
                    string UserId = null;
                    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        UserId = User.Identity.GetUserId();
                    }
                    else
                    {
                        var auth = ValidarDispositivoZwcad(zwcadDeviceId);
                        if (!auth.IsValid || string.IsNullOrWhiteSpace(auth.UserId))
                        {
                            return new HttpStatusCodeResult(403, "Equipo no autorizado para crear diseños.");
                        }
                        UserId = auth.UserId;
                    }

                    TSql_Design newDesign = new TSql_Design
                    {
                        AttLabel = model.DesignName,
                        AttDescription = model.AttDescription,
                        AttCenterX = 0,
                        AttCenterY = 0,
                        AttCreated = DateTime.UtcNow,
                        AttIsDeleted = false,
                        AttActiveCameraType = 1,
                        LinCreatedBy = UserId,
                        LinModifiedBy = UserId,
                        ItIsShared = true,
                        ItIsSharedMyGrup = true,
                        SysUpdateNumber = 1,
                        AttChange = DateTime.UtcNow,
                    };
                    db.TSql_Design.Add(newDesign);
                    db.SaveChanges();
                    TempData.Clear();
                    TempData["ToastType"] = "Act";
                    TempData["ToastTitle"] = "Crear diseño";
                    TempData["ToastMessage"] = "El diseño " + model.DesignName + " a sido creado correctamente";


                    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index");
                    }

                    return RedirectToAction("CreateFromZwcad", new { deviceId = zwcadDeviceId, created = 1 });
                }
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Index(bool? OnlyUser)
        {
            if (OnlyUser == null)
            {
                OnlyUser = true;

            }
            @ViewBag.onlyUser = OnlyUser;
            return View();
        }

        public ActionResult IndexAndGo()
        {
            return RedirectToAction("Index", "DesignTools");

        }

        public ActionResult Snap()
        {
            return View();
        }

        private void ConfigurarVistaCreateEditParaAlta()
        {
            ViewBag.IsEdit = false;
            ViewBag.FormAction = "Create";
            ViewBag.FormId = "formCreateDesign";
            ViewBag.TitleForm = "Crear nuevo diseño";
            ViewBag.SubmitText = "Crear";
            ViewBag.SubmitIcon = "ri-add-line";
            ViewBag.NameFieldName = "DesignName";
            ViewBag.NameFieldId = "DesignName";
            ViewBag.NameFieldLabel = "Nombre del nuevo diseño";
            ViewBag.NameValue = string.Empty;
            ViewBag.DescriptionValue = string.Empty;
        }

        private void ConfigurarVistaCreateEditParaEdicion(TSql_Design designEntity)
        {
            ViewBag.IsEdit = true;
            ViewBag.FormAction = "Edit";
            ViewBag.FormId = "formEditDesign";
            ViewBag.TitleForm = "Editar diseño";
            ViewBag.SubmitText = "Guardar";
            ViewBag.SubmitIcon = "ri-save-line";
            ViewBag.NameFieldName = "AttLabel";
            ViewBag.NameFieldId = "AttLabel";
            ViewBag.NameFieldLabel = "Nombre del diseño";
            ViewBag.NameValue = designEntity?.AttLabel ?? string.Empty;
            ViewBag.DescriptionValue = designEntity?.AttDescription ?? string.Empty;
            ViewBag.SysObjectID = designEntity?.SysObjectID ?? 0;
        }

        private sealed class ZwcadDeviceValidationResult
        {
            public bool IsValid { get; set; }
            public string DeviceId { get; set; }
            public string UserId { get; set; }
        }

        private ZwcadDeviceValidationResult ValidarDispositivoZwcad(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return new ZwcadDeviceValidationResult { IsValid = false };
            }

            var device = db.TSql_PluginDeviceAuth.FirstOrDefault(x => x.DeviceId == deviceId);
            if (device == null)
            {
                return new ZwcadDeviceValidationResult { IsValid = false };
            }

            if (!device.Allowed || !device.IsActive || device.IsRevoked || device.AttIsDeleted)
            {
                return new ZwcadDeviceValidationResult { IsValid = false };
            }

            return new ZwcadDeviceValidationResult
            {
                IsValid = true,
                DeviceId = device.DeviceId,
                UserId = device.LinAspNetUsert
            };
        }

        public JsonResult ListDesing([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, bool OnlyUser)
        {
            try
            {
                string UserId = User.Identity.GetUserId();
                IQueryable<ListDesing> query = null;
                if (OnlyUser == true)
                {
                    query = from design in db.TSql_Design
                            join user in db.AspNetUsers on design.LinCreatedBy equals user.Id
                            join employee in db.TSql_Employee on user.Id equals employee.LinAspNetUsert
                            join company in db.TSql_Company on employee.LinCompany equals company.SysObjectID
                            join branch in db.TSql_Branch on company.SysObjectID equals branch.LinCompany into branchGroup
                            from branch in branchGroup.DefaultIfEmpty()
                            orderby design.AttCreated
                            where design.AttIsDeleted == false && design.LinCreatedBy == UserId
                            select new ListDesing
                            {
                                IdObject = design.SysObjectID,
                                AttDescription = design.AttDescription,
                                AttLabel = design.AttLabel,
                                AttThumbnail = design.AttThumbnail,
                                AttNameEmployee = employee.AttName,
                                AttSurnameEmployee = employee.AttSurname,
                                AttPhotoMenu = employee.AttPhotoMenu,
                                AttCreated = design.AttCreated,
                                AttChange = design.AttChange,
                                DateCreate = design.AttCreated.ToString(),
                                DateChange = design.AttCreated.ToString(),
                                //design.ItIsShared,
                                //design.ItIsSharedMyGrup,
                                AttLabelEmployee = employee.AttName,
                                AddLetercompany = company.AddLeter,
                                Attcompany = company.TextLabel,
                                AttLabelBranch = branch.AttLabel
                            };
                }
                else
                {
                    query = from design in db.TSql_Design
                            join user in db.AspNetUsers on design.LinCreatedBy equals user.Id
                            join employee in db.TSql_Employee on user.Id equals employee.LinAspNetUsert
                            join company in db.TSql_Company on employee.LinCompany equals company.SysObjectID
                            join branch in db.TSql_Branch on company.SysObjectID equals branch.LinCompany into branchGroup
                            from branch in branchGroup.DefaultIfEmpty()
                            orderby design.AttCreated
                            where design.AttIsDeleted == false
                            select new ListDesing
                            {
                                IdObject = design.SysObjectID,
                                AttDescription = design.AttDescription,
                                AttLabel = design.AttLabel,
                                AttThumbnail = design.AttThumbnail,
                                AttNameEmployee = employee.AttName,
                                AttSurnameEmployee = employee.AttSurname,
                                AttPhotoMenu = employee.AttPhotoMenu,
                                AttChange = design.AttChange,
                                AttCreated = design.AttCreated,
                                DateCreate = design.AttCreated.ToString(),
                                DateChange = design.AttCreated.ToString(),
                                //design.ItIsShared,
                                //design.ItIsSharedMyGrup,
                                AttLabelEmployee = employee.AttName,
                                AddLetercompany = company.AddLeter,
                                Attcompany = company.TextLabel,
                                AttLabelBranch = branch.AttLabel
                            };

                }


                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.AttDescription.Contains(value) ||
                                             p.AttLabel.Contains(value) ||
                                             p.AttNameEmployee.Contains(value) ||
                                             p.AttSurnameEmployee.Contains(value) ||
                                             p.AttCreated.ToString().Contains(value) ||
                                             p.AttLabelEmployee.Contains(value) ||
                                             p.Attcompany.Contains(value) ||
                                             p.DateCreate.Contains(value) ||
                                             p.AttLabelBranch.Contains(value));
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "DateCreate";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "AttLabelBranch":
                            orderColumn = "AttLabelBranch";
                            break;
                        case "Attcompany":
                            orderColumn = "Attcompany";
                            break;
                        case "AttLabelEmployee":
                            orderColumn = "AttLabelEmployee";
                            break;
                        case "AttLabel":
                            orderColumn = "AttLabel";
                            break;
                        case "DateCreate":
                            orderColumn = "AttCreated";
                            break;
                        case "DateChange":
                            orderColumn = "DateChange";
                            break;
                        case "System_TextLabel":
                            orderColumn = "AttDescription";
                            break;
                        default:
                            orderColumn = "AttChange";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "AttChange" ? "AttChange" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "AttChange asc" : orderByString);
                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                // Rights
                bool allowEdit = true;
                bool allowDelete = true;
                var data = query.ToList().Select(p => new
                {
                    emptyColumn = "",
                    SysObjectID = p.IdObject,
                    AttNameEmployee = p.AttNameEmployee,
                    AttSurnameEmployee = p.AttSurnameEmployee,
                    AttPhotoMenu = p.AttPhotoMenu,
                    Attcompany = p.Attcompany,
                    AttLabelBranch = p.AttLabelBranch,
                    DateChange = p.AttChange.ToShortDateString(),
                    DateCreate = p.AttCreated.ToShortDateString(),
                    AttLabel = p.AttLabel,
                    AttDescription = p.AttDescription,
                    allowEdit = allowEdit,
                    allowDelete = allowDelete,
                    buttonOpen = "<a title='  Diseñar  'href='" + Url.Content("~/DesignTools/Design/" + p.IdObject) + "' class=\"btn btn-info btn-xs\"><span class=\"fas fa-pen\" aria-hidden=\"true\"></span></a>",
                    buttonEdit = "<a title='  Editar Diseño  ' onclick=EditOB('" + p.IdObject + "')  class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>",
                    buttonDelete = "<a title='  Eliminar Diseño ' onclick=deleteOB('" + p.IdObject + "')  class=\"btn btn-danger btn-xs\" data-modalpaging><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>"
                    //buttonDelete = "<a title='" + Language.Employee.DeleteEmployeeTitle + "' href='" + Url.Content("~/Employee/Delete/" + p.SysObjectID) + "' class=\"btn btn-danger btn-xs\" data-modalpaging><span class=\"glyphicon glyphicon-trash\" aria-hidden=\"true\"></span></a>"
                }).ToList();
                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        public ActionResult GetAt60(string id, IEnumerable<ModelWalls> list)
        {
            try
            {
                var _DataSupEnd = 0;
                List<ModelRenderElement> List = null;
                string UserId = User.Identity.GetUserId();
                TSql_DefaultDesign currentDefaultDisign = db.TSql_DefaultDesign.FirstOrDefault(x => x.LinkAspNetUsers == UserId);
                var MeshRotateX = RotateMesh.rotate_0;
                var MeshRotateMirrowX = RotateMesh.rotate_180;
                var isDimActive = false;
                var restLong = 0;
                var DimRotate = "0";
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                foreach (var dataList in list)
                {
                    if (dataList.Type == 1)
                    {
                        coordinateEndWall_X = (int)(dataList.DataCordenadX + (dataList.Datalong / 100));
                    }
                    if (dataList.Type == 2)
                    {
                        coordinateEndWall_Y = (int)dataList.DataCordenadY;
                    }

                    var TypeTape_0 = dataList.Tape_0;
                    if (TypeTape_0 == "0")
                    {
                        TypeTape_0 = null;
                    }
                    var TypeTape_180 = dataList.Tape_180;
                    var TypeTape_90 = dataList.Tape_90;
                    var TypeTape_270 = dataList.Tape_270;
                    _DataSupEnd = (int)dataList.DataSupEnd;
                    var StarWallX = dataList.DataCordenadX;
                    var StarWallY = dataList.DataCordenadY;
                    var EndWallX = dataList.DataCordenadX + (dataList.Datalong / 10);
                    var EndWallXRemate = dataList.DataCordenadX + (dataList.Datalong / 10) + dataList.DataSupEnd;
                    var EndWallY = dataList.DataCordenadY + (dataList.Datalong / 10);
                    var HasPreviousModule = false;
                    var IsEndModule = false;
                    var IsFirstModule = true;
                    if (dataList.DataSupEnd > 0)
                    {
                        dataList.Datalong = (long)(dataList.Datalong + dataList.DataSupEnd);
                    }
                    List<ModelRenderElement> TemporalListElement = new List<ModelRenderElement>();
                    int _MoveLong = 0;
                    int nLong = 0;
                    int n = 0;
                    if (dataList.Type == 4)
                    {
                        if (dataList.TypeMesh.Substring(0, 4) == "Esq_")
                        {
                            //corner
                            List = Repositories.Atk60.Wall.ModuloWallEsqTLe.setdListElement(
                            dataList.TypeMesh,
                            dataList.YWith,
                            dataList.XWith,
                            dataList.UniversalPanel,
                            currentDefaultDisign,
                            dataList.DataHeight,
                            dataList.DataWith,
                            dataList.Datalong,
                            dataList.DataCordenadX,
                            dataList.DataCordenadY,
                            dataList.Type,
                            dataList.DataWithOtherCorner,
                            dataList.Tape_0,
                            dataList.Tape_180,
                            dataList.Tape_90,
                            dataList.Tape_270
                            );
                            if (List != null)
                            {
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }

                        }
                    }
                    //Puntales
                    if (dataList.Type == 1 || dataList.Type == 2)
                    {
                        //InsertProp.SedProp(dataList.DataWith, dataList.LongLeft, dataList.DataHeight, dataList.Type, dataList.DataCordenadX, dataList.DataCordenadY, ListRenderElement, 59, 61, 150, 151, true);
                    }
                    //Panel
                    if (dataList.Type == 1 || dataList.Type == 2)
                    {

                        List<ModelRenderElement> ListRenderElementDataType1 = null;
                        nLong = (int)(dataList.Datalong / 2700);
                        n = nLong;
                        restLong = (int)(dataList.Datalong - (2700 * nLong));
                        if (nLong >= 1)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                if (dataList.Datalong == 2700)
                                {
                                    HasPreviousModule = false;
                                    IsEndModule = true;
                                }
                                else
                                {
                                    if (nLong == 1)
                                    {
                                        HasPreviousModule = false;
                                        if (restLong >= 300)
                                        {
                                            IsEndModule = false;
                                        }
                                        else
                                        {
                                            IsEndModule = true;
                                        }

                                    }
                                    else
                                    {
                                        if (i == 0)
                                        {
                                            HasPreviousModule = false;
                                            IsEndModule = false;
                                        }
                                        else
                                        {
                                            if (nLong > i + 1)
                                            {
                                                HasPreviousModule = true;
                                                IsEndModule = false;
                                            }
                                            else
                                            {
                                                HasPreviousModule = true;
                                                if (restLong > 0)
                                                {
                                                    IsEndModule = false;
                                                }
                                                else
                                                {
                                                    IsEndModule = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (i == 0)
                                {
                                    isDimActive = true;
                                }

                                if (dataList.Type == 1)
                                {
                                    if (dataList.IdTypeFormworkMode == false)
                                    {
                                        ListRenderElementDataType1 = Repositories.Atk60.Wall.Modulo2700T.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    }
                                    else
                                    {
                                        ListRenderElementDataType1 = Repositories.Atk60.Wall.Modulo2700.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    }
                                }
                                if (dataList.Type == 2)
                                {
                                    if (dataList.IdTypeFormworkMode == false)
                                    {
                                        ListRenderElementDataType1 = Repositories.Atk60.Wall.Modulo2700T.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    }
                                    else
                                    {
                                        ListRenderElementDataType1 = Repositories.Atk60.Wall.Modulo2700.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    }
                                    DimRotate = "90";

                                }
                                if (ListRenderElementDataType1 != null)
                                {
                                    foreach (var item in ListRenderElementDataType1)
                                    {
                                        //if (item.CodeName == "Dim_Horizontal")
                                        //{

                                        //}
                                        ModelRenderElement element = new ModelRenderElement();
                                        element.IdElement = item.IdElement;
                                        element.Type = item.Type;
                                        element.CodeName = item.CodeName;
                                        element.Element = item.Element;
                                        element.ElementF = item.ElementF;
                                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                                        element.ElementWood = item.ElementWood;
                                        element.ElementUnion1 = item.ElementUnion1;
                                        element.LongWood = item.LongWood;
                                        element.heightWood = item.heightWood;
                                        element.x = item.x;
                                        element.y = item.y;
                                        element.z = item.z;
                                        element.XRotate = item.XRotate;
                                        element.YRotate = item.YRotate;
                                        element.ZRotate = item.ZRotate;
                                        element.CodeName = item.CodeName;
                                        element.IdWall = dataList.IdWall;
                                        element.Filter = item.Filter;
                                        ListRenderElement.Add(element);
                                    }
                                }
                                if (dataList.Type == 1 || dataList.Type == 2)
                                {
                                    _MoveLong = _MoveLong + 270;
                                }
                                IsFirstModule = false;
                            }
                            restLong = (int)(dataList.Datalong - (2700 * nLong));
                        }
                        else
                        {
                            restLong = (int)(dataList.Datalong);
                            HasPreviousModule = false;
                            IsEndModule = true;
                        }
                        isDimActive = true;
                        //add 2550
                        if (restLong > 2400)
                        {
                            if (restLong > 2550)
                            {
                                restLong = 2400;
                            }
                        }
                        if (restLong >= 2400)
                        {
                            IsEndModule = true;
                            if (restLong - 2400 <= 300)
                            {
                                HasPreviousModule = false;
                            }

                            if (dataList.Type == 1)
                            {

                                DimRotate = "0";
                                List = Repositories.Atk60.Wall.Modulo2400.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);

                            }
                            if (dataList.Type == 2)
                            {
                                List = Repositories.Atk60.Wall.Modulo2400.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                DimRotate = "90";
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 2400;
                            _MoveLong = _MoveLong + 240;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 2250)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";

                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);

                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = false;
                                dataList.DataCordenadX = dataList.DataCordenadX + 120;
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 60;
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = false;
                                dataList.DataCordenadY = dataList.DataCordenadY + _MoveLong + 120;
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadY = EndWallY - 45;
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                DimRotate = "90";
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 2250;
                            _MoveLong = _MoveLong + 210;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 2100)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 120;
                                if (TypeTape_0 == "TapeS2") { IsEndModule = true; }
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_180, TypeTape_0, EndWallX - 120, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;

                                //Mirar esto

                                //dataList.DataCordenadY = dataList.DataCordenadY - 120;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY - 120, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                DimRotate = "90";
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 2100;
                            _MoveLong = _MoveLong + 210;
                            IsFirstModule = false;
                            HasPreviousModule = true;

                        }
                        if (restLong >= 1950)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 90;
                                if (dataList.CHeck750R == true)
                                {

                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {

                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                            }
                            if (dataList.Type == 2)
                            {
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                dataList.DataCordenadY = dataList.DataCordenadY + 90;
                                IsEndModule = true;
                                if (dataList.CHeck750R == true)
                                {
                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {
                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                DimRotate = "90";
                            }
                            restLong = restLong - 1950;
                            _MoveLong = _MoveLong + 195;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 1800)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = coordinateEndWall_X;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadY = dataList.DataCordenadY + 90;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                DimRotate = "90";
                            }
                            restLong = restLong - 1800;
                            _MoveLong = _MoveLong + 180;
                            IsFirstModule = false;
                            HasPreviousModule = true;

                        }
                        if (restLong >= 1650)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 90;

                                if (dataList.CHeck750R == true)
                                {

                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {

                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                dataList.DataCordenadY = dataList.DataCordenadY + 90;
                                IsEndModule = true;
                                if (dataList.CHeck750R == true)
                                {
                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {
                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                DimRotate = "90";
                            }
                            restLong = restLong - 1650;
                            _MoveLong = _MoveLong + 165;
                            IsFirstModule = false;
                            HasPreviousModule = true;

                        }
                        if (restLong >= 1500)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 90;
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadY = dataList.DataCordenadY + 90;
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                DimRotate = "90";
                            }
                            restLong = restLong - 1500;
                            _MoveLong = _MoveLong + 150;
                            IsFirstModule = false;
                            HasPreviousModule = true;

                        }
                        if (restLong >= 1350)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                IsEndModule = true;
                                dataList.DataCordenadX = dataList.DataCordenadX + 90;
                                if (TypeTape_0 == "TapeS2") { IsEndModule = true; }
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "0";
                                IsEndModule = false;
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(null, null, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                IsFirstModule = false;
                                HasPreviousModule = true;
                                dataList.DataCordenadY = dataList.DataCordenadY + 90;
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                DimRotate = "90";
                            }
                            restLong = restLong - 1350;
                            _MoveLong = _MoveLong + 135;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 1200)
                        {
                            IsEndModule = true;
                            if (restLong - 1200 <= 300)
                            {
                                HasPreviousModule = false;
                            }

                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";

                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "0";
                                List = Repositories.Atk60.Wall.Modulo1200.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                DimRotate = "90";
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 1200;
                            _MoveLong = _MoveLong + 120;
                            IsFirstModule = false;
                            HasPreviousModule = true;

                        }
                        if (restLong >= 1050)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsFirstModule = true;
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo1050.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                IsFirstModule = true;
                                IsFirstModule = true;
                                List = Repositories.Atk60.Wall.Modulo1050.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                DimRotate = "90";
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 1050;
                            _MoveLong = _MoveLong + 105;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 900)
                        {
                            if (restLong - 900 >= 300)
                            {
                                IsEndModule = false;
                            }
                            else
                            {
                                IsEndModule = true;
                            }
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";

                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "90";
                                List = Repositories.Atk60.Wall.Modulo900.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 900;
                            _MoveLong = _MoveLong + 90;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 750)
                        {
                            if (restLong - 750 >= 300)
                            {
                                IsEndModule = false;
                            }
                            else
                            {
                                IsEndModule = true;
                            }
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                if (dataList.CHeck750R == true)
                                {

                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {
                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "90";
                                if (dataList.CHeck750R == true)
                                {
                                    List = Repositories.Atk60.Wall.Modulo750R.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                                else
                                {
                                    List = Repositories.Atk60.Wall.Modulo750.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                    AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                }
                            }
                            restLong = restLong - 750;
                            _MoveLong = _MoveLong + 75;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 600)
                        {
                            if (restLong - 600 >= 300)
                            {
                                IsEndModule = false;
                            }
                            else
                            {
                                IsEndModule = true;
                            }
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                IsEndModule = true;
                                DimRotate = "90";
                                List = Repositories.Atk60.Wall.Modulo600.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 600;
                            _MoveLong = _MoveLong + 60;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 450)
                        {
                            if (restLong - 450 >= 300)
                            {
                                IsEndModule = false;
                            }
                            else
                            {
                                IsEndModule = true;
                            }
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "90";
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo450.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 450;
                            _MoveLong = _MoveLong + 45;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                        if (restLong >= 300)
                        {
                            if (dataList.Type == 1)
                            {
                                DimRotate = "0";
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo300.setdListElement(TypeTape_180, TypeTape_0, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.Type == 2)
                            {
                                DimRotate = "90";
                                IsEndModule = true;
                                List = Repositories.Atk60.Wall.Modulo300.setdListElement(TypeTape_90, TypeTape_270, EndWallX, EndWallY, dataList.LongLeft, dataList.LongRight, IsFirstModule, isDimActive, dataList.Type, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX, dataList.DataCordenadY + _MoveLong, MeshRotateX, MeshRotateMirrowX, dataList.DataRotateZ, dataList.Type, IsEndModule, HasPreviousModule, _DataSupEnd);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            restLong = restLong - 300;
                            _MoveLong = _MoveLong + 30;
                            IsFirstModule = false;
                            HasPreviousModule = true;
                        }
                    }
                    //Braket
                    if (dataList.Type == 1)
                    {
                        List = (List<ModelRenderElement>)Repositories.Atk60.Wall.Bracket_270.SedBraket(dataList.LongLeft, dataList.LongRight, dataList.Type, dataList.DataHeight, dataList.DataWith, dataList.Datalong, StarWallX, dataList.DataCordenadY);
                        AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);

                        List = (List<ModelRenderElement>)Repositories.Atk60.Wall.Bracket_90.SedBraket(dataList.LongLeft, dataList.LongRight, dataList.Type, dataList.DataHeight, dataList.DataWith, dataList.Datalong, StarWallX, dataList.DataCordenadY - dataList.DataWith / 10);
                        AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                    }
                    if (dataList.Type == 2)
                    {
                        List = (List<ModelRenderElement>)Repositories.Atk60.Wall.Bracket_180.SedBraket(dataList.LongLeft, dataList.LongRight, dataList.Type, dataList.DataHeight, dataList.DataWith, dataList.Datalong, StarWallX, StarWallY);
                        AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);

                        List = (List<ModelRenderElement>)Repositories.Atk60.Wall.Bracket_0.SedBraket(dataList.LongLeft, dataList.LongRight, dataList.Type, dataList.DataHeight, dataList.DataWith, dataList.Datalong, StarWallX, dataList.DataCordenadY);
                        AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                    }
                    //Tapes
                    if (dataList.TypeMesh == "Wall_R000")
                    {
                        if (dataList.Tape_0 == "TapeS7")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_0.setdListElement(dataList.CHeck750R, EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng90_0.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng_0_270_RemateCorner.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_0 == "TapeS4")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_0.setdListElement(dataList.CHeck750R, EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng270_0.setdListElement(EndWallX, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng90_0.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_0 == "TapeS6") { dataList.Tape_0 = "TapeS5"; }
                        if (dataList.Tape_0 == "TapeS5")
                        {
                            if (dataList.DataWith <= 352)
                            {
                                List = Repositories.Atk60.Wall.ModuloTape180SExS2.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                List = Repositories.Atk60.Wall.SedUSExS2_180_270.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                List = Repositories.Atk60.Wall.SedUSExS2_180_90.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                            if (dataList.DataWith > 351 && dataList.DataWith <= 851)
                            {
                                List = Repositories.Atk60.Wall.ModuloTape180SExS2.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                List = Repositories.Atk60.Wall.SedAng90_0.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                                List = Repositories.Atk60.Wall.SedUSExS2_180_270.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                                AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            }
                        }
                        if (dataList.Tape_0 == "TapeS1")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_0_01.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, EndWallXRemate, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_0 == "TapeS2")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_0_02.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, EndWallXRemate, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_0 == "TapeS3")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_0_02.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, EndWallXRemate, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.ModuloTape_0_M_01.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_180 == "SExS1_Borrar")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape_180.setdListElement(dataList.CHeck750R, StarWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY - dataList.DataWith / 10);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng270_180.setdListElement(StarWallX, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                            List = Repositories.Atk60.Wall.SedAng90_180.setdListElement(StarWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                        if (dataList.Tape_180 == "SExS2_Borrar")
                        {
                            List = Repositories.Atk60.Wall.ModuloTape180SExS2.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                            AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                        }
                    }
                    if (dataList.TypeMesh == "Pilar")
                    {
                        List = Repositories.Atk60.Wall.ModuloTapeRegularPilar.setdListElement(EndWallX, dataList.LongLeft, dataList.LongRight, currentDefaultDisign, dataList.DataHeight, dataList.DataWith, dataList.Datalong, dataList.DataCordenadX + _MoveLong, dataList.DataCordenadY);
                        AddtoList(List, ListRenderElement, DimRotate, dataList.IdWall);
                    }
                    IsFirstModule = false;
                }
                return Json(new { success = true, ListRenderElement });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        private void AddtoList(List<ModelRenderElement> list, List<ModelRenderElement> ListRenderElement, string dimRotate, string idWall)
        {

            if (list.Count() == 0)
            {
                return;
            }
            foreach (var item in list)
            {
                if (item.CodeName == "Dim_Horizontal")
                {

                }

                ModelRenderElement element = new ModelRenderElement();
                element.IdElement = item.IdElement;
                element.Type = item.Type;
                element.CodeName = item.CodeName;
                element.Element = item.Element;
                element.ElementF = item.ElementF;
                //DIM
                element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                element.LongDimTypeVertical = item.LongDimTypeVertical;
                element.ElementWood = item.ElementWood;
                element.ElementUnion1 = item.ElementUnion1;
                element.LongWood = item.LongWood;
                element.heightWood = item.heightWood;
                element.x = item.x;
                element.y = item.y;
                element.z = item.z;
                element.XRotate = item.XRotate;
                element.YRotate = item.YRotate;
                element.ZRotate = item.ZRotate;
                element.CodeName = item.CodeName;
                element.IdWall = idWall;
                element.Filter = item.Filter;
                element.ParametFilter = item.ParametFilter;
                ListRenderElement.Add(element);
            }
        }

        public ActionResult Save_DesingAndExit(IEnumerable<ModelDesing> list, long DesignId, bool Type)
        {
            try
            {
                using (var trans = db.Database.BeginTransaction())
                {

                    //string UserId = User.Identity.GetUserId();
                    //var currentDesign = db.TSql_DesignDetails.Where(x => x.LinDesaing == DesignId);

                    //if (currentDesign != null)
                    //{
                    //    foreach (var eraseObject in currentDesign)
                    //    {
                    //        db.TSql_DesignDetails.Remove(eraseObject);
                    //    }
                    //    db.SaveChanges();
                    //}
                    //if (list != null)
                    //{
                    //    foreach (var iten in list)
                    //    {
                    //        var Sub_Long_180 = "0";
                    //        var Sub_Long_0 = "0";
                    //        var Sub_Long_90 = "0";
                    //        var Sub_Long_270 = "0";
                    //        var IdWall_0 = "0";
                    //        var IdWall_180 = "0";
                    //        var IdWall_90  = "0";
                    //        var IdWall_270 = "0";
                    //        if (iten.Sub_Long_0 != null) { Sub_Long_0 = iten.Sub_Long_0; }
                    //        if (iten.Sub_Long_180 != null) { Sub_Long_180 = iten.Sub_Long_180; }
                    //        if (iten.Sub_Long_90 != null) { Sub_Long_90 = iten.Sub_Long_90; }
                    //        if (iten.Sub_Long_270 != null) { Sub_Long_270 = iten.Sub_Long_270; }
                    //        if (iten.IdWall_0 != null) { IdWall_0 = iten.IdWall_0; }
                    //        if (iten.IdWall_180 != null) { IdWall_180 = iten.IdWall_180; }
                    //        if (iten.IdWall_90  != null) { IdWall_90  = iten.IdWall_90 ; }
                    //        if (iten.IdWall_270 != null) { IdWall_270 = iten.IdWall_270; }
                    //        TSql_DesignDetails newDesingDetail = new TSql_DesignDetails
                    //        {
                    //            LinDesaing = DesignId,
                    //            LinModifiedBy = UserId,
                    //            AddDateMade = DateTime.UtcNow,
                    //            AddChangeBy = UserId,
                    //            Ntimeschanged = +1,
                    //            AddPositionX = (double?)decimal.Parse(iten.PositionX, CultureInfo.InvariantCulture),
                    //            AddPositionY = (double?)decimal.Parse(iten.PositionY, CultureInfo.InvariantCulture),
                    //            AddPositionZ = (double?)decimal.Parse(iten.PositionZ, CultureInfo.InvariantCulture),
                    //            IdWall = iten.IdWall,
                    //            AddRotationX = (double?)decimal.Parse(iten.RotationX, CultureInfo.InvariantCulture),
                    //            AddRotationY = (double?)decimal.Parse(iten.RotationY, CultureInfo.InvariantCulture),
                    //            AddRotationZ = (double?)decimal.Parse(iten.RotationZ, CultureInfo.InvariantCulture),
                    //            AddName = iten.Name,
                    //            //AddScaleX = float.Parse(iten.ScaleX, CultureInfo.InvariantCulture.NumberFormat),
                    //            AddScaleX = (double?)decimal.Parse(iten.ScaleX, CultureInfo.InvariantCulture),
                    //            AddScaleY = (double?)decimal.Parse(iten.ScaleY, CultureInfo.InvariantCulture),
                    //            AddScaleZ = (double?)decimal.Parse(iten.ScaleZ, CultureInfo.InvariantCulture),
                    //            AddIniciall_Wall = iten.Iniciall_Wall,
                    //            AddEnd_Wall = iten.End_Wall,
                    //            AddTypeWall = iten.TypeWall,
                    //            AddTypeWallLeft = iten.TypeWallLeft,
                    //            AddTypeWallRight = iten.TypeWallRight,
                    //            TypeWall_180 = iten.TypeWall_180,
                    //            TypeWall_0 = iten.TypeWall_0,
                    //            IDCornerDown = iten.IDCornerDown,
                    //            IDCornerLeft = iten.IDCornerLeft,
                    //            ScaleEsqy = iten.ScaleEsqy,
                    //            CHeckDimWall = iten.CHeckDimWall,
                    //            CHeckBracketInside = iten.CHeckBracketInside,
                    //            CHeckBracketOutside = iten.CHeckBracketOutside,
                    //            CHeckRijiInside = iten.CHeckRijiInside,
                    //            CHeckRijiOutside = iten.CHeckRijiOutside,
                    //            CHeckPropInside = iten.CHeckPropInside,
                    //            CHeckPropOutside = iten.CHeckPropOutside,
                    //            CHeckPropInsideInf = iten.CHeckPropInsideInf,
                    //            CHeckPropOutsideInf = iten.CHeckPropOutsideInf,
                    //            CHeck750R = iten.CHeck750R,
                    //            LongLeft = iten.LongLeft,
                    //            LongRight = iten.LongRight,
                    //            IsSolutionCornerYUniversalPanelCorner = iten.IsSolutionCornerYUniversalPanelCorner,
                    //            IsSolutionCornerXUniversalPanelCorner = iten.IsSolutionCornerXUniversalPanelCorner,
                    //            AddTape_0 = iten.Tape_0,
                    //            AddTape_180 = iten.Tape_180,
                    //            AddTape_90 = iten.Tape_90,
                    //            AddTape_270 = iten.Tape_270,
                    //            AddGrup = iten.Grupo,
                    //            Sub_Long_180 = Sub_Long_180,
                    //            Sub_Long_0 = Sub_Long_0,
                    //            Sub_Long_90 = Sub_Long_90,
                    //            Sub_Long_270 = Sub_Long_270,
                    //            IdWall_0 = IdWall_0,
                    //            IdWall_180 = IdWall_180,
                    //            IdWall_90  = IdWall_90 ,
                    //            IdWall_270 = IdWall_270,
                    //        };
                    //        db.TSql_DesignDetails.Add(newDesingDetail);
                    //    }
                    //    db.SaveChanges();
                    //}
                    //trans.Commit();
                    if (Type == true)
                    {

                        TempData.Clear();
                        TempData["ToastType"] = "Act";
                        TempData["ToastTitle"] = "Salvar diseño";
                        TempData["ToastMessage"] = "El No se salvo, falta Implementar";
                        return Json(new { data = true, DesignId, IsOk = true });
                    }
                    else
                    {

                        return Json(new { success = true, DesignId, TypeSave = "GoDesaing" });
                    }

                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }



        public ActionResult Delete_All(IEnumerable<ModelDesing> list, long DesignId, bool Type)
        {
            try
            {
                using (var trans = db.Database.BeginTransaction())
                {

                    string UserId = User.Identity.GetUserId();
                    var currentDesign = db.TSql_DesignDetails.Where(x => x.LinDesaing == DesignId);

                    if (currentDesign != null)
                    {
                        foreach (var eraseObject in currentDesign)
                        {
                            db.TSql_DesignDetails.Remove(eraseObject);
                        }
                        db.SaveChanges();
                    }

                    trans.Commit();
                    if (Type == true)
                    {
                        TempData.Clear();
                        TempData["ToastType"] = "Act";
                        TempData["ToastTitle"] = "Eliminar Todo";
                        TempData["ToastMessage"] = "Todo el muro a sido borrado";
                        return Json(new { data = true, DesignId, IsOk = true });
                    }
                    else
                    {
                        return Json(new { success = true, DesignId, TypeSave = "GoDesaing" });
                    }
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }

        public ActionResult Save_Desing(IEnumerable<ModelDesing> list, long DesignId, bool Type, FormCollection form)
        {
            try
            {
                string screenshot = form[0];
                //byte[] screenshotFile = CreateScreenshotFile(screenshot, true, 750, 500);
                Save_DesingDef(list, DesignId, Type);
                if (Type == true)
                {

                    TempData.Clear();
                    TempData["ToastType"] = "Act";
                    TempData["ToastTitle"] = "Salvar diseño";
                    TempData["ToastMessage"] = "El diseño a sido salvado correctamente";
                    return Json(new { data = true, DesignId, IsOk = true });
                }
                else
                {

                    return Json(new { success = true, DesignId, TypeSave = "GoDesaing" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }
        public ActionResult Save_DesingDef(IEnumerable<ModelDesing> list, long DesignId, bool Type)
        {
            try
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    string UserId = User.Identity.GetUserId();
                    TSql_Design Design = db.TSql_Design.FirstOrDefault(x => x.SysObjectID == DesignId);
                    Design.AttChange = DateTime.UtcNow;
                    var currentDesign = db.TSql_DesignDetails.Where(x => x.LinDesaing == DesignId);
                    if (currentDesign != null)
                    {
                        foreach (var eraseObject in currentDesign)
                        {
                            db.TSql_DesignDetails.Remove(eraseObject);
                        }
                        db.SaveChanges();
                    }
                    if (list != null)
                    {
                        foreach (var iten in list)
                        {
                            var Sub_Long_180 = "0";
                            var Sub_Long_0 = "0";
                            var Sub_Long_90 = "0";
                            var Sub_Long_270 = "0";
                            var IdWall_0 = "0";
                            var IdWall_180 = "0";
                            var IdWall_90 = "0";
                            var IdWall_270 = "0";
                            if (iten.Sub_Long_0 != null) { Sub_Long_0 = iten.Sub_Long_0; }
                            if (iten.Sub_Long_180 != null) { Sub_Long_180 = iten.Sub_Long_180; }
                            if (iten.Sub_Long_90 != null) { Sub_Long_90 = iten.Sub_Long_90; }
                            if (iten.Sub_Long_270 != null) { Sub_Long_270 = iten.Sub_Long_270; }
                            if (iten.IdWall_0 != null) { IdWall_0 = iten.IdWall_0; }
                            if (iten.IdWall_180 != null) { IdWall_180 = iten.IdWall_180; }
                            if (iten.IdWall_90 != null) { IdWall_90 = iten.IdWall_90; }
                            if (iten.IdWall_270 != null) { IdWall_270 = iten.IdWall_270; }
                            TSql_DesignDetails newDesingDetail = new TSql_DesignDetails
                            {
                                LinDesaing = DesignId,
                                LinModifiedBy = UserId,
                                AddDateMade = DateTime.UtcNow,
                                AddChangeBy = UserId,
                                Ntimeschanged = +1,
                                AddPositionX = (double?)decimal.Parse(iten.PositionX, CultureInfo.InvariantCulture),
                                AddPositionY = (double?)decimal.Parse(iten.PositionY, CultureInfo.InvariantCulture),
                                AddPositionZ = (double?)decimal.Parse(iten.PositionZ, CultureInfo.InvariantCulture),
                                IdWall = iten.IdWall,
                                AddRotationX = (double?)decimal.Parse(iten.RotationX, CultureInfo.InvariantCulture),
                                AddRotationY = (double?)decimal.Parse(iten.RotationY, CultureInfo.InvariantCulture),
                                AddRotationZ = (double?)decimal.Parse(iten.RotationZ, CultureInfo.InvariantCulture),
                                AddName = iten.Name,
                                //AddScaleX = float.Parse(iten.ScaleX, CultureInfo.InvariantCulture.NumberFormat),
                                AddScaleX = (double?)decimal.Parse(iten.ScaleX, CultureInfo.InvariantCulture),
                                AddScaleY = (double?)decimal.Parse(iten.ScaleY, CultureInfo.InvariantCulture),
                                AddScaleZ = (double?)decimal.Parse(iten.ScaleZ, CultureInfo.InvariantCulture),
                                AddIniciall_Wall = iten.Iniciall_Wall,
                                AddEnd_Wall = iten.End_Wall,
                                AddTypeWall = iten.TypeWall,
                                AddTypeWallLeft = iten.TypeWallLeft,
                                AddTypeWallRight = iten.TypeWallRight,

                                IDCornerDown = iten.IDCornerDown,
                                IDCornerLeft = iten.IDCornerLeft,
                                ScaleEsqy = iten.ScaleEsqy,
                                CHeckDimWall = iten.CHeckDimWall,
                                CHeckBracketInside = iten.CHeckBracketInside,
                                CHeckBracketOutside = iten.CHeckBracketOutside,
                                CHeckRijiInside = iten.CHeckRijiInside,
                                CHeckRijiOutside = iten.CHeckRijiOutside,
                                CHeckPropInside = iten.CHeckPropInside,
                                CHeckPropOutside = iten.CHeckPropOutside,
                                CHeckPropInsideInf = iten.CHeckPropInsideInf,
                                CHeckPropOutsideInf = iten.CHeckPropOutsideInf,
                                CHeck750R = iten.CHeck750R,
                                LongLeft = iten.LongLeft,
                                LongRight = iten.LongRight,
                                IsSolutionCornerYUniversalPanelCorner = iten.IsSolutionCornerYUniversalPanelCorner,
                                IsSolutionCornerXUniversalPanelCorner = iten.IsSolutionCornerXUniversalPanelCorner,
                                AddTape_0 = iten.Tape_0,
                                AddTape_180 = iten.Tape_180,
                                AddTape_90 = iten.Tape_90,
                                AddTape_270 = iten.Tape_270,
                                AddGrup = iten.Grupo,
                                Sub_Long_180 = Sub_Long_180,
                                Sub_Long_0 = Sub_Long_0,
                                Sub_Long_90 = Sub_Long_90,
                                Sub_Long_270 = Sub_Long_270,
                                IdWall_0 = IdWall_0,
                                IdWall_180 = IdWall_180,
                                IdWall_90 = IdWall_90,
                                IdWall_270 = IdWall_270,
                                TypeWall_180 = iten.TypeWall_180,
                                TypeWall_0 = iten.TypeWall_0,
                                TypeWall_90 = iten.TypeWall_90,
                                TypeWall_270 = iten.TypeWall_270,
                                IdTypeFormworkMode = iten.IdTypeFormworkMode,
                            };
                            db.TSql_DesignDetails.Add(newDesingDetail);
                        }
                        db.SaveChanges();
                    }
                    trans.Commit();
                    return null;
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex.Message);
            }
        }
        public ActionResult Save_Config(IEnumerable<ModelStock> _listDefault, IEnumerable<ModelDesing> list,
        long DesignId, bool Type, double positionX, double positionY, double positionZ, double targetX,
        double targetY, double targetZ, double zoom)
        {
            try
            {
                Save_DesingDef(list, DesignId, Type);
                Save_CamaraDef(DesignId, 1, positionX, positionY, positionZ, targetX, targetY, targetZ, zoom);
                string UserId = User.Identity.GetUserId();
                TSql_DefaultDesign currentDefaultDisign = db.TSql_DefaultDesign.FirstOrDefault(x => x.LinkAspNetUsers == UserId);
                if (currentDefaultDisign != null)
                {
                    db.TSql_DefaultDesign.Remove(currentDefaultDisign);
                }
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = "Cambiar configuración";
                TempData["ToastMessage"] = "la configuración a sido cambiada correctamente";
                TSql_DefaultDesign newDefault = new TSql_DefaultDesign
                {
                    IsSolutionCornerWithUniversalPanel = _listDefault.FirstOrDefault().IsSolutionCornerWithUniversalPanel,
                    LinkAspNetUsers = UserId,
                    NumberClosingStartEndWall = 1,
                    NumberWallheigh = 1,
                    NumberwallWidth = 1,
                    NumberwallLength = 1,
                    LinkMadeBy = UserId,
                    LinModifiedBy = UserId,
                    AddDateMade = DateTime.UtcNow,
                    AddChangeBy = UserId,
                    AddLastDateChange = DateTime.UtcNow,
                    Ntimeschanged = 1,
                    BitIsDeleted = false,
                    LinkEnvironment = _listDefault.FirstOrDefault().IdEnvironmentValue,
                    OrbitControlsType = _listDefault.FirstOrDefault().IdEnvironmentOrbitValue,
                    ExitingPanel2400 = _listDefault.FirstOrDefault().ExitingPanel2400,
                    AddTapeWidthExactIfPossible = _listDefault.FirstOrDefault().AddTapeWidthExactIfPossible,
                    LinTapeInialEndWallIMoreThan300 = 1,
                    LinTapeInialEndWallIsLessThan300 = 1,
                };
                db.TSql_DefaultDesign.Add(newDefault);
                db.SaveChanges();
                return Json(new { success = true, });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        public ActionResult Save_Camara(long LinDesaing, long type, double positionX, double positionY, double positionZ, double targetX, double targetY, double targetZ, double zoom)
        {
            try
            {
                Save_CamaraDef(LinDesaing, type, positionX, positionY, positionZ, targetX, targetY, targetZ, zoom);
                TempData.Clear();
                TempData["ToastType"] = "Act";
                TempData["ToastTitle"] = "Salvar camara";
                TempData["ToastMessage"] = "la camara a sido salvado correctamente";
                return Json(new { data = true, LinDesaing, IsOk = true });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }

        public ActionResult Save_CamaraDef(long LinDesaing, long type, double positionX, double positionY, double positionZ, double targetX, double targetY, double targetZ, double zoom)
        {
            try
            {
                string UserId = User.Identity.GetUserId();
                TSql_DesignCamera currentCamera = db.TSql_DesignCamera.FirstOrDefault(x => x.LinDesaing == LinDesaing && x.LinUser == UserId && x.AttCameraType == type);
                if (currentCamera != null)
                {
                    db.TSql_DesignCamera.Remove(currentCamera);
                }
                TSql_DesignDetails currentDesign = db.TSql_DesignDetails.FirstOrDefault(x => x.LinDesaing == LinDesaing);
                TSql_DesignCamera newCamera = new TSql_DesignCamera
                {
                    LinUser = UserId,
                    LinDesaing = LinDesaing,
                    AttCameraType = type,
                    AttPositionX = positionX,
                    AttPositionY = positionY,
                    AttPositionZ = positionZ,
                    AttTargetX = targetX,
                    AttTargetY = targetY,
                    AttTargetZ = targetZ,
                    AttZoom = zoom,
                    AttLastModification = DateTime.UtcNow,
                    AttIsActive = true,
                    LinModifiedBy = UserId,
                };
                db.TSql_DesignCamera.Add(newCamera);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }
        public ActionResult Save3DView(long idEstimation, long type, double positionX, double positionY, double positionZ, double targetX, double targetY, double targetZ, string zoom)
        {
            try
            {
                string UserId = User.Identity.GetUserId();
                long estimationId = Save3DView(idEstimation, type, positionX, positionY, positionZ, targetX, targetY, targetZ, long.Parse(zoom), UserId);
                if (estimationId == 0)
                {
                    return Json(new { success = false, estimationId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, estimationId });
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
        public long Save3DView(long idEstimation, long type, double positionX, double positionY, double positionZ, double targetX, double targetY, double targetZ, double zoom, string userId)
        {
            try
            {
                using (var trans = db.Database.BeginTransaction())
                {

                    TSql_DesignCamera currentCamera = db.TSql_DesignCamera.FirstOrDefault(x => x.LinDesaing == idEstimation && x.LinUser == userId && x.AttCameraType == type);

                    if (currentCamera != null)
                    {
                        db.TSql_DesignCamera.Remove(currentCamera);
                    }

                    TSql_Design estimation = db.TSql_Design.Find(idEstimation);
                    estimation.AttActiveCameraType = type;
                    db.Entry(estimation).State = EntityState.Modified;
                    db.SaveChanges();
                    TSql_DesignCamera newCamera = new TSql_DesignCamera
                    {
                        LinUser = userId,
                        LinDesaing = idEstimation,
                        AttCameraType = type,
                        AttPositionX = positionX,
                        AttPositionY = positionY,
                        AttPositionZ = positionZ,
                        AttTargetX = targetX,
                        AttTargetY = targetY,
                        AttTargetZ = targetZ,
                        AttZoom = zoom,
                        AttLastModification = DateTime.UtcNow,
                        AttIsActive = true,
                    };

                    db.TSql_DesignCamera.Add(newCamera);

                    db.SaveChanges();
                    trans.Commit();

                    return idEstimation;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public ActionResult Design(long? id)
        {
            try
            {
                //Develop
                var idd = 85;
                if (id != null)
                {
                    idd = (int)id;
                }
                //End Develop
                if (TempData["ToastType"] == null)
                {
                    TempData["ToastType"] = "";
                    TempData["ToastTitle"] = "";
                    TempData["ToastMessage"] = "";

                }
                TSql_Design estimationEntity = db.TSql_Design.Find(idd);
                ModelDesign3d data = CreateDesign3dDModel(idd);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error500", ex);
            }
        }
        public List<TSql_DesignCamera> LoadCameras(long idEstimation)
        {
            List<TSql_DesignCamera> listOfCameras = db.TSql_DesignCamera.Where(x => x.LinDesaing == idEstimation).ToList();
            return listOfCameras;
        }
        public List<TSql_DesignDetails> LoadElement(long idEstimation)
        {
            List<TSql_DesignDetails> listOfElement = db.TSql_DesignDetails.Where(x => x.LinDesaing == idEstimation).ToList();
            return listOfElement;
        }
        private ModelDesign3d CreateDesign3dDModel(long id)
        {
            var IdUser = User.Identity.GetUserId();
            //Develop
            if (IdUser == null)
            {
                IdUser = "1287a994-0fb7-4b08-bdf7-de3d936cedca";
            }
            //End Develop
            TSql_Design estimationEntity = db.TSql_Design.Find(id);
            AspNetUsers SystemID = db.AspNetUsers.Find(IdUser);
            ModelDesign3d data = LoadDesign3D(id, IdUser);
            data.Avatar = db.TSql_Employee.Where(n => n.LinAspNetUsert == IdUser).FirstOrDefault().AttPhotoMenu;
            data.ElemtOfDesign = LoadElement(id);
            data.ListOfCameras = LoadCameras(id);
            data.GroundSizeX = 0;
            data.GroundSizeY = 0;
            return data;
        }
        public ModelDesign3d LoadDesign3D(long Id, string userId)
        {

            TSql_Design TDesign = db.TSql_Design.Find(Id);
            //develop
            if (userId == null)
            {
                userId = "1287a994-0fb7-4b08-bdf7-de3d936cedca";
            }
            // End Develop
            TSql_DefaultDesign TDDefault = (TSql_DefaultDesign)db.TSql_DefaultDesign.Where(x => x.LinkAspNetUsers == userId).FirstOrDefault();
            ModelDesign3d model3d = new ModelDesign3d()
            {

                IsSolutionCornerWithUniversalPanel = TDDefault.IsSolutionCornerWithUniversalPanel,
                LinkEnvironment = TDDefault.LinkEnvironment,
                LinkEnvironmentOrbitValue = TDDefault.OrbitControlsType,
                DesignId = TDesign.SysObjectID,
                DesignName = TDesign.AttLabel,
                SelectedCamera = TDesign.AttActiveCameraType,
                ExitingPanel2400 = TDDefault.ExitingPanel2400,
                NumberClosingStartEndWall = (decimal?)TDDefault.NumberClosingStartEndWall,
            };
            return model3d;
        }

        //Translete this Class to Repository Helper
        public static byte[] CreateScreenshotFile(string screenshot, bool resize, int width, int height)
        {
            byte[] screenshotFile = Convert.FromBase64String(screenshot.Replace("data:image/jpeg;base64,", ""));

            using (MemoryStream ms = new MemoryStream(screenshotFile))
            {
                Image originalImage = Image.FromStream(ms);

                Size size = resize ? new Size(width, height) : new Size(originalImage.Width, originalImage.Height);

                Bitmap finalImage = ResizeImage(originalImage, size);
                if (resize)
                {
                    finalImage = CropImage(finalImage, new Rectangle(finalImage.Width / 2 - 375, 0, 750, 500));
                }

                ImageConverter converter = new ImageConverter();
                screenshotFile = (byte[])converter.ConvertTo(finalImage, typeof(byte[]));
            }

            return screenshotFile;
        }
        #region Images
        public static Bitmap ResizeImage(Image originalImage, Size size)
        {
            double ratioX = (double)size.Width / originalImage.Width;
            double ratioY = (double)size.Height / originalImage.Height;
            double ratio = Math.Max(ratioX, ratioY);

            int width = (int)(originalImage.Width * ratio);
            int height = (int)(originalImage.Height * ratio);

            var newImage = new Bitmap(width, height);
            Graphics.FromImage(newImage).DrawImage(originalImage, 0, 0, width, height);

            Bitmap resizeImage = new Bitmap(newImage);
            return resizeImage;
        }

        public static Bitmap CropImage(Image originalImage, Rectangle sourceRectangle, Rectangle? destinationRectangle = null)
        {
            if (destinationRectangle == null)
            {
                destinationRectangle = new Rectangle(Point.Empty, sourceRectangle.Size);
            }
            var croppedImage = new Bitmap(destinationRectangle.Value.Width, destinationRectangle.Value.Height);
            using (var graphics = Graphics.FromImage(croppedImage))
            {
                graphics.DrawImage(originalImage, destinationRectangle.Value, sourceRectangle, GraphicsUnit.Pixel);
            }
            return croppedImage;
        }

        #endregion




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