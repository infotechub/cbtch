using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Collections;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models.Plan;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Search;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.Services.Search;
using MrCMS.Web.Apps.Core.Services.Widgets;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Faqs.Models;
using MrCMS.Website;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Core.Services;
using MrCMS.Web.Apps.Core.MapperConfig;
using AutoMapper;
using MrCMS.Logging;
using MrCMS.Web.Areas.Admin.Services;
using System.Linq;
using Elmah;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class TariffPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly ITariffService _tariffService;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ILogAdminService _logger;
        private readonly IUserService _userservice;
        public TariffPageController(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IHelperService helperService, ITariffService tariffService, IProviderService Providersvc, IUserService userservice)
        {

            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _userservice = userservice;
            _tariffService = tariffService;
            _providerSvc = Providersvc;

        }

        [ActionName("TarrifList")]
        public ActionResult TarrifList(TariffPage page)
        {
            ISet<MrCMS.Entities.People.UserRole> permited = _uniquePageService.GetUniquePage<TariffContentPage>().FrontEndAllowedRoles;
            ISet<MrCMS.Entities.People.UserRole> userroles = CurrentRequestData.CurrentUser.Roles;
            bool showuser = false;
            foreach (MrCMS.Entities.People.UserRole role in userroles)
            {
                if (permited.Contains(role))
                {
                    showuser = true;
                    break;
                }

            }
            page.ShowEditButtonToUser = showuser;
            return View(page);
        }

        public JsonResult GetJson()
        {

            IList<Tariff> reply = _tariffService.GetallTariff();
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            List<TariffObject> responsee = new List<TariffObject>();
            foreach (Tariff item in reply)
            {
                TariffObject pond = new TariffObject();
                MrCMS.Entities.People.User cteatedby = _userservice.GetUser(item.CreatedBy);
                MrCMS.Entities.People.User authuser = _userservice.GetUser(item.authBy);

                pond.Id = item.Id.ToString();
                pond.Name = CurrentRequestData.CultureInfo.TextInfo.ToTitleCase(!string.IsNullOrEmpty(item.Name) ? item.Name : "--");
                pond.Description = item.Description;
                pond.Status = item.Status.ToString();
                pond.CreatedBy = cteatedby != null ? cteatedby.Name : "--";
                pond.CreatedDate =
                    Convert.ToDateTime(item.CreatedOn)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                pond.authstatus = item.authstatus ? "Authorized" : "Pending";
                pond.authBy = authuser != null ? authuser.Name.ToUpper() : "--";
                pond.AuthorizedDate = item.authstatus ? Convert.ToDateTime(item.AuthorizedDate)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern) : "--";


                responsee.Add(pond);

            }
            return Json(new
            {
                aaData = responsee
            });


        }



        [HttpGet]
        public ActionResult Details(int id)
        {
            ProviderVm providervm = _providerSvc.GetProviderVm(id);


            return PartialView("Details", providervm);
        }

        //Action for Add View
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Tariff tariff, FormCollection form)
        {




            bool response = _tariffService.AddnewTariff(tariff);

            //Add Defaultcategory for both drug and service
            TariffCategory category = new TariffCategory();
            category.Name = "ALL Drugs";
            category.Description = "all the drugs in the tariff.";
            category.TariffId = tariff.Id;
            category.Type = 0;
            _tariffService.AddnewCategory(category);


            //Category Service
            TariffCategory category2 = new TariffCategory();
            category2.Name = "ALL Services";
            category2.Description = "all the services in the tariff.";
            category2.TariffId = tariff.Id;
            category2.Type = 1;
            _tariffService.AddnewCategory(category2);


            //assign to a provider

            if (tariff.defaultProvider > 0)
            {
                Provider theprovider = _providerSvc.GetProvider(tariff.defaultProvider);

                if (theprovider != null)
                {
                    theprovider.ProviderTariffs = tariff.Id.ToString();
                    _providerSvc.UpdateProvider(theprovider);
                }
            }





            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Tariff [{0}] was added successfully.", tariff.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding tariff [{0}] ",
                                                              tariff.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<TariffPage>();



        }

        //Action for Edit View
        [HttpGet]
        public ActionResult Edit(int id)
        {

            Tariff tariff = _tariffService.GetTariff(id);
            return PartialView("Edit", tariff);
        }
        [HttpPost]
        public ActionResult Edit(Tariff tariff)
        {
            bool response = _tariffService.UpdateTarrif(tariff);

            if (response)
            {
                _pageMessageSvc.SetSuccessMessage(string.Format("Tariff [{0}] was updated successfully.", tariff.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was an error updating [{0}] ",
                                                              tariff.Name.ToUpper()));
            }
            return _uniquePageService.RedirectTo<TariffPage>();
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            Tariff item = _tariffService.GetTariff(id);
            return PartialView("Delete", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Tariff tariff)
        {
            if (tariff.Id > 0)
            {


                bool response = _tariffService.DeleteTariff(tariff);

                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage($"Tariff [{tariff.Name.ToUpper()}] was deleted successfully.");
                }
                else
                {
                    _pageMessageSvc.SetErrormessage($"There was an error deleting tariff [{tariff.Name.ToUpper()}] ");
                }
            }
            return _uniquePageService.RedirectTo<TariffPage>();
        }

        [HttpGet]
        public ActionResult Add()
        {

            IList<GenericReponse2> providers = _providerSvc.GetProviderNameList();

            ViewBag.providerlistsss = new SelectList(providers.AsEnumerable(), "Id", "Name");
            //Redirect to the creation page
            return PartialView("Add");
        }


        [ActionName("Content")]
        public ActionResult EditContent(TariffContentPage page, int? id)
        {
            if (id > 0)
            {
                Tariff tariff = _tariffService.GetTariff(Convert.ToInt32(id));
                page.TarrifName = tariff.Name.ToUpper();

                //set tariff id
                //Session["tariffID"] = id;

                ViewBag.TariffID = id;

            }
            return View(page);
        }

        [ActionName("ContentView")]
        public ActionResult ViewContent(TariffListViewPage page, int? id)
        {
            if (id > 0)
            {
                Tariff tariff = _tariffService.GetTariff(Convert.ToInt32(id));
                page.TarrifName = tariff.Name.ToUpper();
                //set tariff id

                ViewBag.TariffID = id;
                //Session["tariffID"] = id;

            }
            return View(page);
        }

        [HttpGet]
        public ActionResult AddCategory(int? id)
        {

            int intid = Convert.ToInt32(id);

            TariffCategory model = new TariffCategory();
            model.Type = intid;
            //Redirect to the creation page
            return PartialView("AddCategory", model);
        }


        public JsonResult AddCategory(TariffCategory category, FormCollection form)
        {
            //Redirect to the creation page
            //get id from session
            int id = Convert.ToInt32(CurrentRequestData.CurrentContext.Request["id"]);



            if (id > 0)
            {
                category.TariffId = id;
                //category.Type = 0;
                _tariffService.AddnewCategory(category);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCategoryJson(int? id)
        {
            int intid = Convert.ToInt32(id);
            int pageid = Convert.ToInt32(CurrentRequestData.CurrentContext.Request["id"]);
            IEnumerable<TariffCategory> reply = _tariffService.GetallTariffCategory().Where(x => x.TariffId == pageid && x.Type == id);
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return response;


        }

        public JsonResult GetDrugTariffJson(int? id)
        {
            id = Convert.ToInt32(id);
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);
            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];


            int toltareccount = 0;
            int totalinresult = 0;
            IEnumerable<TariffCategory> categoryinlist = _tariffService.GetallTariffCategory().Where(x => x.TariffId == id && x.Type == 0);


            IList<DrugTariff> output = _tariffService.QueryAllDrugTariff(out toltareccount, out totalinresult, search, Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, categoryinlist.FirstOrDefault() != null ? categoryinlist.First().Id : -1, sortOrder, "", "", 0);



            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });



        }

        [HttpGet]
        public ActionResult AddDrug(int? id)
        {
            id = Convert.ToInt32(id);
            ViewBag.TariffID = id;
            //Redirect to the creation page
            // var lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 0 && x.TariffId == id).ToList();
            //ViewBag.drugGroup = lst;
            return PartialView("AddDrugTariff");
        }


        public JsonResult AddDrug(DrugTariff drugTariff, FormCollection form)
        {
            //Redirect to the creation page
            //get id from session
            int tariff = Convert.ToInt32(form["Tariffid"]);

            if (drugTariff != null)
            {
                TariffCategory lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 0 && x.TariffId == tariff).FirstOrDefault();

                if (lst == null)
                {
                    TariffCategory category = new TariffCategory();
                    category.Type = 0;
                    category.Name = "ALL";
                    category.TariffId = tariff;
                    _tariffService.AddnewCategory(category);
                    lst = category;
                }
                drugTariff.GroupId = lst.Id;
                drugTariff.Groupname = "ALL";
                drugTariff.Price = Convert.ToDecimal(form["drugprice"]);
                _tariffService.AddnewDrugTariff(drugTariff);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        //Action for Edit View
        [HttpGet]
        public ActionResult EditDrug(int id)
        {
            int pageid = Convert.ToInt32(CurrentRequestData.CurrentContext.Request["id"]);
            DrugTariff tariff = _tariffService.GetDrug(id);
            // var lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 0 && x.TariffId == pageid).ToList();
            // ViewBag.drugGroup = lst;
            ViewBag.pricestr = Convert.ToString(tariff.Price);
            return PartialView("EditDrugTariff", tariff);
        }
        [HttpPost]
        public JsonResult EditDrug(DrugTariff drugTariff, FormCollection form)
        {

            if (drugTariff != null)
            {
                //var name = _tariffService.GetCategory(drugTariff.GroupId).Name;
                //drugTariff.Groupname = name.ToUpper();
                drugTariff.Price = Convert.ToDecimal(form["drugprice"]);
                _tariffService.UpdateDrug(drugTariff);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteDrug(int id)
        {
            DrugTariff item = _tariffService.GetDrug(id);
            return PartialView("DeleteDrug", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDrug(DrugTariff tariff)
        {
            if (tariff.Id > 0)
            {


                bool response = _tariffService.DeleteDrug(tariff);


            }
            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        //



        public JsonResult GetServiceTariffJson(int? id)
        {

            id = Convert.ToInt32(id);
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);
            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];


            int toltareccount = 0;
            int totalinresult = 0;

            IEnumerable<TariffCategory> categoryinlist = _tariffService.GetallTariffCategory().Where(x => x.TariffId == id && x.Type == 1);
            IList<ServiceTariff> output = _tariffService.QueryAllServiceTariff(out toltareccount, out totalinresult, search, Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, categoryinlist.FirstOrDefault() != null ? categoryinlist.First().Id : -1, sortOrder, "", "", 0);


            //foreach (var item in categoryinlist)
            //{
            //    TariffCategory item1 = item;
            //    var cont = _tariffService.GetallServicetariff().Where(x => x.GroupId == item1.Id);
            //    if (cont.Any())
            //    {
            //        output.AddRange(cont.ToList());
            //    }




            //}
            JsonResult response = Json(output, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response.Data
            });
        }

        [HttpGet]
        public ActionResult AddService(int? id)
        {
            id = Convert.ToInt32(id);
            ViewBag.TariffID = id;
            //Redirect to the creation page
            //var lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 1 && x.TariffId == id).ToList();
            //ViewBag.ServiceGroup = lst;
            return PartialView("AddServiceTariff");
        }


        public JsonResult AddService(ServiceTariff serviceTariff, FormCollection form)
        {
            //Redirect to the creation page
            //get id from session
            int tariff = Convert.ToInt32(form["Tariffid"]);
            if (serviceTariff != null)
            {
                // var name = _tariffService.GetCategory(serviceTariff.GroupId).Name;
                TariffCategory lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 1 && x.TariffId == tariff).FirstOrDefault();

                if (lst == null)
                {
                    TariffCategory category = new TariffCategory();
                    category.Type = 1;
                    category.Name = "ALL";
                    category.TariffId = tariff;
                    _tariffService.AddnewCategory(category);
                    lst = category;
                }
                serviceTariff.GroupId = lst.Id;
                serviceTariff.Groupname = "ALL";
                serviceTariff.Price = Convert.ToDecimal(form["serviceprice"]);
                _tariffService.AddnewServiceTariff(serviceTariff);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        //Action for Edit View
        [HttpGet]
        public ActionResult EditService(int id)
        {

            ServiceTariff tariff = _tariffService.GetServiceTariff(id);
            //var lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 1 && x.TariffId == tariff.Id).ToList();
            //ViewBag.serviceGroup = lst;
            ViewBag.servpricestr = Convert.ToString(tariff.Price);
            return PartialView("EditServiceTariff", tariff);
        }
        [HttpPost]
        public JsonResult EditService(ServiceTariff serviceTariff, FormCollection form)
        {
            int id = Convert.ToInt32(CurrentRequestData.CurrentContext.Request["id"]);
            if (serviceTariff != null)
            {
                string name = _tariffService.GetCategory(serviceTariff.GroupId).Name;
                serviceTariff.Groupname = name.ToUpper();
                serviceTariff.Price = Convert.ToDecimal(form["serviceprice"]);
                _tariffService.UpdateServiceTariff(serviceTariff);
            }

            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteService(int id)
        {
            ServiceTariff item = _tariffService.GetServiceTariff(id);
            return PartialView("DeleteService", item);
        }
        [HttpGet]
        public ActionResult UploadService(int id, int mode)
        {
            ServiceTariff item = _tariffService.GetServiceTariff(id);
            ViewBag.TariffID = id;
            ViewBag.Mode = mode;
            return PartialView("UploadServiceTariff", item);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult UploadService(FormCollection form)
        //{
        //    var file = CurrentRequestData.CurrentContext.Request.Files[0];
        //    var mode =Convert.ToInt32(form["mode"]);


        //    var id = Convert.ToInt32(form["Tariffid"]);
        //    var lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 1 && x.TariffId == id && x.IsDeleted==false).FirstOrDefault<TariffCategory>();

        //    if (lst == null)
        //    {
        //        var category = new TariffCategory();
        //        category.Type = 1;
        //        category.Name = "ALL";
        //        category.TariffId = Convert.ToInt32(id);
        //        _tariffService.AddnewCategory(category);
        //        lst = category;
        //    }
        //    if (file != null)
        //    {
        //        using (var package = new ExcelPackage(file.InputStream))
        //        {
        //            //excel open
        //            if (package.Workbook.Worksheets.Count == 0)
        //            {

        //                  _pageMessageSvc.SetErrormessage("The workbook does not contain any workbook.");

        //            }
        //            else
        //            {

        //                var count = 0;
        //                if (package.Workbook.Worksheets.Count > 0)
        //                {
        //                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
        //                    var start = worksheet.Dimension.Start;
        //                    var end = worksheet.Dimension.End;
        //                    for (int row = start.Row; row <= end.Row; row++)
        //                    {
        //                        // Row by row...
        //                        count++;
        //                        var serviceName= worksheet.Cells[row, 1].Text;
        //                        var Unit = worksheet.Cells[row, 2].Text;
        //                        var Price = worksheet.Cells[row, 3].Text;
        //                        var authorizationRequired= worksheet.Cells[row, 4].Text;
        //                        var remarks = worksheet.Cells[row, 5].Text;


        //                        //Do what you wanna
        //                        if (row > 1)
        //                        {
        //                            var servicetariff = new ServiceTariff();
        //                            try
        //                            {
        //                                var priceD = 0m;
        //                                servicetariff.GroupId = lst.Id;
        //                                servicetariff.Name = serviceName.ToUpper();
        //                                servicetariff.Description = serviceName.ToUpper();
        //                                servicetariff.Unit = Unit.ToUpper();
        //                                servicetariff.Price = decimal.TryParse(Price, out priceD) ? priceD : priceD;
        //                                servicetariff.Groupname = lst.Name.ToUpper();
        //                                servicetariff.Remark = remarks;
        //                            }
        //                            catch(Exception ex)
        //                            {
        //                                _pageMessageSvc.SetErrormessage(string.Format("There was an error uploading tariff {0}", ex.Message));
        //                                return Redirect(
        //            string.Format(
        //                _uniquePageService.GetUniquePage<TariffContentPage>().AbsoluteUrl +
        //                "?id={0}", id)); 
        //                            }



        //                            if (authorizationRequired.ToLower().Contains("y"))
        //                            {
        //                                servicetariff.PreauthorizationRequired = true;
        //                            }else
        //                            {
        //                                servicetariff.PreauthorizationRequired = false;
        //                            }
        //                            if(mode> 0 && !string.IsNullOrEmpty(servicetariff.Name))
        //                            {
        //                                _tariffService.AddnewServiceTariff(servicetariff);
        //                            }else
        //                            {
        //                               lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 0 && x.TariffId == id).FirstOrDefault<TariffCategory>();
        //                                if (lst == null)
        //                                {
        //                                    var category = new TariffCategory();
        //                                    category.Type = 0;
        //                                    category.Name = "ALL";
        //                                    category.TariffId = Convert.ToInt32(id);
        //                                    _tariffService.AddnewCategory(category);
        //                                    lst = category;
        //                                }
        //                                var drugariff = new DrugTariff();

        //                                try
        //                                {
        //                                    var priceD = 0m;
        //                                    drugariff.GroupId = lst.Id;
        //                                    drugariff.Name = serviceName.ToUpper();
        //                                    drugariff.Description = serviceName.ToUpper();
        //                                    drugariff.Unit = Unit.ToUpper();
        //                                    drugariff.Price = decimal.TryParse(Price,out priceD ) ? priceD : priceD ;
        //                                    drugariff.Groupname = lst.Name.ToUpper();
        //                                    drugariff.Remark = remarks;
        //                                }
        //                                catch(Exception ex)
        //                                {
        //                                    _pageMessageSvc.SetErrormessage(string.Format("There was an error uploading tariff {0}", ex.Message));
        //                                    return Redirect(
        //                string.Format(
        //                    _uniquePageService.GetUniquePage<TariffContentPage>().AbsoluteUrl +
        //                    "?id={0}", id)); ;
        //                                }




        //                                if (authorizationRequired.ToLower().Contains("y"))
        //                                {
        //                                    drugariff.PreauthorizationRequired = true;
        //                                }
        //                                else
        //                                {
        //                                    drugariff.PreauthorizationRequired = false;
        //                                }

        //                                if (!string.IsNullOrEmpty(drugariff.Name))
        //                                {
        //                                    _tariffService.AddnewDrugTariff(drugariff);
        //                                }

        //                            }

        //                        }


        //                    }
        //                }
        //            }
        //        }
        //                    }


        //    _pageMessageSvc.SetSuccessMessage("File Upload Complete.");
        //    return
        //        Redirect(
        //            string.Format(
        //                _uniquePageService.GetUniquePage<TariffContentPage>().AbsoluteUrl +
        //                "?id={0}", id));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadService(FormCollection form)
        {
            if (CurrentRequestData.CurrentContext.Request.Files.Count > 0)
            {
                System.Web.HttpPostedFileBase file = CurrentRequestData.CurrentContext.Request.Files[0];
                int mode = Convert.ToInt32(form["mode"]);


                int id = Convert.ToInt32(form["Tariffid"]);
                TariffCategory lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 1 && x.TariffId == id && x.IsDeleted == false).FirstOrDefault();

                if (lst == null)
                {
                    TariffCategory category = new TariffCategory();
                    category.Type = 1;
                    category.Name = "ALL";
                    category.TariffId = Convert.ToInt32(id);
                    _tariffService.AddnewCategory(category);
                    lst = category;
                }
                //the new reading using csv

                try
                {
                    using (TextReader reader = new StreamReader(file.InputStream))
                    {
                        CsvReader csv = new CsvReader(reader);
                        List<UploadTariffCSV> records = csv.GetRecords<UploadTariffCSV>().ToList();


                        foreach (UploadTariffCSV item in records)
                        {
                            ServiceTariff servicetariff = new ServiceTariff();

                            servicetariff.GroupId = lst.Id;
                            servicetariff.Name = item.itemname.ToUpper();
                            servicetariff.Description = item.itemname.ToUpper();
                            servicetariff.Unit = item.unit.ToUpper();
                            servicetariff.Price = item.price;
                            servicetariff.Groupname = lst.Name.ToUpper();
                            servicetariff.Remark = item.remark;

                            if (item.authorizationRequired.ToLower().Contains("y"))
                            {
                                servicetariff.PreauthorizationRequired = true;
                            }
                            else
                            {
                                servicetariff.PreauthorizationRequired = false;
                            }

                            if (mode > 0 && !string.IsNullOrEmpty(servicetariff.Name))
                            {
                                _tariffService.AddnewServiceTariff(servicetariff);
                            }
                            else
                            {
                                //do for drug 
                                lst = _tariffService.GetallTariffCategory().Where(x => x.Type == 0 && x.TariffId == id).FirstOrDefault();
                                if (lst == null)
                                {
                                    TariffCategory category = new TariffCategory();
                                    category.Type = 0;
                                    category.Name = "ALL";
                                    category.TariffId = Convert.ToInt32(id);
                                    _tariffService.AddnewCategory(category);
                                    lst = category;
                                }
                                DrugTariff drugariff = new DrugTariff();

                                drugariff.GroupId = lst.Id;
                                drugariff.Name = item.itemname.ToUpper();
                                drugariff.Description = item.itemname.ToUpper();
                                drugariff.Unit = item.unit.ToUpper();
                                drugariff.Price = item.price;
                                drugariff.Groupname = lst.Name.ToUpper();
                                drugariff.Remark = item.remark;



                                if (item.authorizationRequired.ToLower().Contains("y"))
                                {
                                    drugariff.PreauthorizationRequired = true;
                                }
                                else
                                {
                                    drugariff.PreauthorizationRequired = false;
                                }

                                if (!string.IsNullOrEmpty(drugariff.Name))
                                {
                                    _tariffService.AddnewDrugTariff(drugariff);
                                }



                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("There was an error reading the CSV uploaded,kindly check the format and try again.");

                    return
                    Redirect(
                        string.Format(
                            _uniquePageService.GetUniquePage<TariffContentPage>().AbsoluteUrl +
                            "?id={0}", id));
                }



            }
            else
            {
                _pageMessageSvc.SetErrormessage("You have not uploaded any file.");

                return
                Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri);

            }




            _pageMessageSvc.SetSuccessMessage("File Upload Complete.");
            return
                 Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri);


        }

        public ActionResult ClearTariff(FormCollection form)
        {

            uint mode = Convert.ToUInt32(CurrentRequestData.CurrentContext.Request.QueryString["mode"]);
            int id = Convert.ToInt32(CurrentRequestData.CurrentContext.Request["id"]);

            if (mode == 0)
            {
                _tariffService.ClearallDrugTariff(id);
            }

            if (mode == 1)
            {
                _tariffService.ClearallServiceTariff(id);
            }

            _pageMessageSvc.SetSuccessMessage("Tariff has been cleared successfully.");
            return Redirect(
                   string.Format(
                       _uniquePageService.GetUniquePage<TariffContentPage>().AbsoluteUrl +
                       "?id={0}", id)); ;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteService(ServiceTariff tariff)
        {
            if (tariff.Id > 0)
            {


                bool response = _tariffService.DeleteServiceTariff(tariff);


            }
            return Json("{result:1}", JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetProviderServiceTariffJson(int? providerid)
        {

            int id = Convert.ToInt32(providerid);
            Provider provider = _providerSvc.GetProvider(id);
            int tariffid = -1;
            if (provider != null)
            {
                //var t_id = !string.IsNullOrEmpty(provider.ProviderTariffs) ? provider.ProviderTariffs.Split(',')[0] : "-1";
                tariffid = -1;


                if (!string.IsNullOrEmpty(provider.ProviderTariffs))
                {
                    if (!int.TryParse(provider.ProviderTariffs.Split(',')[0], out tariffid))
                    {
                        tariffid = -1;
                    }

                }
                TariffCategory categoryinlist = _tariffService.GetallTariffCategory().Where(x => x.TariffId == tariffid && x.Type == 1).FirstOrDefault();


                if (categoryinlist != null)
                {
                    IList<ServiceTariff> service_tariff = _tariffService.GetallservicetariffByCategory(categoryinlist.Id);

                    List<TariffGenericReponse> response = new List<TariffGenericReponse>();

                    foreach (ServiceTariff item in service_tariff)
                    {
                        //handle null value
                        if (string.IsNullOrEmpty(item.Unit))
                        {
                            item.Unit = "";

                        }
                        response.Add(new TariffGenericReponse
                        {
                            Id = item.Id.ToString(),
                            Name = !string.IsNullOrEmpty(item.Name) ? item.Name.ToUpper() + " " + item.Unit.ToLower() : "--",
                            Price = item.Price.ToString()
                        });
                    }

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                return Json(new { respcode = 99, respmsg = "There is not enrollee category." }, JsonRequestBehavior.AllowGet);


            }



            return Json(new { respcode = 98, respmsg = "The provider id is invalid." }, JsonRequestBehavior.AllowGet);






        }
        public JsonResult GetProviderDrugTariffJson(int? providerid)
        {

            int id = Convert.ToInt32(providerid);
            Provider provider = _providerSvc.GetProvider(id);
            int tariffid = -1;
            if (provider != null)
            {
                //var t_id = !string.IsNullOrEmpty(provider.ProviderTariffs) ? provider.ProviderTariffs.Split(',')[0] : "-1";
                tariffid = -1;

                //var tariffID = 0;

                if (!string.IsNullOrEmpty(provider.ProviderTariffs))
                {
                    if (!int.TryParse(provider.ProviderTariffs.Split(',')[0], out tariffid))
                    {
                        tariffid = -1;
                    }

                }

                TariffCategory categoryinlist = _tariffService.GetallTariffCategory().Where(x => x.TariffId == tariffid && x.Type == 0).FirstOrDefault();


                if (categoryinlist != null)
                {
                    IList<DrugTariff> service_tariff = _tariffService.GetalldrugtariffByCategory(categoryinlist.Id);

                    List<TariffGenericReponse> response = new List<TariffGenericReponse>();

                    foreach (DrugTariff item in service_tariff)
                    {

                        if (string.IsNullOrEmpty(item.Name))
                        {
                            item.Name = "--";

                        }
                        if (string.IsNullOrEmpty(item.Unit))
                        {
                            item.Unit = "--";

                        }
                        response.Add(new TariffGenericReponse
                        {
                            Id = item.Id.ToString(),
                            Name = item.Name.ToUpper() + " " + item.Unit.ToLower(),
                            Price = item.Price.ToString()
                        });
                    }

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                return Json(new { respcode = 99, respmsg = "There is not enrollee category." }, JsonRequestBehavior.AllowGet);


            }



            return Json(new { respcode = 98, respmsg = "The provider id is invalid." }, JsonRequestBehavior.AllowGet);






        }

        public ActionResult ApproveTarrif(int id)
        {

            Tariff tariff = _tariffService.GetTariff(id);
            if (tariff != null)
            {
                tariff.authstatus = true;
                tariff.authBy = CurrentRequestData.CurrentUser.Id;
                tariff.AuthorizedDate = CurrentRequestData.Now;
                _tariffService.UpdateTarrif(tariff);
            }

            _pageMessageSvc.SetSuccessMessage("Tariff Authorized Succesfully.");
            return _uniquePageService.RedirectTo<TariffPage>();


        }
    }




}
