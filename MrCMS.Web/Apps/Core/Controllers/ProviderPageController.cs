using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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
using MrCMS.Web.Apps.Core.MapperConfig;
using AutoMapper;
using MrCMS.Logging;
using MrCMS.Web.Areas.Admin.Services;
using System.Linq;
using System.Linq.Expressions;
using Elmah;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.People;
using MrCMS.Web.Areas.Admin.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class ProviderPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ITariffService _tariffSvc;
        private readonly ILogAdminService _logger;
        private readonly IUserService _userservice;
        private readonly ICompanyService _compservice;
        private readonly IClaimService _claimserv;
        private readonly IFileAdminService _fileService;
        public ProviderPageController(IPlanService planService, IUniquePageService uniquepageService, IPageMessageSvc pageMessageSvc, IHelperService helperService, IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger, ITariffService tariffService, IUserService userservice, IFileAdminService fileService, IFileAdminService fileAdminService, ICompanyService companyservice, IClaimService claimservice)
        {
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _tariffSvc = tariffService;
            _userservice = userservice;
            _fileService = fileService;
            _compservice = companyservice;
            //_fileAdminService = fileAdminService;
            _claimserv = claimservice;

        }

        [ActionName("Show")]
        public ActionResult Show(ProviderPage page)
        {

            IEnumerable<State> states = _helperSvc.GetallStates();

            //Load the States
            page.States.Clear();
            page.States.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (State item in states)
            {

                page.States.Add(item);
            }


            //Load the Plans


            IEnumerable<Plan> plans = _planService.GetallPlans().Where(x => x.Status == true); //Get those that are active only
            List<PlanVm> planslst = plans.Select(plan => new PlanVm()
            {
                Id = plan.Id,
                Name = plan.Name,
                Code = plan.Code,
                CreatedOn = CurrentRequestData.Now
            }).ToList();
            ViewBag.planlist = new MultiSelectList(planslst, "Id", "Name");

            List<Tariff> tariffs = _tariffSvc.GetallTariff().ToList();
            List<GenericReponse2> companylist = new List<GenericReponse2>();
            IList<Company> companies = _compservice.GetallCompany();

            foreach (Company comp in companies)
            {
                GenericReponse2 neww = new GenericReponse2
                {
                    Id = comp.Id,
                    Name = comp.Name
                };

                companylist.Add(neww);

            }
            ViewBag.companylist = new MultiSelectList(companylist.ToList().OrderBy(x => x.Name), "Id", "Name");


            ViewBag.tarifflist = new MultiSelectList(tariffs, "Id", "Name");
            List<GenericReponse> Pcategory = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(ProviderCategory)))
            {
                Pcategory.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(ProviderCategory), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.Pcategory = Pcategory;

            //Load services
            IEnumerable<Service> services = _serviceSvc.GetallServices().Where(x => x.Status == true); //Get the active services only.
            List<ServiceVm> servicelist = services.Select(serv => new ServiceVm()
            {
                Id = serv.Id,
                Name = serv.Name
            }).ToList();
            ViewBag.servicelist = new MultiSelectList(servicelist, "Id", "Name");

            //load banks

            IEnumerable<Bank> banks = _helperSvc.Getallbanks();
            List<Bank> banklist = banks.Select(bank => new Bank()
            {
                Id = bank.Id,
                Name = bank.Name
            }
                ).ToList();

            ViewBag.banklist = banklist;


            return View(page);
        }

        //This is the provider list json
        public JsonResult GetJson()
        {

            IEnumerable<ProviderVm> reply = _providerSvc.GetallProviderforJson().Where(x => x.AuthorizationStatus == 0 || x.AuthorizationStatus == 2);  //get only those approved. or does not require approval
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }

        public JsonResult GetJsoAdvance()
        {

            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);
            string scrprovidername = CurrentRequestData.CurrentContext.Request["src_providername"];
            string scrprovideraddress = CurrentRequestData.CurrentContext.Request["scr_provideraddress"];

            string scrState = CurrentRequestData.CurrentContext.Request["scr_State"];


            string scrMobilenumber = CurrentRequestData.CurrentContext.Request["scr_mobilenumber"];
            string scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            string scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];
            string scr_Plantype = CurrentRequestData.CurrentContext.Request["scr_Plantype"];
            string scr_Zone = CurrentRequestData.CurrentContext.Request["scr_Zone"];
            string scr_ServiceType = CurrentRequestData.CurrentContext.Request["scr_ServiceType"];
            string scr_category = CurrentRequestData.CurrentContext.Request["scr_Category"];

            string scr_BoundBYType = CurrentRequestData.CurrentContext.Request["BoundByPlan"];
            if (string.IsNullOrEmpty(scrState))
            {
                scrState = "0";
            }

            int toltareccount = 0;
            int totalinresult = 0;

            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;

            int outcat = -1;

            int.TryParse(scr_category, out outcat);

            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            IList<ProviderVm> response = _providerSvc.QueryallProviderforJson(out toltareccount, out totalinresult, search,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrprovidername, scrprovideraddress, Convert.ToInt32(scrState), 0, usedate, fromdate, todate, scr_user, 0, Convert.ToInt32(scr_Plantype), Convert.ToInt32(scr_Zone), Convert.ToInt32(scr_ServiceType), Convert.ToInt32(scr_BoundBYType), outcat, false);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response
            });


        }
        public JsonResult GetJsonPendingAdvance()
        {

            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);
            string scrprovidername = CurrentRequestData.CurrentContext.Request["src_providername"];
            string scrprovideraddress = CurrentRequestData.CurrentContext.Request["scr_provideraddress"];

            string scrState = CurrentRequestData.CurrentContext.Request["scr_State"];


            string scrMobilenumber = CurrentRequestData.CurrentContext.Request["scr_mobilenumber"];
            string scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            string scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];


            if (string.IsNullOrEmpty(scrState))
            {
                scrState = "0";
            }

            int toltareccount = 0;
            int totalinresult = 0;

            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            IList<ProviderVm> response = _providerSvc.QueryallPendingProviderforJson(out toltareccount, out totalinresult, search,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrprovidername, scrprovideraddress, Convert.ToInt32(scrState), 0, usedate, fromdate, todate, scr_user, 0, -1);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response
            });


        }
        [Authorize]
        public JsonResult GetJsonPending()
        {
            Log log = new Log()
            {

            };



            IEnumerable<ProviderVm> reply = _providerSvc.GetallProviderforJson().Where(x => x.AuthorizationStatus == 1);  //get only those approved. or does not require approval
            JsonResult response = Json(reply, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                aaData = response.Data
            });


        }
        [ActionName("ProviderList")]
        public ActionResult ProviderList(ProviderListPage page)
        {
            IEnumerable<State> states = _helperSvc.GetallStates();
            List<GenericReponse> Pcategory = new List<GenericReponse>();
            Pcategory.Add(new GenericReponse { Id = "-1", Name = "Show All" });
            foreach (string item in Enum.GetNames(typeof(ProviderCategory)))
            {
                Pcategory.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(ProviderCategory), item)).ToString(), Name = item.ToUpper() });
            }

            ViewBag.Pcategory = Pcategory;
            //Load the States
            List<GenericReponse> statelist = new List<GenericReponse>();

            foreach (State item in states)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name
                };

                statelist.Add(itemo);
            }

            List<GenericReponse> Zonelist = new List<GenericReponse>();
            List<Zone> templist = _helperSvc.GetallZones().OrderBy(x => x.Name).ToList();


            templist.Insert(0, new Zone { Id = -1, Name = "All Zone" });
            ViewBag.Zonelist = templist;


            List<Service> servicelist = _serviceSvc.GetallServices().OrderBy(x => x.Name).ToList();
            servicelist.Insert(0, new Service { Id = -1, Name = "All Service" });
            ViewBag.Servicelist = servicelist;


            statelist.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.Statelist = statelist;




            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();

            List<GenericReponse> userListmain = new List<GenericReponse>();

            foreach (User item in userlist)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Guid.ToString(),
                    Name = item.Name,
                };
                userListmain.Add(itemo);
            }


            userListmain.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.userlist = userListmain;





            IList<Plan> planlist = _planService.GetallPlans();

            List<GenericReponse> PlanListmain = new List<GenericReponse>();

            foreach (Plan item in planlist)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name,
                };
                PlanListmain.Add(itemo);
            }


            PlanListmain.Insert(0, new GenericReponse() { Id = "-1", Name = "Select All" });
            ViewBag.Planlist = PlanListmain;

            //Return to the providers list page
            return View(page);
        }
        [ActionName("PendingApprovalList")]
        public ActionResult PendingApprovalList(ProviderApprovalPage page)
        {
            IEnumerable<State> states = _helperSvc.GetallStates();

            //Load the States
            List<GenericReponse> statelist = new List<GenericReponse>();

            foreach (State item in states)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name
                };

                statelist.Add(itemo);
            }



            statelist.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.Statelist = statelist;




            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();

            List<GenericReponse> userListmain = new List<GenericReponse>();

            foreach (User item in userlist)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Guid.ToString(),
                    Name = item.Name,
                };
                userListmain.Add(itemo);
            }


            userListmain.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.userlist = userListmain;

            //Return to the providers list page
            return View(page);
        }
        [HttpGet]
        public ActionResult Details(int id)
        {
            ProviderVm providervm = _providerSvc.GetProviderVm(id);
            providervm.CategoryString = Enum.GetName(typeof(ProviderCategory), providervm.category);



            return PartialView("Details", providervm);
        }

        //Action for Add View
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProviderPage page, FormCollection form)
        {

            //Get the form items.

            string providername = form["providername"];
            string providercode = form["providercode"];
            string providersubcode = form["providersubcode"];
            string provideremail = form["provideremail"];
            string providerphone1 = form["providerphone1"];
            string providerphone2 = form["providerphone2"];
            string providerwebsite = form["providerwebsite"];
            string providerState = form["providerState"];
            string providerlga = form["providerlga"];
            string provideraddress = form["provideraddress"];
            string providerArea = form["providerArea"];
            string servicelist = form["servicelist"];
            string planlist = form["planlist"];

            string providerbankname = form["providerbankname"];
            string providerbankaccname = form["providerbankaccname"];
            string providerbankaccnum = form["providerbankaccnum"];
            string providerbankbranch = form["providerbankbranch"];

            string providerbankname2 = form["providerbankname2"];
            string providerbankaccname2 = form["providerbankaccname2"];
            string providerbankaccnum2 = form["providerbankaccnum2"];
            string providerbankbranch2 = form["providerbankbranch2"];

            string tarifflist = form["tarifflist"];
            string consessions = form["consession"];
            string paymentemail1 = form["PaymentEmail1"];
            string PaymentEmail2 = form["PaymentEmail2"];

            string providercategory = form["providercategory"];
            //Do validation


            //Generate subcode and code for 

            if (string.IsNullOrEmpty(providercode) || string.IsNullOrEmpty(providersubcode))
            {
                //generate code and subcode
                Lga lga = _helperSvc.GetLgabyId(Convert.ToInt32(providerlga));
                providercode = string.Format("LAN-{0}", lga.Code);
                providersubcode = _helperSvc.GenerateProvidersubCode(lga);
            }
            //do insert
            int pcatint = 0;
            Provider model = new Provider()
            {
                Name = providername,
                Code = providercode,
                SubCode = providersubcode,
                Email = provideremail,
                Phone = providerphone1,
                Phone2 = providerphone2,
                Website = providerwebsite,
                State = _helperSvc.GetState(Convert.ToInt32(providerState)),
                Lga = _helperSvc.GetLgabyId(Convert.ToInt32(providerlga)),
                Address = provideraddress,
                Area = providerArea,
                Providerservices = servicelist,
                Providerplans = planlist,
                Provideraccount = new ProviderAccount()
                {
                    BankId = Convert.ToInt32(providerbankname),
                    Bankaccountname = providerbankaccname,
                    Bankaccountnum = providerbankaccnum,
                    Bankbranch = providerbankbranch,
                    AuthorizationStatus = (int)AuthorizationStatus.Pending
                },

                Provideraccount2 = new ProviderAccount()
                {
                    BankId = Convert.ToInt32(providerbankname2),
                    Bankaccountname = providerbankaccname2,
                    Bankaccountnum = providerbankaccnum2,
                    Bankbranch = providerbankbranch2,
                    AuthorizationStatus = (int)AuthorizationStatus.Pending
                },
                CompanyConsession = consessions,

                PaymentEmail1 = paymentemail1,
                PaymentEmail2 = PaymentEmail2,
                ProviderTariffs = tarifflist,
                Category = int.TryParse(providercategory, out pcatint) && pcatint > 0 ? (ProviderCategory)pcatint : ProviderCategory.Primary,
                AuthorizationStatus = (int)AuthorizationStatus.Pending,
                Assignee = CurrentRequestData.CurrentUser.Id,
                CreatedBy = CurrentRequestData.CurrentUser.Guid.ToString()
            };


            bool response = _providerSvc.AddnewProvider(model);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was added successfully.", model.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding provider [{0}] ",
                                                              model.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderPage>();



        }

        //Action for Edit View
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ProviderVm provider = _providerSvc.GetProviderVm(id);
            IEnumerable<State> states = _helperSvc.GetallStates();
            IEnumerable<Lga> lgas = _helperSvc.GetLgainstate(provider.State.Id);
            IEnumerable<Bank> banks = _helperSvc.Getallbanks();

            //ViewBag.providerlga = provider.Lgaid;

            ViewBag.StateList = new SelectList(states.AsEnumerable(), "Id", "Name", provider.State.Id);
            ViewBag.LGAList = lgas;
            ViewBag.BankList = new SelectList(banks.AsEnumerable(), "Id", "Name", provider.Provideraccount.BankId);
            ViewBag.BankList2 = new SelectList(banks.AsEnumerable(), "Id", "Name", provider.Provideraccount2 != null ? provider.Provideraccount2.BankId : 1);
            IEnumerable<Plan> plans = _planService.GetallPlans().Where(x => x.Status == true); //Get those that are active only
            List<PlanVm> planslst = plans.Select(plan => new PlanVm()
            {
                Id = plan.Id,
                Name = plan.Name,
                Code = plan.Code,
                CreatedOn = CurrentRequestData.Now
            }).ToList();

            if (provider.Provideraccount2 == null)
            {
                //var providerraw = _providerSvc.GetProvider(provider.Id);
                //providerraw.Provideraccount2 =  new ProviderAccount()
                //{
                //    BankId = Convert.ToInt32(1),
                //    Bankaccountname = string.Empty,
                //    Bankaccountnum = string.Empty,
                //    Bankbranch = string.Empty,
                //    AuthorizationStatus = (int)Utility.AuthorizationStatus.Default
                //};
                //_providerSvc.UpdateProvider(providerraw);
                provider.Provideraccount2 = new ProviderAccount()
                {
                    BankId = Convert.ToInt32(1),
                    Bankaccountname = string.Empty,
                    Bankaccountnum = string.Empty,
                    Bankbranch = string.Empty,
                    AuthorizationStatus = (int)AuthorizationStatus.Default
                };


            }

            List<GenericReponse> Pcategory = new List<GenericReponse>();
            foreach (string item in Enum.GetNames(typeof(ProviderCategory)))
            {
                Pcategory.Add(new GenericReponse() { Id = ((int)Enum.Parse(typeof(ProviderCategory), item)).ToString(), Name = item.ToUpper() });
            }
            ViewBag.Pcategory = new SelectList(Pcategory, "Id", "Name", ((int)provider.category).ToString());
            ViewBag.planlist2 = new MultiSelectList(planslst, "Id", "Name", provider.ProviderplansStr?.Split(',').ToArray() ?? new string[] { });

            //Load services
            IEnumerable<Service> services = _serviceSvc.GetallServices().Where(x => x.Status == true); //Get the active services only.
            List<ServiceVm> servicelist = services.Select(serv => new ServiceVm()
            {
                Id = serv.Id,
                Name = serv.Name
            }).ToList();

            ViewBag.servicelist2 = new MultiSelectList(servicelist, "Id", "Name", provider.ProviderservicesStr?.Split(',').ToArray() ?? new string[] { });
            //load tariffs
            List<Tariff> tariffs = _tariffSvc.GetallTariff().ToList();
            try
            {
                ViewBag.tarifflist2 = new MultiSelectList(tariffs, "Id", "Name", provider.ProvidertariffsStr?.Split(',').ToArray() ?? new string[] { });
            }
            catch (Exception)
            {

                ViewBag.tarifflist2 = new MultiSelectList(tariffs, "Id", "Name");
            }
            List<GenericReponse2> companylist = new List<GenericReponse2>();
            IList<Company> companies = _compservice.GetallCompany();

            foreach (Company comp in companies)
            {
                GenericReponse2 neww = new GenericReponse2
                {
                    Id = comp.Id,
                    Name = comp.Name
                };

                companylist.Add(neww);

            }

            ViewBag.companylistandselect = companylist.OrderBy(x => x.Name);
            string[] selectconsessions = provider.consessions?.Split(',').ToArray() ?? new string[] { };

            ViewBag.selectedconsession = selectconsessions.Select(int.Parse).ToArray();




            return PartialView("Edit", provider);
        }

        [HttpPost]
        public ActionResult Edit(ProviderVm provider, FormCollection form)
        {

            //Get the form items.

            string providername = form["providername"];
            string providercode = form["providercode"];
            string providersubcode = form["providersubcode"];
            string provideremail = form["provideremail"];
            string providerphone1 = form["providerphone1"];
            string providerphone2 = form["providerphone2"];
            string providerwebsite = form["providerwebsite"];
            string providerState = form["providerState"];
            string providerlga = form["providerlga"];
            string provideraddress = form["provideraddress"];
            string providerArea = form["providerArea"];
            string servicelist = form["servicelist"];
            string planlist = form["planlist"];

            string providerbankname = form["providerbankname"];
            string providerbankaccname = form["providerbankaccname"];
            string providerbankaccnum = form["providerbankaccnum"];
            string providerbankbranch = form["providerbankbranch"];
            string providerStatus = form["providerStatus"];

            string providerbankname2 = form["providerbankname2"];
            string providerbankaccname2 = form["providerbankaccname2"];
            string providerbankaccnum2 = form["providerbankaccnum2"];
            string providerbankbranch2 = form["providerbankbranch2"];
            string providerStatus2 = form["providerStatus2"];
            string consessions = form["consessions2"];
            string tarifflist = form["tarifflist"];

            string providercategory = form["providercategory"];
            //Do validation


            //Generate subcode and code for 

            if (string.IsNullOrEmpty(providercode) || string.IsNullOrEmpty(providersubcode))
            {
                //generate code and subcode
                Lga lga = _helperSvc.GetLgabyId(Convert.ToInt32(providerlga));
                providercode = string.Format("LAN-{0}", lga.Code);
                providersubcode = _helperSvc.GenerateProvidersubCode(lga);
            }
            //do insert
            Provider model = _providerSvc.GetProvider(provider.Id);

            model.Name = providername;
            model.Code = providercode;
            model.SubCode = providersubcode;
            model.Email = provideremail;
            model.Phone = providerphone1;
            model.Phone2 = providerphone2;
            model.Website = providerwebsite;
            model.State = _helperSvc.GetState(Convert.ToInt32(providerState));
            model.Lga = _helperSvc.GetLgabyId(Convert.ToInt32(providerlga));
            model.Address = provideraddress;
            model.Providerservices = servicelist;
            model.Providerplans = planlist;
            model.CompanyConsession = consessions;
            model.Area = providerArea;
            model.Provideraccount.BankId = Convert.ToInt32(providerbankname);
            model.Provideraccount.Bankaccountname = providerbankaccname;
            model.Provideraccount.Bankaccountnum = providerbankaccnum;
            model.Provideraccount.Bankbranch = providerbankbranch;
            int pcatint = 0;
            model.Category = int.TryParse(providercategory, out pcatint) && pcatint > 0 ? (ProviderCategory)pcatint : ProviderCategory.Primary;

            if (model.Provideraccount2 == null)
            {

                model.Provideraccount2 = new ProviderAccount
                {
                    BankId = Convert.ToInt32(providerbankname2),
                    Bankaccountname = providerbankaccname2,
                    Bankaccountnum = providerbankaccnum2,
                    Bankbranch = providerbankbranch2
                };

            }
            else
            {
                model.Provideraccount2.BankId = Convert.ToInt32(providerbankname2);
                model.Provideraccount2.Bankaccountname = providerbankaccname2;
                model.Provideraccount2.Bankaccountnum = providerbankaccnum2;
                model.Provideraccount2.Bankbranch = providerbankbranch2;
            }


            model.Status = Convert.ToBoolean(providerStatus.Split(',')[0]);
            model.ProviderTariffs = tarifflist;

            bool response = _providerSvc.UpdateProvider(model);

            if (response)
            {
                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was updated successfully.", model.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updating provider [{0}] ",
                                                              model.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderListPage>();
        }

        //Action for Delete View

        [HttpGet]
        public ActionResult Delete(int id)
        {
            ProviderVm item = _providerSvc.GetProviderVm(id);
            //check if provider got enrollee's assigned

            int providercount = _providerSvc.GetenrolleeusingproviderCount(id);

            bool mustsetalt = false;

            if (providercount > 0)
            {
                mustsetalt = true;

            }
            ViewBag.mustsetalt = mustsetalt;
            ViewBag.enrolleecount = providercount;

            IList<ProviderReponse> providerlist = _providerSvc.GetProviderNameWithAddressList();
            List<GenericReponse2> providerlisttt = new List<GenericReponse2>();
            foreach (ProviderReponse bag in providerlist)
            {
                providerlisttt.Add(new GenericReponse2 { Id = bag.Id, Name = bag.Name.ToUpper() + " - " + bag.Address.ToLower() });

            }
            ViewBag.providerlist = providerlisttt;


            return PartialView("Delete", item);
        }
        public JsonResult GetLga(int? id)
        {

            IEnumerable<Lga> items = _helperSvc.GetLgainstate(Convert.ToInt32(id));
            return Json(items, JsonRequestBehavior.AllowGet);

        }


        public JsonResult GetProviderNamesJson()
        {


            IList<ProviderReponse> providers = _providerSvc.GetProviderNameWithAddressList();

            providers = providers.OrderBy(x => x.Name).ToList();

            return Json(providers, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection provider)
        {

            string alt = provider["provider_alt"];
            string DelistNote = provider["DelistNote"];
            string id = provider["Id"];
            int iddout = 0;
            int alt_p = 0;

            int.TryParse(id, out iddout);
            int.TryParse(alt, out alt_p);

            if (iddout > 0)
            {
                Provider prov = _providerSvc.GetProvider(iddout);
                int providercount = _providerSvc.GetenrolleeusingproviderCount(iddout);

                //if(providercount > 0  && alt_p < 1 || (alt_p == iddout))
                //{
                //    _pageMessageSvc.SetErrormessage("The provider have enrollee(s) assigned to it, kindly select an alternative provider for them");
                //    return _uniquePageService.RedirectTo<ProviderListPage>();
                //}


                prov.isDelisted = true;
                prov.delistedBy = CurrentRequestData.CurrentUser.Id;
                prov.DelistNote = DelistNote;

                bool response = _providerSvc.UpdateProvider(prov) && _providerSvc.SetAltProvider(iddout, alt_p);
                if (response)
                {
                    _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was de-listed successfully.", prov.Name.ToUpper()));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(string.Format("There was an error de-listing provider [{0}] ",
                                                                  prov.Name.ToUpper()));
                }
            }
            return _uniquePageService.RedirectTo<ProviderListPage>();
        }

        [HttpGet]
        public ActionResult Add()
        {
            //Redirect to the creation page
            return _uniquePageService.RedirectTo<ProviderPage>();
        }


        //Action for Edit View
        [HttpGet]
        public ActionResult Approve(int id)
        {
            ProviderVm item = _providerSvc.GetProviderVm(id);
            return PartialView("Approve", item);

        }

        [HttpPost]
        public ActionResult Approve(ProviderVm provider, FormCollection form)
        {

            Provider providerM = _providerSvc.GetProvider(provider.Id);
            providerM.AuthorizationNote = provider.AuthorizationNote;
            providerM.Status = true;
            providerM.AuthorizationStatus = (int)AuthorizationStatus.Authorized;
            providerM.AuthorizedDate = CurrentRequestData.Now;
            providerM.AuthorizedBy = CurrentRequestData.CurrentUser.Id;

            //update the stuff 
            bool resp = _providerSvc.UpdateProvider(providerM);
            if (resp)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was approved successfully.", providerM.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  approving provider [{0}] ",
                                                              providerM.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderApprovalPage>();
        }
        //Action for Edit View
        [HttpGet]
        public ActionResult Disapprove(int id)
        {
            ProviderVm item = _providerSvc.GetProviderVm(id);
            return PartialView("Disapprove", item);

        }

        [HttpPost]
        public ActionResult Disapprove(ProviderVm provider, FormCollection form)
        {

            Provider providerM = _providerSvc.GetProvider(provider.Id);
            providerM.DisapprovalNote = provider.DisapprovalNote;
            providerM.Status = false;
            providerM.AuthorizationStatus = (int)AuthorizationStatus.Disapproved;
            providerM.DisapprovalDate = CurrentRequestData.Now;
            providerM.DisapprovedBy = CurrentRequestData.CurrentUser.Id;

            //update the stuff 
            bool resp = _providerSvc.UpdateProvider(providerM);
            if (resp)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Provider [{0}] was disapproved successfully.", providerM.Name.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  disapproving provider [{0}] ",
                                                              providerM.Name.ToUpper()));
            }

            return _uniquePageService.RedirectTo<ProviderApprovalPage>();
        }


        [HttpPost]
        public ActionResult DoBulkApproval(FormCollection form)
        {
            string ids = form["hidden_selectedIDs"];


            if (!string.IsNullOrEmpty(ids))
            {

                string[] providerlist = ids.Split(',');
                int count = 0;
                foreach (string itemid in providerlist)
                {



                    Provider providerM = _providerSvc.GetProvider(Convert.ToInt32(itemid));
                    providerM.AuthorizationNote = "Bulk Approval. ";
                    providerM.Status = true;
                    providerM.AuthorizationStatus = (int)AuthorizationStatus.Authorized;
                    providerM.AuthorizedDate = CurrentRequestData.Now;
                    providerM.AuthorizedBy = CurrentRequestData.CurrentUser.Id;

                    //update the stuff 
                    bool resp = _providerSvc.UpdateProvider(providerM);

                    if (resp)
                    {
                        count++;
                    }
                    ;

                }

                _pageMessageSvc.SetSuccessMessage(
                   string.Format("You have successfully approved {0} provider(s).", count));

                return _uniquePageService.RedirectTo<ProviderApprovalPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage(
                    "You have not selected any provider to approve.");
                return _uniquePageService.RedirectTo<ProviderApprovalPage>();
            }

        }

        public ActionResult ExportProvider(MediaCategory mediaCategory)
        {

            IEnumerable<Provider> providers = _providerSvc.GetallProvider().Where(x => x.IsDeleted == false && (x.AuthorizationStatus == 0 || x.AuthorizationStatus == 2));

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);

            int count = 1;
            DataTable test = new DataTable();
            test.Columns.Add("S/N", typeof(string));
            test.Columns.Add("UPN", typeof(int));
            test.Columns.Add("STATE", typeof(string));
            test.Columns.Add("LGA", typeof(string));
            test.Columns.Add("LAN", typeof(string));
            test.Columns.Add("PROVIDER NAME *", typeof(string));
            test.Columns.Add("ADDRESS", typeof(string));
            test.Columns.Add("PLANS", typeof(string));
            test.Columns.Add("SERVICES", typeof(string));
            test.Columns.Add("EMAIL", typeof(string));
            test.Columns.Add("MOBILE", typeof(string));
            test.Columns.Add("REMARK", typeof(string));



            foreach (Provider item in providers)
            {
                try
                {
                    string services = item.Providerservices;
                    string plans = item.Providerplans;
                    string servicesOut = "";
                    string planout = "";

                    string[] servicesSplit = services.Split(',');
                    string[] planSplit = plans.Split(',');
                    string lga = item.Lga.Name.ToUpper();

                    foreach (string serv in servicesSplit)
                    {
                        servicesOut = servicesOut + "," + _serviceSvc.GetService(Convert.ToInt32(serv)).Name.ToUpper();

                    }

                    foreach (string plan in planSplit)
                    {
                        planout = planout + "," + _planService.GetPlan(Convert.ToInt32(plan)).Name.ToUpper();

                    }

                    test.Rows.Add(Convert.ToString(count), item.Id, item.State.Name.ToUpper(), lga,
                        item.Code.ToUpper(), item.Name.ToUpper(), item.Address, planout.ToUpper(),
                        servicesOut.ToUpper(), item.Email, item.Phone + "," + item.Phone2, string.Empty);

                }
                catch (Exception ex)
                {

                }

                count++;
            }


            byte[] excelarray = Tools.DumpExcelGetByte(test);


            //write excel to folder
            string path = Path.Combine(fullpath, foldername + ".xlsx");
            System.IO.File.WriteAllBytes(path, excelarray);

            //zip folder and send to client

            //string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

            //ZipFile.CreateFromDirectory(fullpath, zipPath);

            //create new downloadfile
            DownloadFile file = new DownloadFile();

            file.fileName = "Provider List Exported " + CurrentRequestData.Now.ToLongDateString() + ".xlsx";

            file.filelink = path.Replace("//", "/");
            file.filestaus = 1;
            file.createdby = CurrentRequestData.CurrentUser.Id;


            Stream fs = System.IO.File.OpenRead(path);


            if (_fileService.IsValidFileType(file.fileName))
            {
                ViewDataUploadFilesResult dbFile = _fileService.AddFile(fs, file.fileName,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fs.Length,

                    mediaCategory);


                file.filelink = CurrentRequestData.CurrentSite.BaseUrl + dbFile.url;



            }

            fs.Close();

            _helperSvc.AddDownloadFile(file);


            //deleted the zip file

            System.IO.File.Delete(path);

            return _uniquePageService.RedirectTo<DownloadFilesPage>();


        }
        public ActionResult ExportProvider2(MediaCategory mediaCategory)
        {

            //var providers = _providerSvc.GetallProvider();

            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");
            string foldername = Guid.NewGuid().ToString();
            string fullpath = Path.Combine(appdatafolder, foldername);
            Directory.CreateDirectory(fullpath);

            int count = 1;
            DataTable test = new DataTable();
            test.Columns.Add("S/N", typeof(string));
            test.Columns.Add("UPN", typeof(int));
            test.Columns.Add("STATE", typeof(string));
            test.Columns.Add("LGA", typeof(string));
            test.Columns.Add("LAN", typeof(string));
            test.Columns.Add("PROVIDER NAME *", typeof(string));
            test.Columns.Add("ADDRESS", typeof(string));
            //test.Columns.Add("PLANS", typeof(string));
            //test.Columns.Add("SERVICES", typeof(string));
            test.Columns.Add("EMAIL", typeof(string));
            test.Columns.Add("MOBILE", typeof(string));
            test.Columns.Add("REMARK", typeof(string));

            IList<Plan> allplans = _planService.GetallPlans();
            IList<Service> allservices = _serviceSvc.GetallServices();
            foreach (Plan plan in allplans)
            {
                IList<Provider> providers2 = _providerSvc.GetallProviderByPlan(plan.Id);
                byte[] Bytearray = new byte[0];


                using (ExcelPackage pck = new ExcelPackage())
                {
                    foreach (Service service in allservices)
                    {
                        List<Provider> providersunderservices = new List<Provider>();
                        //set sheetname to plan
                        IEnumerable<Provider> providersunderserviceshist = providers2.Where(x => x.Providerservices != null && x.Providerservices.Contains(Convert.ToString(service.Id)));

                        foreach (Provider prov in providersunderserviceshist)
                        {
                            string[] spillted = prov.Providerservices.Split(',');
                            //providersunderservices.AddRange(from spill in spillted where spill.Trim() == Convert.ToString(service.Id) select prov);
                            if (spillted.Any())
                            {
                                foreach (string shii in spillted)
                                {
                                    if (Convert.ToInt32(shii) == service.Id)
                                    {
                                        providersunderservices.Add(prov);

                                    }
                                }


                            }

                        }

                        foreach (Provider item in providersunderservices)
                        {
                            try
                            {
                                string services = item.Providerservices;
                                string plans = item.Providerplans;
                                string servicesOut = "";
                                string planout = "";

                                string[] servicesSplit = services.Split(',');
                                string[] planSplit = plans.Split(',');
                                string lga = item.Lga.Name.ToUpper();

                                foreach (string serv in servicesSplit)
                                {
                                    servicesOut = servicesOut + "," + _serviceSvc.GetService(Convert.ToInt32(serv)).Name.ToUpper();

                                }

                                foreach (string plan2 in planSplit)
                                {
                                    planout = planout + "," + _planService.GetPlan(Convert.ToInt32(plan2)).Name.ToUpper();

                                }

                                test.Rows.Add(Convert.ToString(count), item.Id, item.State.Name.ToUpper(), lga,
                                    item.Code.ToUpper(), item.Name.ToUpper(), item.Address
                                    , item.Email, item.Phone + "," + item.Phone2, string.Empty);

                            }
                            catch (Exception ex)
                            {

                            }

                            count++;
                        }



                        //add shit to the excel
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add(service.Name.ToUpper());

                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        ws.Cells["A1"].LoadFromDataTable(test, true);

                        //Format the header for column 1-3
                        using (ExcelRange rng = ws.Cells["A1:C1"])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            rng.Style.Font.Color.SetColor(Color.White);
                        }

                        //Example how to Format Column 1 as numeric 
                        using (ExcelRange col = ws.Cells[2, 1, 2 + test.Rows.Count, 1])
                        {
                            col.Style.Numberformat.Format = "#,##0.00";
                            col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }

                        //Write it back to the client
                        //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        // Response.AddHeader("content-disposition", "attachment;  filename=EnrolleeListDatabase.xlsx");
                        //Response.BinaryWrite(pck.GetAsByteArray());

                        //clear rows and reset counter
                        test.Rows.Clear();
                        count = 1;

                    }

                    Bytearray = pck.GetAsByteArray();
                    //write excel to folder
                    string path = Path.Combine(fullpath, plan.Name.ToUpper() + ".xlsx");
                    System.IO.File.WriteAllBytes(path, Bytearray);
                }
            }











            //zip folder and send to client

            string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

            ZipFile.CreateFromDirectory(fullpath, zipPath);

            //create new downloadfile
            DownloadFile file = new DownloadFile();

            file.fileName = "Provider List Exported(ClientService)" + CurrentRequestData.Now.ToLongDateString() + ".zip";

            file.filelink = zipPath.Replace("//", "/");
            file.filestaus = 1;
            file.createdby = CurrentRequestData.CurrentUser.Id;


            Stream fs = System.IO.File.OpenRead(zipPath);


            if (_fileService.IsValidFileType(file.fileName))
            {
                ViewDataUploadFilesResult dbFile = _fileService.AddFile(fs, file.fileName,
                    "application/zip", fs.Length,

                    mediaCategory);


                file.filelink = CurrentRequestData.CurrentSite.BaseUrl + dbFile.url;

            }

            //}

            fs.Close();

            _helperSvc.AddDownloadFile(file);


            //deleted the zip file

            System.IO.File.Delete(zipPath);
            //System.IO.File.Delete(fullpath);
            return _uniquePageService.RedirectTo<DownloadFilesPage>();


        }


        public ActionResult SubmitFeedBack()
        {

            string guid = (string)Session["EnrolleeGuid"];

            if (string.IsNullOrEmpty(guid))
            {
                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();

            }

            string providerid = CurrentRequestData.CurrentContext.Request["provider_feedback"];
            string feedback = CurrentRequestData.CurrentContext.Request["feedbacktxt"];
            string rating = CurrentRequestData.CurrentContext.Request["rating"];
            //var phone = CurrentRequestData.CurrentContext.Request["phone"];
            string dateaccessed = CurrentRequestData.CurrentContext.Request["datepicker3"];
            string enrolleeID = CurrentRequestData.CurrentContext.Request["enrolleeidfeedback"];


            DateTime dobtin = !string.IsNullOrEmpty(dateaccessed)
              ? Tools.ParseMilitaryTime("0101", Convert.ToInt32(dateaccessed.Substring(6, 4)),
                  Convert.ToInt32(dateaccessed.Substring(3, 2)), Convert.ToInt32(dateaccessed.Substring(0, 2)))
              : CurrentRequestData.Now.AddYears(-100);

            int provideridd = 0;

            int ratingInt = 0;


            if (!string.IsNullOrEmpty(providerid) && !string.IsNullOrEmpty(feedback) && int.TryParse(providerid, out provideridd) && int.TryParse(rating, out ratingInt))
            {
                ProviderRating obj = new ProviderRating()
                {
                    providerID = provideridd,
                    FeedBack = feedback,
                    dateaccessed = dobtin,
                    rating = ratingInt,
                    PhoneNumber = "",
                    enrolleeid = Convert.ToInt32(enrolleeID)
                };


                if (_providerSvc.addproviderFeedBack(obj))
                {
                    _pageMessageSvc.SetSuccessMessage("Thank you for your feedback,we are continually improving our provider network to serve you better.");
                }



            }
            return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + guid);

        }

        public ActionResult PortalHome(ProviderPortalPage page)
        {

            if (Session["providerportalactive007"] != null)
            {
                Provider providerrr = (Provider)Session["providerportalactive007"];

                ViewBag.provideriddd = providerrr.Id;
                ViewBag.providernameeee = providerrr.Name.ToUpper();

                ViewBag.roleid = (int)Session["providerportalrole007"];


                //ViewBag.nEmail1 = providerrr.providerlogin.email;

                //ViewBag.nEmail2 = providerrr.providerlogin.Altemail;



                return View(page);
            }
            return _uniquePageService.RedirectTo<ProviderPortalLoginPage>();


        }

        public JsonResult AddProviderService(int providerid, int id, string name, string openinghours)
        {

            bool added = false;
            int theid = -1;
            if (providerid > 0)
            {
                Provider provider = _providerSvc.GetProvider(providerid);
                if (provider != null)
                {
                    if (provider.Servicesanddays != null)
                    {
                        provider.Servicesanddays.Add(new ProviderServices()
                        {
                            Name = name,
                            description = name,
                            OpeningDays = openinghours
                        });
                        added = true && _providerSvc.UpdateProvider(provider);
                        provider = _providerSvc.GetProvider(providerid);
                        // theid = provider.Servicesanddays.LastOrDefault().Id;
                    }
                }
                else
                {
                    provider.Servicesanddays = new List<ProviderServices>();
                    provider.Servicesanddays.Add(new ProviderServices()
                    {
                        Name = name,
                        description = name,
                        OpeningDays = openinghours
                    });
                    added = true && _providerSvc.UpdateProvider(provider);
                    provider = _providerSvc.GetProvider(providerid);
                    // theid= provider.Servicesanddays.LastOrDefault().Id;


                }
            }

            if (added)
            {
                return Json(new { resp = 0, msg = theid }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { resp = 99, msg = "-1" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult ProviderPortalLogin(ProviderPortalLoginPage page)
        {



            return View(page);
        }

        [HttpPost]
        [ActionName("ProviderPortalLogin")]
        public ActionResult ProviderPortalLogin(FormCollection form)
        {
            string upn = form["Providercode"];
            string password = form["password"];
            string role = form["userrole"];


            int roleid = 0;


            int.TryParse(role, out roleid);
            Session["providerportalrole007"] = roleid;



            if (!string.IsNullOrEmpty(upn) && !string.IsNullOrEmpty(password))
            {
                int providerid = 0;
                int.TryParse(upn, out providerid);
                Provider theprovider = _providerSvc.GetProvider(providerid);

                if (theprovider != null)
                {

                    if (theprovider.providerlogin == null)
                    {

                        ProviderLogin providerlogin = new ProviderLogin();
                        providerlogin.Provider = theprovider;
                        Regex reg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

                        MatchCollection match = reg.Matches(theprovider.Email);


                        providerlogin.email = match.Count > 0 ? match[0].ToString() : "anthonya@novohealthafrica.org";
                        providerlogin.password = "password";
                        providerlogin.active = true;

                        theprovider.providerlogin = providerlogin;

                        _providerSvc.UpdateProvider(theprovider);
                    }

                    if (theprovider.providerlogin != null && theprovider.providerlogin.password == password)
                    {

                        theprovider.providerlogin.lastlogin = CurrentRequestData.Now;

                        Session["providerportalactive007"] = theprovider;
                        System.Web.HttpBrowserCapabilitiesBase browser = CurrentRequestData.CurrentContext.Request.Browser;
                        string browserid = browser.Id;
                        Session["browserchangedshii"] = false;

                        if (!string.IsNullOrEmpty(theprovider.providerlogin.browserid) && theprovider.providerlogin.browserid != browserid)
                        {
                            Session["browserchangedshii"] = true;

                        }
                        theprovider.providerlogin.browserid = CurrentRequestData.CurrentContext.Request.Browser.Id;
                        _providerSvc.UpdateProvider(theprovider);




                        return _uniquePageService.RedirectTo<ProviderPortalPage>();


                    }
                    else
                    {
                        Session["ErrormessageProviderPortalLogin"] = "Wrong Credential.";
                        return _uniquePageService.RedirectTo<ProviderPortalLoginPage>();

                    }

                }
                else
                {
                    Session["ErrormessageProviderPortalLogin"] = "there was a problem logining in kindly contact admin.";
                    return _uniquePageService.RedirectTo<ProviderPortalLoginPage>();
                }

            }
            Session["ErrormessageProviderPortalLogin"] = "wrong login credentials.";
            return _uniquePageService.RedirectTo<ProviderPortalLoginPage>();
        }


        public ActionResult LogoutfromproviderPortal()
        {
            try
            {
                Session["providerportalactive007"] = null;
            }
            catch (Exception ex)
            {

            }


            return _uniquePageService.RedirectTo<ProviderPortalLoginPage>();

        }

        public ActionResult delistProvider(int providerId, string delistNote, int altprovider)
        {
            if (providerId > 0 && !string.IsNullOrEmpty(delistNote))
            {
                Provider providerr = _providerSvc.GetProvider(providerId);

                if (providerr != null)
                {
                    providerr.isDelisted = true;
                    providerr.DelistNote = delistNote;
                    providerr.delisteddate = CurrentRequestData.Now;
                    providerr.delistedBy = CurrentRequestData.CurrentUser.Id;

                    _providerSvc.UpdateProvider(providerr);
                    _pageMessageSvc.SetSuccessMessage("Provider was successfully delisted.");


                }



            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem Delisting the provider.");

            }
            return _uniquePageService.RedirectTo<ProviderListPage>();

        }
        [HttpPost]
        public ActionResult updateProviderProfile(FormCollection form)
        {
            string theupn = form["upnshii"];
            string theemail1 = form["provideremail1"];
            string theemail2 = form["provideremail2"];
            string theemail3 = form["provideremail3"];
            string theemail4 = form["provideremail4"];
            int id = 0;
            if (!string.IsNullOrEmpty(theupn) && !string.IsNullOrEmpty(theemail1) && int.TryParse(theupn, out id))
            {
                Provider provider = _providerSvc.GetProvider(id);
                if (provider == null)
                {
                    _pageMessageSvc.SetErrormessage("Profile could not be updated.");
                }
                else
                {
                    //good

                    provider.providerlogin.email = theemail1;

                    if (!string.IsNullOrEmpty(theemail2))
                    {
                        provider.providerlogin.Altemail = theemail2;
                    }

                    if (!string.IsNullOrEmpty(theemail3))
                    {
                        provider.providerlogin.Altemail2 = theemail3;
                    }

                    if (!string.IsNullOrEmpty(theemail4))
                    {
                        provider.providerlogin.Altemail3 = theemail4;
                    }
                    _providerSvc.UpdateProvider(provider);
                    _pageMessageSvc.SetSuccessMessage("Profile updated successfully.");

                    ViewBag.nEmail1 = provider.providerlogin.email;

                    ViewBag.nEmail2 = provider.providerlogin.Altemail;
                    Session["providerportalactive007"] = provider;

                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Please fill the form properly.");
            }

            return _uniquePageService.RedirectTo<ProviderPortalPage>();

        }

        [HttpPost]
        public ActionResult updateProviderPassword(FormCollection form)
        {
            string theupn = form["upnshii2"];
            string thecurrentpass = form["currentpassword"];
            string thenewpass = form["newpassword"];
            string theconfpass = form["cnewpassword"];
            int id = 0;
            if (!string.IsNullOrEmpty(theupn) && !string.IsNullOrEmpty(thecurrentpass) && !string.IsNullOrEmpty(thenewpass) && !string.IsNullOrEmpty(theconfpass) && int.TryParse(theupn, out id))
            {
                Provider provider = _providerSvc.GetProvider(id);
                if (provider == null)
                {
                    _pageMessageSvc.SetErrormessage("Passsword could not be changed.");
                }
                else
                {
                    if (thenewpass == theconfpass)
                    {
                        if (provider.providerlogin.password.ToLower() == thecurrentpass.ToLower())
                        {
                            provider.providerlogin.password = thenewpass;
                            provider.providerlogin.passwordchange = true;

                            _providerSvc.UpdateProvider(provider);
                            _pageMessageSvc.SetSuccessMessage("Password changed successfully.");


                        }
                        else
                        {
                            _pageMessageSvc.SetErrormessage("Current password is incorrect.");
                        }
                    }
                    else
                    {
                        _pageMessageSvc.SetErrormessage("Passwords do not match");
                    }


                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Please fill the form properly.");
            }

            return _uniquePageService.RedirectTo<ProviderPortalPage>();

        }

        [HttpPost]
        public JsonResult Storeforbk(List<Dictionary<string, object>> rData)
        {


            //var key = CurrentRequestData.CurrentContext.Request["key"];

            //var sstringg = (string)json;

            if (rData == null)
            {
                return null;
            }

            string json = JsonConvert.SerializeObject(rData);
            string[] resp = json.Replace(@"\", "").Replace("}", "").Replace("}", "").Split(',');
            int count = 0;
            string key = "";
            string providerid = "";

            foreach (string item in resp)
            {
                count++;
                if (item.Contains("hiddenID"))
                {
                    string itemno = resp[count].Split(':')[1];
                    itemno = itemno.Remove(0, 1);
                    itemno = itemno.Substring(0, itemno.Length - 1);
                    key = itemno.Trim();


                }
                if (item.Contains("providerID"))
                {
                    string itemno = resp[count].Split(':')[1];
                    itemno = itemno.Remove(0, 1);
                    itemno = itemno.Substring(0, itemno.Length - 1);
                    providerid = itemno.Trim();


                }
            }
            int provideridint = 0;
            int.TryParse(providerid, out provideridint);

            Provider provider = _providerSvc.GetProvider(provideridint);
            ProviderClaimBK objjj = new ProviderClaimBK();

            objjj.provider = provider;
            objjj.clientkey = key;
            objjj.data = json;
            key = key.Trim();
            ProviderClaimBK itemold = provider.ClaimBK.Where(x => x.clientkey == key).SingleOrDefault();
            ProviderClaimBK itemnew = itemold;


            //does not exist in submited bills

            bool issubmittedbill = _claimserv.CheckClaimByClientID(key);
            if (issubmittedbill)
            {
                //this bill have been submitted before so delete from the claim batch

                if (itemold != null)
                {
                    provider.ClaimBK.Remove(itemold);
                    _providerSvc.UpdateProvider(provider);
                }
            }
            else
            {

                //bill have not been submitted
                if (itemold != null)
                {
                    itemnew.data = json;
                    itemnew.UpdatedOn = CurrentRequestData.Now;

                    provider.ClaimBK[provider.ClaimBK.IndexOf(itemold)] = itemnew;

                    _providerSvc.UpdateProvider(provider);
                    ProviderClaimBKArgs args = new ProviderClaimBKArgs
                    {
                        ClaimBK = itemnew
                    };
                    //Notify the Hub of the new Input
                    EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
                }
                else
                {
                    provider.ClaimBK.Add(objjj);
                    _providerSvc.UpdateProvider(provider);
                    ProviderClaimBKArgs args = new ProviderClaimBKArgs
                    {
                        ClaimBK = objjj
                    };
                    //Notify the Hub of the new Input
                    EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
                }
                //pul





            }







            return Json(key, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getmybackupfromserver(int providerid, string lasttimedate)
        {
            Provider provider = _providerSvc.GetProvider(providerid);
            DateTime datetimeer = CurrentRequestData.Now;
            DateTime.TryParse(lasttimedate, out datetimeer);
            //var dtttt= new DateTime()
            IList<ProviderClaimBK> list = provider.ClaimBK;

            if (!string.IsNullOrEmpty(lasttimedate) && lasttimedate != "null")
            {
                list = provider.ClaimBK.Where(x => x.UpdatedOn >= datetimeer).ToList();

            }


            List<claimbkresp> response = new List<claimbkresp>();

            foreach (ProviderClaimBK item in list)
            {

                claimbkresp tonnyy = new claimbkresp();
                tonnyy.key = item.clientkey;
                object test = JsonConvert.DeserializeObject(item.data);
                tonnyy.data = test;

                response.Add(tonnyy);

            }

            return (Json(response, JsonRequestBehavior.AllowGet));

        }


        public JsonResult DeleteBk(int providerid, string clientkey)
        {
            //method returns key for local deleting.

            Provider provider = _providerSvc.GetProvider(providerid);
            string response = clientkey;
            if (provider != null)
            {
                ProviderClaimBK item = provider.ClaimBK.Where(x => x.clientkey == clientkey).SingleOrDefault();
                if (item != null)
                {
                    provider.ClaimBK.Remove(item);
                    _providerSvc.UpdateProvider(provider);
                    response = clientkey;

                }

            }

            DeleteBillProviderPortalArgs args = new DeleteBillProviderPortalArgs
            {
                providerid = provider != null ? provider.Id.ToString() : "-----",
                key = clientkey
            };
            //Notify the Hub of the new Input
            EventContext.Instance.Publish(typeof(INewNotificationEvent), args);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResult checkifkeyexist(int providerid, string clientkey)
        {
            Provider provider = _providerSvc.GetProvider(providerid);
            string response = "";
            if (provider != null)
            {
                ProviderClaimBK item = provider.ClaimBK.Where(x => x.clientkey == clientkey).SingleOrDefault();
                if (item == null)
                {
                    response = clientkey;

                }

            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResult checkiffirsttime(int providerid)
        {
            Provider provider = _providerSvc.GetProvider(providerid);
            string response = "";
            if (provider != null)
            {
                if (provider.ClaimBK.Count < 2)
                {
                    response = "1";
                }

            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DelistedProviders(DelistedProviderPage page)
        {
            IEnumerable<State> states = _helperSvc.GetallStates();

            //Load the States
            List<GenericReponse> statelist = new List<GenericReponse>();

            foreach (State item in states)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Id.ToString(),
                    Name = item.Name
                };

                statelist.Add(itemo);
            }



            statelist.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.Statelist = statelist;




            List<User> userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();

            List<GenericReponse> userListmain = new List<GenericReponse>();

            foreach (User item in userlist)
            {
                GenericReponse itemo = new GenericReponse()
                {
                    Id = item.Guid.ToString(),
                    Name = item.Name,
                };
                userListmain.Add(itemo);
            }


            userListmain.Insert(0, new GenericReponse() { Id = string.Empty, Name = "Select All" });
            ViewBag.userlist = userListmain;

            //Return to the providers list page

            return View(page);
        }


        [HttpPost]
        public ActionResult DoRestoreProvider(FormCollection form)
        {
            string ids = form["hidden_selectedIDs"];


            if (!string.IsNullOrEmpty(ids))
            {

                string[] providerlist = ids.Split(',');
                int count = 0;
                foreach (string itemid in providerlist)
                {



                    Provider providerM = _providerSvc.GetProvider(Convert.ToInt32(itemid));
                    providerM.isDelisted = false;
                    providerM.delistedBy = 0;
                    providerM.delisteddate = null;
                    providerM.DelistNote = string.Empty;

                    //update the stuff 
                    bool resp = _providerSvc.UpdateProvider(providerM);

                    if (resp)
                    {
                        count++;
                    }
                    ;

                }

                _pageMessageSvc.SetSuccessMessage(
                   string.Format("You have successfully restored {0} provider(s).", count));

                return _uniquePageService.RedirectTo<DelistedProviderPage>();

            }
            else
            {
                _pageMessageSvc.SetErrormessage(
                    "You have not selected any provider to approve.");
                return _uniquePageService.RedirectTo<ProviderApprovalPage>();
            }

        }

        public JsonResult GetJsonDelisted()
        {

            string draw = CurrentRequestData.CurrentContext.Request["draw"];
            string echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            string displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            string displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            string sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnnumber = CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            string sortColumnName = CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(CultureInfo.CurrentCulture);
            string scrprovidername = CurrentRequestData.CurrentContext.Request["src_providername"];
            string scrprovideraddress = CurrentRequestData.CurrentContext.Request["scr_provideraddress"];

            string scrState = CurrentRequestData.CurrentContext.Request["scr_State"];


            string scrMobilenumber = CurrentRequestData.CurrentContext.Request["scr_mobilenumber"];
            string scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            string scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];

            string scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            string scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            string scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];

            string search = CurrentRequestData.CurrentContext.Request["sSearch"];
            string scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];


            if (string.IsNullOrEmpty(scrState))
            {
                scrState = "0";
            }

            int toltareccount = 0;
            int totalinresult = 0;

            DateTime fromdate = CurrentRequestData.Now;
            DateTime todate = CurrentRequestData.Now;
            bool usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            IList<ProviderVm> response = _providerSvc.QueryallDelistedProviderforJson(out toltareccount, out totalinresult, search,
                                                                 Convert.ToInt32(displayStart),
                                                                 Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrprovidername, scrprovideraddress, Convert.ToInt32(scrState), 0, usedate, fromdate, todate, scr_user, 0, -1);
            return Json(new
            {
                sEcho = echo.ToString(),
                recordsTotal = toltareccount.ToString(),
                recordsFiltered = toltareccount.ToString(),
                iTotalRecords = toltareccount.ToString(),
                iTotalDisplayRecords = totalinresult.ToString(),
                aaData = response
            });


        }
    }
}
