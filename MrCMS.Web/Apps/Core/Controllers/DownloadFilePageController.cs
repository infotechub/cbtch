using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using System;
using System.Collections;
using MrCMS.Entities.Messaging;
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
using System.Threading;
using System.Web;
using Elmah;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using OfficeOpenXml;
namespace MrCMS.Web.Apps.Core.Controllers
{

    public class DownloadFilePageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ICompanyService _companySvc;
        private readonly ILogAdminService _logger;
        private readonly UserService _userservice;
        private readonly IRoleService _rolesvc;
        private readonly IEmailSender _emailSender;
        private readonly IEnrolleeService _enrolleeService;

        public DownloadFilePageController(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IHelperService helperService, IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger, ICompanyService companyService, UserService userService, IRoleService roleService, IEmailSender emailSender, IEnrolleeService enrolleeService)
        {


            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _companySvc = companyService;
            _userservice = userService;
            _rolesvc = roleService;
            _emailSender = emailSender;
            _enrolleeService = enrolleeService;


        }
        [ActionName("Show")]
        public ActionResult Show(DownloadFilesPage page)
        {
            //show all shii


            return View(page);
        }

        public JsonResult GetDownloadFilesJson()
        {
            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            string scrshowExpungetype = CurrentRequestData.CurrentContext.Request["scr_expungetype"];
            string scrPrincipalType = CurrentRequestData.CurrentContext.Request["scr_PrincipalType"];
            string scrotherFilter = CurrentRequestData.CurrentContext.Request["scr_otherFilter"];
            string searchText = CurrentRequestData.CurrentContext.Request["sSearch"];

            int toltareccount = 0;
            int totalinresult = 0;

            IList<DownloadFile> items = _helperSvc.QueryDownloadFiles(out toltareccount, out totalinresult, searchText, Convert.ToInt32(displayStart), Convert.ToInt32(displayLength), searchText);

            List<Downloadfilemodel> output = new List<Downloadfilemodel>();

            foreach (DownloadFile item in items)
            {
                Downloadfilemodel obj = new Downloadfilemodel();

                obj.Id = item.Id;
                obj.Filename = item.fileName;
                obj.CreatedBy = _userservice.GetUser(item.createdby).Name;
                obj.Datecreated = CurrentRequestData.Now.ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                obj.DownloadCount = item.downloadCount.ToString();
                obj.Downloadlink = item.filelink;

                output.Add(obj);

            }
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

        public ActionResult DownloadFile(int Id)
        {

            DownloadFile filee = _helperSvc.getDownloadedFile(Id);
            if (filee != null)
            {

                //string filename = filee.fileName + ".zip";
                //string filepath = filee.filelink;
                //byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                //string contentType = MimeMapping.GetMimeMapping(filee.filelink);

                //var cd = new System.Net.Mime.ContentDisposition
                //{
                //    FileName = filename ,
                //    Inline = true,
                //};
                //Response.AppendHeader("Content-Disposition", cd.ToString());


                int count = filee.downloadCount + 1;
                filee.downloadCount = count;
                _helperSvc.updateDownloadFile(filee);

                return Redirect(filee.filelink);
                //return File(filedata, contentType);
            }

            _pageMessageSvc.SetErrormessage("The file does not exist.");
            return _uniquePageService.RedirectTo<DownloadFilesPage>();

        }
    }
}