using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System;
using System.Collections;
using MrCMS.Entities.People;
using MrCMS.Services;
using MrCMS.Web.Apps.Articles.ActionResults;
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
using MrCMS.Web.Apps.Core.Services.UserChat;
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
using Elmah;
using NHibernate.Criterion;
using NHibernate.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using MaritalStatus = MrCMS.Web.Apps.Core.Utility.MaritalStatus;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Text;
using MrCMS.Web.Areas.Admin.Models;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.Messaging;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Controllers
{


    public class EnrolleePageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IDocumentService _documentService;
        private readonly IFileAdminService _fileService;
        private readonly IFileAdminService _fileAdminService;
        private readonly IPlanService _planService;
        private readonly IServicesService _serviceSvc;
        private readonly IUniquePageService _uniquePageService;
        private readonly IPageMessageSvc _pageMessageSvc;
        private readonly IHelperService _helperSvc;
        private readonly IProviderService _providerSvc;
        private readonly ITariffService _tariffSvc;
        private readonly ILogAdminService _logger;
        private readonly ICompanyService _companyService;
        private readonly IEnrolleeService _enrolleeService;
        private readonly ISmsService _smsservice;
        private readonly IUserService _userservice;
        private readonly IUserChat _userchat;
        private readonly MailSettings _mailSettings;
        private readonly IEmailSender _emailSender;
        private readonly IClaimService _claimService;
        public EnrolleePageController(IPlanService planService, IUniquePageService uniquepageService,
            IPageMessageSvc pageMessageSvc, IHelperService helperService,
            IServicesService serviceSvc, IProviderService Providersvc, ILogAdminService logger,
            ITariffService tariffService, ICompanyService companyService,
            IEnrolleeService enrolleeService, ISmsService smsSvc, IUserChat iUserChat, IUserService userservice,
            IFileAdminService fileService, IFileAdminService fileAdminService, IDocumentService documentService,
            MailSettings mailSettings, IEmailSender emailSender, IClaimService ClaimService)
        {
            _planService = planService;
            _uniquePageService = uniquepageService;
            _pageMessageSvc = pageMessageSvc;
            _helperSvc = helperService;
            _serviceSvc = serviceSvc;
            _providerSvc = Providersvc;
            _logger = logger;
            _tariffSvc = tariffService;
            _companyService = companyService;
            _enrolleeService = enrolleeService;
            _smsservice = smsSvc;
            _userchat = iUserChat;
            _userservice = userservice;
            _fileService = fileService;
            _fileAdminService = fileAdminService;
            _documentService = documentService;
            _mailSettings = mailSettings;
            _emailSender = emailSender;
            _claimService = ClaimService;
        }

        [ActionName("Enrollee")]
        public ActionResult EnrolleePage(EnrolleePage page, int? id)
        {
            var staff = new Staff();
            var idint = 0;
            if (int.TryParse(id.ToString(), out idint))
            {
                staff = _companyService.Getstaff(idint);
                ViewBag.Stafffullname = staff.StaffFullname.ToUpper();
                ViewBag.StaffID = idint;

            }



            var maritallist = Enum.GetValues(typeof(MaritalStatus));
            ViewBag.MaritalStatus = (from object item in maritallist
                                     select new DdListItem()
                                     {
                                         Id = Convert.ToString((int)item),
                                         Name = Enum.GetName(typeof(MaritalStatus), item)
                                     }).ToList();


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();


            var sponsorlist = Enum.GetValues(typeof(Sponsorshiptype));
            ViewBag.Sponsorshiptype = (from object item in sponsorlist
                                       select new DdListItem()
                                       {
                                           Id = Convert.ToString((int)item),
                                           Name = _helperSvc.GetDescription((Sponsorshiptype)item)
                                       }).ToList();



            ViewBag.Company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId)).Name.ToUpper();
            ViewBag.CompanySubsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary).Subsidaryname.ToUpper();

            PlanVm plan = null;
            if (idint > 0)
            {
                plan = _planService.GetPlan(_companyService.GetCompanyPlan(staff.StaffPlanid).Planid);

                ViewBag.SubscriptionType = plan.Name.ToUpper();
            }
            else
            {
                ViewBag.SubscriptionType = string.Empty;
            }

            var states = _helperSvc.GetallStates();

            var providerrrs = _providerSvc.GetallProviderByPlan(plan.Id).OrderBy(x => x.Name);
            var p_provider = new List<GenericReponse2>();
            foreach (var item in providerrrs)
            {
                var P_item = new GenericReponse2();
                P_item.Id = item.Id;
                P_item.Name = item.Name.ToUpper() + " - " + item.Address.ToLower();
                p_provider.Add(P_item);
            }
            ViewBag.providerlist = p_provider;

            //Load the States
            page.States.Clear();
            page.States.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (var item in states)
            {

                page.States.Add(item);
            }
            var policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();
            ViewBag.policynumber = policynumber;

            return View(page);
        }

        [ActionName("EnrolleeList")]
        public ActionResult EnrolleeList(EnrolleeListPage page)
        {
            var providerlist = _providerSvc.GetallProvider().OrderBy(x => x.Name).ToList();
            providerlist.Insert(0, new Provider() { Id = -1, Name = "All Providers" });

            ViewBag.providerlist = providerlist;

            var companylist = _companyService.GetallCompany().OrderBy(x => x.Name).ToList();
            companylist.Insert(0, new Company() { Id = -1, Name = "All Companies" });
            ViewBag.Companylist = companylist;

            var userlist = _userservice.GetAllUsers().Where(x => x.IsAdmin == false).OrderBy(x => x.Name).ToList();
            userlist.Insert(0, new User() { Id = -1, FirstName = "Select All", LastName = "" });
            ViewBag.userlist = userlist;

            var sublist = new List<Company>();
            sublist.Insert(0, new Company() { Id = -1, Name = "All Subsidiary" });
            ViewBag.sublist = sublist;


            var zones = _helperSvc.GetallZones();
            var zonees = new List<Utility.GenericReponse2>();
            zonees.Add(new GenericReponse2 { Id = -1, Name = "All Regions" });
            foreach (var item in zones)
            {
                zonees.Add(new GenericReponse2 { Id = item.Id, Name = item.Name.ToUpper() });

            }

            ViewBag.regions = zonees;


            var states = _helperSvc.GetallStates().ToList<State>();
            var additional = new State { Id = -1, Name = "--Select All--" };

            states.Insert(0, additional);
            ViewBag.States = states;




            var planlist = _planService.GetallPlans();
            var planadd = new Plan { Id = -1, Name = "--Select All--" };
            planlist.Insert(0, planadd);


            ViewBag.planlist = planlist;





            return View(page);
        }




        [ActionName("ShowVerificationAnalysis")]
        public ActionResult ShowVerificationAnalysisPage(VerificationCodeAnalysisPage page)
        {
            ViewBag.AllVerification = _helperSvc.GetTotalNoofverification();
            ViewBag.AllAuthenticated = _helperSvc.GetAllAuthenticatedVerification();
            ViewBag.WithoutAuthentication = _helperSvc.GetwithoutAuthentication();
            ViewBag.GeneratedMobileApp = _helperSvc.GetMobileCount();
            ViewBag.GeneratedShortCode = _helperSvc.GetSmsCount();
            ViewBag.EnrolleeCount = _helperSvc.GetUniquesEnrolleeGenerated();
            ViewBag.ProviderCount = _helperSvc.GetUniqueProvidersAuntenticated();


            var week1start = CurrentRequestData.Now.Date;
            var starttime = "00:01";
            var endtime = "23:59";

            var week1startNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 1,
                    week1start.Year, starttime));

            var week1EndNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 6,
                    week1start.Year, endtime));


            var week2startNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 7,
                    week1start.Year, starttime));


            var week2EndNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 13,
                    week1start.Year, endtime));


            var week3startNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 14,
                    week1start.Year, starttime));

            var week3EndNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 20,
                    week1start.Year, endtime));

            var week4startNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 21,
                    week1start.Year, starttime));

            var week4EndNew =
                Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", week1start.Month, 30,
                    week1start.Year, endtime));


            ViewBag.EWeek1 = _helperSvc.GetTotalNoofverificationByDate(week1startNew, week1EndNew);
            ViewBag.EWeek2 = _helperSvc.GetTotalNoofverificationByDate(week2startNew, week2EndNew);
            ViewBag.EWeek3 = _helperSvc.GetTotalNoofverificationByDate(week3startNew, week3EndNew);
            ViewBag.EWeek4 = _helperSvc.GetTotalNoofverificationByDate(week4startNew, week4EndNew);


            ViewBag.PWeek1 = _helperSvc.GetAllAuthenticatedVerificationByDate(week1startNew, week1EndNew);
            ViewBag.PWeek2 = _helperSvc.GetAllAuthenticatedVerificationByDate(week2startNew, week2EndNew);
            ViewBag.PWeek3 = _helperSvc.GetAllAuthenticatedVerificationByDate(week3startNew, week3EndNew);
            ViewBag.PWeek4 = _helperSvc.GetAllAuthenticatedVerificationByDate(week4startNew, week4EndNew);

            return View(page);
        }

        [ActionName("ShowVerification")]
        public ActionResult ShowVerificationPage(VerificationCodePage page)
        {
            var providerlist = _providerSvc.GetallProvider().OrderBy(x => x.Name).ToList();
            providerlist.Insert(0, new Provider() { Id = -1, Name = "All Providers" });


            ViewBag.providerlist = providerlist;
            var evstypeofvisit = Enum.GetValues(typeof(evsvisittype));
            ViewBag.evstypeofvisit = (from object item in evstypeofvisit
                                      select new DdListItem()
                                      {
                                          Id = Convert.ToString((int)item),
                                          Name = Enum.GetName(typeof(evsvisittype), item)
                                      }).ToList();


            return View(page);
        }

        [ActionName("ProviderAuthenticate")]
        public ActionResult ProviderAuthenticate(ProviderAuthenticatePage page)
        {

            return View(page);
        }

        [ActionName("ProviderAuthenticateResult")]
        public ActionResult ProviderAuthenticateResult(ProviderAuthenticateResultPage page)
        {
            var vercode = (string)Session["ProviderAuthenticateEnrollee"];
            var providercode = (string)Session["ProviderAuthenticateEnrolleeProviderCode"];


            var provider = _providerSvc.GetProvider(Convert.ToInt32(providercode));



            if (!string.IsNullOrEmpty(vercode) && !string.IsNullOrEmpty(providercode) && provider != null)
            {

                var verification = _helperSvc.GetverificationByVerificationCode(vercode);
                var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);

                page.Fullname = enrollee.Surname + " " + enrollee.Othernames;
                page.Name = "Enrollee Verified : " + page.Fullname;
                page.Gender = Enum.GetName(typeof(Sex), enrollee.Sex);
                page.PolicyNumber = enrollee.Policynumber.ToUpper();
                page.Passport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw);
                page.Company =
                    _companyService.Getsubsidiary(_companyService.Getstaff(enrollee.Staffprofileid).CompanySubsidiary)
                        .Subsidaryname.ToUpper();

                page.Message = "The Verification Code is Valid,Kindly attend to the bearer whose details appears above.";

                verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                verification.DateAuthenticated = CurrentRequestData.Now;
                verification.AuthChannel = (int)ChannelType.Web;
                verification.ProviderId = Convert.ToInt32(providercode);
                verification.Note = "The Verification code was authenticated.";

                page.Verificationcode = vercode;

                if (provider != null)
                {
                    page.Hospital = provider.Name;
                }
                else
                {
                    page.Hospital = "NIL";
                }

                _helperSvc.Updateverification(verification);

                Session["ProviderAuthenticateEnrollee"] = null;
                Session["ProviderAuthenticateEnrolleeProviderCode"] = null;
            }
            else
            {

                _pageMessageSvc.SetErrormessage("The UPN is Invalid.");
                return _uniquePageService.RedirectTo<ProviderAuthenticatePage>();
            }

            return View(page);
        }

        [HttpPost]
        [ActionName("PostProviderAuthenticate")]
        public ActionResult PostProviderAuthenticate(ProviderAuthenticatePage page, FormCollection form,
            ProviderAuthenticateResultPage result)
        {

            var providercode = form["Providercode"];
            var verCode = form["Verificationcode"];
            var reply = "";


            if (!string.IsNullOrEmpty(providercode) && !string.IsNullOrEmpty(verCode))
            {

                //check if provider exist.
                var provider = _providerSvc.GetProvider(Convert.ToInt32(providercode));


                if (verCode.ToLower().Contains("nha"))
                {

                    var enrolle = _enrolleeService.GetEnrolleeByPolicyNumber(verCode);
                    var hassub = false;
                    if (enrolle != null)
                    {

                        var staff = _companyService.Getstaff(enrolle.Staffprofileid);
                        var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                        var sub_sub =
                            _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                        //var validsub = comp_sub || sub_sub;

                        if (sub_sub || comp_sub)
                        {
                            var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                staff.CompanySubsidiary);

                            var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                            if (Ssubscription != null &&
                                (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                            {
                                hassub = true;
                            }


                            if (csubscription != null &&
                                (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                            {
                                hassub = true;
                            }
                            //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                            //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                        }




                        if (!hassub)
                        {

                            reply =
                                "Your subscription has expired kindly contact your HR.Thank you.";
                        }
                    }

                    if (enrolle != null || provider == null && hassub)
                    {
                        var verificationcode = _helperSvc.GenerateVerificationCode();
                        var verification2 = new EnrolleeVerificationCode();
                        verification2.EnrolleeId = enrolle.Id;
                        verification2.VerificationCode = verificationcode;
                        verification2.EncounterDate = CurrentRequestData.Now;
                        verification2.CreatedBy = 1;
                        verification2.Channel = (int)ChannelType.Web;
                        verification2.RequestPhoneNumber = "0";
                        verification2.Note = "Verification code was sent to enrollee for hospital access.";
                        verification2.Status = EnrolleeVerificationCodeStatus.Authenticated;
                        verification2.DateAuthenticated = CurrentRequestData.Now;
                        verification2.AuthChannel = (int)ChannelType.Web;
                        verification2.ProviderId = Convert.ToInt32(providercode);
                        verification2.Note = "Verification Code was authenticated. Generated By Provider";

                        _helperSvc.Addverification(verification2);

                        verCode = verification2.VerificationCode;
                    }
                    else
                    {



                        if (enrolle == null)
                        {
                            reply = "The Policy Number is  not Valid.";
                        }
                        else
                        {
                            reply = "The UPN is not valid.";
                        }



                    }


                }

                var verification = _helperSvc.GetverificationByVerificationCode(verCode.Trim());
                if (verification != null)
                {
                    var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                    if (verification.Status == EnrolleeVerificationCodeStatus.Expired)
                    {
                        //Expired 
                        reply = "The Verification Code has expired, Kindly generate a new code. Thank you";


                    }

                    if (enrollee.Isexpundged)
                    {
                        reply =
                            "The Enrollee has been expunged from our list.Kindly contact our customer service for futher clarification.Thank you";

                    }

                    //if (verification.Status == EnrolleeVerificationCodeStatus.Authenticated)
                    //{
                    //    reply =
                    //       "The Verification Code have been used.";

                    //}



                }
                else
                {
                    reply = "The Verification code is invalid.Kindly check the code and try again.";
                }

                if (string.IsNullOrEmpty(reply))
                {
                    //its good


                    Session["ProviderAuthenticateEnrollee"] = verCode;
                    Session["ProviderAuthenticateEnrolleeProviderCode"] = providercode;
                    return _uniquePageService.RedirectTo<ProviderAuthenticateResultPage>();
                }
                else
                {

                    Session["ErrormessageAuthenticate"] = reply;
                    return _uniquePageService.RedirectTo<ProviderAuthenticatePage>();
                }
            }
            else
            {


                Session["ErrormessageAuthenticate"] =
                    "Please check the information you entered,you must fill the required information properly.";
                return _uniquePageService.RedirectTo<ProviderAuthenticatePage>();
            }



        }

        [ActionName("EnrolleeDetails")]
        public ActionResult EnrolleeDetailsPage(EnrolleeDetailsPage page, int? id)
        {
            var staff = new Staff();
            var idint = 0;
            if (int.TryParse(id.ToString(), out idint))
            {
                staff = _companyService.Getstaff(idint);
                ViewBag.Stafffullname = staff.StaffFullname.ToUpper();
                ViewBag.StaffID = idint;

            }

            var mode = CurrentRequestData.CurrentContext.Request["Mode"];
            var enrolleeid = CurrentRequestData.CurrentContext.Request["enrolleeID"];

            //do for dependant


            var maritallist = Enum.GetValues(typeof(MaritalStatus));
            ViewBag.MaritalStatus = (from object item in maritallist
                                     select new DdListItem()
                                     {
                                         Id = Convert.ToString((int)item),
                                         Name = Enum.GetName(typeof(MaritalStatus), item)
                                     }).ToList();


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sexx = (from object item in sexlist
                            select new DdListItem()
                            {
                                Id = Convert.ToString((int)item),
                                Name = Enum.GetName(typeof(Sex), item)
                            }).ToList();


            var sponsorlist = Enum.GetValues(typeof(Sponsorshiptype));
            ViewBag.Sponsorshiptype = (from object item in sponsorlist
                                       select new DdListItem()
                                       {
                                           Id = Convert.ToString((int)item),
                                           Name = _helperSvc.GetDescription((Sponsorshiptype)item)
                                       }).ToList();



            var company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId));
            var subsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary);

            ViewBag.Company = company != null ? company.Name.ToUpper() : "Deleted Company";

            ViewBag.CompanySubsidiary = subsidiary != null ? subsidiary.Subsidaryname.ToUpper() : "Deleted Subsidiary";
            PlanVm plan = null;
            if (idint > 0)
            {
                plan = _planService.GetPlan(_companyService.GetCompanyPlan(staff.StaffPlanid).Planid);


                if (plan == null)
                {
                    _pageMessageSvc.SetErrormessage("The Enrollee details could not be displayed because the plan has been deleted. please Contact Admin.");

                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer != null ? CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri : _uniquePageService.GetUniquePage<EnrolleeListPage>().AbsoluteUrl);
                }
                ViewBag.SubscriptionType = plan.Name.ToUpper();
            }
            else
            {
                ViewBag.SubscriptionType = string.Empty;
            }

            var states = _helperSvc.GetallStates();

            var providerrrs = _providerSvc.GetallProviderByPlan(plan.Id).OrderBy(x => x.Name);
            var p_provider = new List<GenericReponse2>();
            foreach (var item in providerrrs)
            {
                var P_item = new GenericReponse2();
                P_item.Id = item.Id;
                P_item.Name = item.Name.ToUpper() + " - " + item.Address.ToLower();
                p_provider.Add(P_item);
            }
            ViewBag.providerlist = p_provider;
            //Load the States
            page.States.Clear();
            page.States.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (var item in states)
            {

                page.States.Add(item);
            }
            Enrollee enrolleemodel;
            if (!string.IsNullOrEmpty(mode) && Convert.ToInt32(mode) == 2)
            {
                enrolleemodel = _enrolleeService.GetEnrollee(Convert.ToInt32(enrolleeid));
            }
            else
            {
                enrolleemodel = _enrolleeService.GetEnrollee(staff.Profileid);
            }

            if (enrolleemodel == null)
            {
                //stuff is empty 
                _pageMessageSvc.SetErrormessage("The Enrollee details could not be displayed please Contact Admin.");

                return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer != null ? CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri : _uniquePageService.GetUniquePage<EnrolleeListPage>().AbsoluteUrl);

            }
            ViewBag.idCardPrintedValue = enrolleemodel.IdCardPrinted;

            page.Enrolleemodel = enrolleemodel;

            ViewBag.enrolleeimg = Convert.ToBase64String(enrolleemodel.EnrolleePassport.Imgraw);
            //var plan = _companyService.Getallplan().Where(x => x.Companyid == enrolleemodel.Companyid).Take(1);
            ViewBag.DependentsEnabled = (bool)
                _companyService.GetCompanyPlan(
                        _companyService.Getstaff(enrolleemodel.Staffprofileid).StaffPlanid).
                    AllowChildEnrollee;
            ViewBag.LGAS = _helperSvc.GetLgainstate(enrolleemodel.Stateid);
            ViewBag.companyPlanId = staff.StaffPlanid;



            ViewBag.enrolleeimg = Convert.ToBase64String(enrolleemodel.EnrolleePassport.Imgraw);
            //
            ViewBag.Staff_StaffId = staff.StaffId;

            ViewBag.DateAddedEnrolle =
                Convert.ToDateTime(enrolleemodel.CreatedOn)
                    .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
            ViewBag.AddedByEnrollee = enrolleemodel.Createdby > 0
                ? _userservice.GetUser(enrolleemodel.Createdby).Name
                : "Auto Upload";
            ViewBag.companyPlanId = staff.StaffPlanid;
            if (enrolleemodel.Isexpundged)

            {



                var userexpun = _userservice.GetUser(enrolleemodel.Expungedby);

                ViewBag.ExpungedByEnrollee = userexpun != null ? userexpun.Name.ToUpper() : "System";
                ViewBag.DateExpungedEnrollee =
                    Convert.ToDateTime(enrolleemodel.Dateexpunged)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
            }
            return View(page);
        }

        [ActionName("GeneratePolicyNumber")]
        public ActionResult GeneratePolicyNumber(GeneratePolicyNumberPage page)
        {


            return View(page);
        }


        [ActionName("ShowSmsIncoming")]
        public ActionResult ShowSmsIncoming(SmsIncomingPage page)
        {

            //testemail
            //_helperSvc.PushUserNotification 
            var emailmsg = new QueuedMessage();
            emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
            emailmsg.ToAddress = "asije.anthony@gmail.com";
            emailmsg.Subject = "NovoHub Notification - Testing ";
            emailmsg.FromName = "NOVOHUB";
            emailmsg.Body = "Testing my system";

            _emailSender.AddToQueue(emailmsg);

            //var config = _smsservice.GetConfig();
            //page.Config = config ?? new SmsConfig();

            return View(page);
        }

        [ActionName("ShowSms")]
        public ActionResult ShowSms(SmsPage page)
        {

            var config = _smsservice.GetConfig();
            page.Config = config ?? new SmsConfig();

            return View(page);
        }

        //Action for Add View
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add(EnrolleePage page, FormCollection form)
        {

            //var images = form["photoInputFile"];
            //Get the form items.
            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFile"];
            //var stafffullname = form["Stafffullname"];
            var policynumber = form["policynumber"];
            var surname = form["surname"];
            var othernames = form["othernames"];
            var dob = form["datepicker"];
            //var age = form["age"];
            var maritalstatus = form["maritalstatusvalue"];
            var sex = form["sex"];
            var state = form["state"];
            var lga = form["lga"];
            var address = form["address"];
            var occupation = form["occupation"];
            var mobilenumber = form["mobilenumber"];
            var email = form["email"];
            var sponsorshiptype = form["sponsorshiptypevalue"];
            var sponsorshipothername = form["sponsorshipothername"];
            //var subscriptiontype = form["subscriptiontypevalue"];
            //var company = form["company"];
            var provider = form["provider"];
            var premedicalcondition = form["premedicalcondition"];
            var staffid = form["staffidd"];

            var validation = true;
            var errormessage = new StringBuilder();


            if (string.IsNullOrEmpty(surname))
            {
                validation = false;
                errormessage.AppendLine("The surname field is blank.");
            }

            if (string.IsNullOrEmpty(othernames))
            {
                validation = false;
                errormessage.AppendLine("The othername field is blank.");
            }
            var outdate = new DateTime();
            if (string.IsNullOrEmpty(dob) || (!string.IsNullOrEmpty(dob) && DateTime.TryParse(dob, out outdate) == false))
            {
                validation = false;
                errormessage.AppendLine("The dob field is blank or not in the correct format.");
            }


            if (string.IsNullOrEmpty(maritalstatus))
            {
                validation = false;
                errormessage.AppendLine("The marital status field is blank.");
            }

            if (string.IsNullOrEmpty(sex))
            {
                validation = false;
                errormessage.AppendLine("The sex field is blank.");
            }
            if (string.IsNullOrEmpty(state) || (!string.IsNullOrEmpty(state) && Convert.ToInt32(state) < 1))
            {
                validation = false;
                errormessage.AppendLine("The state field is empty.");
            }
            if (string.IsNullOrEmpty(lga) || (!string.IsNullOrEmpty(state) && Convert.ToInt32(lga) < 1))
            {
                validation = false;
                errormessage.AppendLine("The LGA field is empty.");
            }

            if (string.IsNullOrEmpty(address))
            {
                validation = false;
                errormessage.AppendLine("The residential address  field is empty.");
            }
            if (string.IsNullOrEmpty(provider))
            {
                validation = false;
                errormessage.AppendLine("The provider address  field is empty.");
            }
            if (string.IsNullOrEmpty(policynumber))
            {
                validation = false;
                errormessage.AppendLine("The policynumber  field is empty.");
            }


            if (!validation)
            {
                //the form is not valid return
                _pageMessageSvc.SetErrormessage(string.Format("The form was not properly filled, {0}", errormessage.ToString()));

                return
                    Redirect(string.Format(_uniquePageService.GetUniquePage<EnrolleePage>().AbsoluteUrl + "?id={0}",
                        staffid));
            }

            var today = CurrentRequestData.Now;
            var dobtin = Convert.ToDateTime(dob);
            if (today.Year - dobtin.Year >= 70)
            {
                //above limit

                _pageMessageSvc.SetErrormessage("The Enrollee Age is above 70 , Dependant age must be below 70 years.");

                return
                    Redirect(string.Format(_uniquePageService.GetUniquePage<EnrolleePage>().AbsoluteUrl + "?id={0}",
                        staffid));
            }
            var staff = new Staff();
            var idint = 0;
            if (int.TryParse(staffid.ToString(), out idint))
            {
                staff = _companyService.Getstaff(idint);


            }
            //do image work
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }
            else
            {
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData = System.IO.File.ReadAllBytes(path);


            }

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                enrollee.Policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();

            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            //enrollee.Age = Convert.ToInt32(age);
            enrollee.Maritalstatus = Convert.ToInt32(maritalstatus);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = email;
            enrollee.Stateid = Convert.ToInt32(state);
            enrollee.Lgaid = Convert.ToInt32(lga);
            enrollee.Residentialaddress = address;
            enrollee.Occupation = occupation;
            enrollee.Sponsorshiptype = Convert.ToInt32(sponsorshiptype);
            enrollee.Sponsorshiptypeothername = sponsorshipothername;
            enrollee.Subscriptionplanid = staff.StaffPlanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = Convert.ToInt32(staffid);
            enrollee.Companyid = Convert.ToInt32(staff.CompanyId);
            enrollee.Status = (int)EnrolleesStatus.Active;

            enrollee.Createdby = CurrentRequestData.CurrentUser.Id;


            var response = _enrolleeService.AddEnrollee(enrollee, imgData);










            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Enrollee [{0}] was added successfully.",
                    enrollee.Policynumber.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding enrollee [{0}] ",
                    enrollee.Policynumber.ToUpper()));
            }


            //redirect to details page
            return
                Redirect(string.Format(_uniquePageService.GetUniquePage<EnrolleeDetailsPage>().AbsoluteUrl + "?id={0}",
                    staffid));


        }


        public bool AddAutomatics(FormCollection form, Staff staff)
        {
            //var images = form["photoInputFile"];
            //Get the form items.
            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFile"];
            //var stafffullname = form["Stafffullname"];
            var policynumber = form["policynumber"];
            var surname = form["surname"];
            var othernames = form["othernames"];
            var dob = form["datepicker"];
            //var age = form["age"];
            var maritalstatus = form["maritalstatusvalue"];
            var sex = form["sex"];
            var state = form["state"];
            var lga = form["lga"];
            var address = form["address"];
            var occupation = form["occupation"];
            var mobilenumber = form["mobilenumber"];
            var email = form["email"];
            var sponsorshiptype = form["sponsorshiptypevalue"];
            var sponsorshipothername = form["sponsorshipothername"];
            //var subscriptiontype = form["subscriptiontypevalue"];
            //var company = form["company"];
            var provider = form["provider"];
            var premedicalcondition = form["premedicalcondition"];
            var staffid = form["staffidd"];
            var reference = form["reference"];



            //do image work
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }
            else
            {
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData = System.IO.File.ReadAllBytes(path);


            }

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                enrollee.Policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();

            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            //enrollee.Age = Convert.ToInt32(age);
            enrollee.Maritalstatus = Convert.ToInt32(maritalstatus);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = email;
            enrollee.Stateid = Convert.ToInt32(state);
            enrollee.Lgaid = Convert.ToInt32(lga);
            enrollee.Residentialaddress = address;
            enrollee.Occupation = occupation;
            enrollee.Sponsorshiptype = Convert.ToInt32(sponsorshiptype);
            enrollee.Sponsorshiptypeothername = sponsorshipothername;
            enrollee.Subscriptionplanid = staff.StaffPlanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = Convert.ToInt32(staffid);
            enrollee.Companyid = Convert.ToInt32(staff.CompanyId);
            enrollee.Status = (int)EnrolleesStatus.Active;
            if (!string.IsNullOrEmpty(reference))
            {
                enrollee.RefPolicynumber = reference;
            }

            var response = _enrolleeService.AddEnrollee(enrollee, imgData);










            return response;

        }

        public bool AddAutomaticsWebEnrollee(TempEnrollee Enrollee, Staff staff)
        {


            //check if the enrollee still has no profile
            if (_companyService.CheckStaffProfileStatus(staff.Id))
            {
                return false;
            }

            int proID = 1;
            if (!string.IsNullOrEmpty(Enrollee.Primaryprovider) && int.TryParse(Enrollee.Primaryprovider, out proID))
            {

            }
            else
            {

                if (!string.IsNullOrEmpty(Enrollee.Primaryprovider))
                {
                    var prov = _providerSvc.GetProviderByName(Enrollee.Primaryprovider);
                    if (prov != null)
                    {
                        proID = prov.Id;
                    }

                }


            }

            var providerr = _providerSvc.GetProvider(proID); //_providerSvc.GetProviderByName(Enrollee.Primaryprovider);


            //var images = form["photoInputFile"];
            //Get the form items.
            var image = Enrollee.Imgraw;
            //var stafffullname = form["Stafffullname"];
            var policynumber = string.IsNullOrEmpty(Enrollee.Policynumber)
                ? _helperSvc.GeneratePolicyNumber(1, true).SingleOrDefault()
                : Enrollee.Policynumber;
            var surname = Enrollee.Surname;
            var othernames = Enrollee.Othernames;
            var dob = Enrollee.Dob;
            //var age = form["age"];
            var maritalstatus = Enrollee.Maritalstatus;
            var sex = Enrollee.Sex;
            var state = Enrollee.Stateofresidence;
            var lga = 1;
            var address = Enrollee.Residentialaddress;
            var occupation = Enrollee.Occupation;
            var mobilenumber = Enrollee.Mobilenumber;
            var email = Enrollee.Emailaddress;
            var sponsorshiptype = 0;
            var sponsorshipothername = string.Empty;
            //var subscriptiontype = form["subscriptiontypevalue"];
            //var company = form["company"];
            var provider = providerr != null ? providerr.Id : 1;
            var premedicalcondition = Enrollee.Preexistingmedicalhistory;
            var staffid = staff.Id;
            var reference = "";

            var staffplan = _companyService.GetCompanyPlan(staff.StaffPlanid);



            //do image work
            byte[] imgData = Enrollee.Imgraw;

            //if (image != null && image.ContentLength > 0)
            //{
            //    Image image2 = Image.FromStream(image.InputStream);

            //    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            //    var memoryStream = new MemoryStream();
            //    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    imgData = memoryStream.ToArray();

            //}
            //else
            //{
            //    var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

            //    imgData = System.IO.File.ReadAllBytes(path);


            //}

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                enrollee.Policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();

            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            //enrollee.Age = Convert.ToInt32(age);
            enrollee.Maritalstatus = Convert.ToInt32(maritalstatus);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = email;
            enrollee.Stateid = Convert.ToInt32(state);
            enrollee.Lgaid = Convert.ToInt32(lga);
            enrollee.Residentialaddress = address;
            enrollee.Occupation = occupation;
            enrollee.Sponsorshiptype = Convert.ToInt32(sponsorshiptype);
            enrollee.Sponsorshiptypeothername = sponsorshipothername;
            enrollee.Subscriptionplanid = staff.StaffPlanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = Convert.ToInt32(staffid);
            enrollee.Companyid = Convert.ToInt32(staff.CompanyId);
            enrollee.Status = (int)EnrolleesStatus.Active;
            enrollee.Createdby = 1;
            if (!string.IsNullOrEmpty(reference))
            {
                enrollee.RefPolicynumber = reference;
            }







            var response = _enrolleeService.AddEnrollee(enrollee, imgData);
            var exist = false;
            if (!response)
            {
                exist = _enrolleeService.GetEnrolleeByPolicyNumber(enrollee.Policynumber) != null;
            }
            if (response && staffplan.AllowChildEnrollee)
            {

                //add spouse and child if only the principal is in family plan

                if (Enrollee.addspouse)
                {
                    ////
                    var providers = 1;
                    int.TryParse(Enrollee.S_hospital, out providers);

                    var providerS = _providerSvc.GetProvider(providers);
                    //var form = new FormCollection();
                    //form.Add("photoInputFileD", Enrollee.S_Imgraw);
                    //form.Add("relationship", 0);
                    //form.Add("policynumberD","");
                    //form.Add("surnameD", Enrollee.s_Surname);
                    //form.Add("othernamesD", Enrollee.s_Othernames);
                    //form.Add("datepicker2", Enrollee.s_Dob);
                    //form.Add("sexD", Enrollee.S_Sex);
                    //form.Add("mobilenumberD", Enrollee.S_mobile);
                    //form.Add("providerD", providerS != null ? providerS.Id : 1);
                    //form.Add("premedicalcondition", Enrollee.S_medicalhistory);
                    //form.Add("principalid", enrollee.Id);
                    //form.Add("reference", "");

                    AddDependentAutomaticStaff(Enrollee.S_Imgraw, 0, "", Enrollee.s_Surname, Enrollee.s_Othernames,
                        Enrollee.s_Dob, Enrollee.S_Sex, Enrollee.S_mobile, providerS != null ? providerS.Id : 1,
                        Enrollee.S_medicalhistory, enrollee.Id, enrollee);
                }

                if (Enrollee.addchild1)
                {
                    //

                    var providers = 1;
                    int.TryParse(Enrollee.child1_hospital, out providers);
                    var providerS = _providerSvc.GetProvider(providers);


                    AddDependentAutomaticStaff(Enrollee.child1_Imgraw, 1, "", Enrollee.child1_Surname,
                        Enrollee.child1_Othernames, Enrollee.child1_Dob, Enrollee.child1_Sex, Enrollee.child1_mobile,
                        providerS != null ? providerS.Id : 1, Enrollee.child1_medicalhistory, enrollee.Id, enrollee);
                }

                if (Enrollee.addchild2)
                {
                    //
                    var providers = 1;
                    int.TryParse(Enrollee.child2_hospital, out providers);
                    var providerS = _providerSvc.GetProvider(providers);


                    AddDependentAutomaticStaff(Enrollee.child2_Imgraw, 1, "", Enrollee.child2_Surname,
                        Enrollee.child2_Othernames, Enrollee.child2_Dob, Enrollee.child2_Sex, Enrollee.child2_mobile,
                        providerS != null ? providerS.Id : 1, Enrollee.child2_medicalhistory, enrollee.Id, enrollee);
                }

                if (Enrollee.addchild3)
                {
                    //

                    var providers = 1;
                    int.TryParse(Enrollee.child3_hospital, out providers);
                    var providerS = _providerSvc.GetProvider(providers);

                    //var form = new FormCollection();
                    //form.Add("photoInputFileD", Enrollee.child3_Imgraw);
                    //form.Add("relationship", 1);
                    //form.Add("policynumberD", "");
                    //form.Add("surnameD", Enrollee.child3_Surname);
                    //form.Add("othernamesD", Enrollee.child3_Othernames);
                    //form.Add("datepicker2", Enrollee.child3_Dob);
                    //form.Add("sexD", Enrollee.child3_Sex);
                    //form.Add("mobilenumberD", Enrollee.child3_mobile);
                    //form.Add("providerD", providerS != null ? providerS.Id : 1);
                    //form.Add("premedicalcondition", Enrollee.child3_medicalhistory);
                    //form.Add("principalid", enrollee.Id);
                    //form.Add("reference", "");
                    AddDependentAutomaticStaff(Enrollee.child3_Imgraw, 1, "", Enrollee.child3_Surname,
                        Enrollee.child3_Othernames, Enrollee.child3_Dob, Enrollee.child3_Sex, Enrollee.child3_mobile,
                        providerS != null ? providerS.Id : 1, Enrollee.child3_medicalhistory, enrollee.Id, enrollee);
                }

                if (Enrollee.addchild4)
                {
                    //
                    var providers = 1;
                    int.TryParse(Enrollee.child4_hospital, out providers);
                    var providerS = _providerSvc.GetProvider(providers);


                    //var form = new FormCollection();
                    //form.Add("photoInputFileD", Enrollee.child4_Imgraw);
                    //form.Add("relationship", 1);
                    //form.Add("policynumberD", "");
                    //form.Add("surnameD", Enrollee.child4_Surname);
                    //form.Add("othernamesD", Enrollee.child4_Othernames);
                    //form.Add("datepicker2", Enrollee.child4_Dob);
                    //form.Add("sexD", Enrollee.child4_Sex);
                    //form.Add("mobilenumberD", Enrollee.child4_mobile);
                    //form.Add("providerD", providerS != null ? providerS.Id : 1);
                    //form.Add("premedicalcondition", Enrollee.child4_medicalhistory);
                    //form.Add("principalid", enrollee.Id);
                    //form.Add("reference", "");

                    AddDependentAutomaticStaff(Enrollee.child4_Imgraw, 1, "", Enrollee.child4_Surname,
                        Enrollee.child4_Othernames, Enrollee.child4_Dob, Enrollee.child4_Sex, Enrollee.child4_mobile,
                        providerS != null ? providerS.Id : 1, Enrollee.child4_medicalhistory, enrollee.Id, enrollee);
                }
            }



            return response;

        }


        public JsonResult GetDependentsJson(int? id)
        {
            var today = CurrentRequestData.Now;


            var dependentslist =
                _enrolleeService.GetDependentsEnrollee(Convert.ToInt32(id)).Where(
                        x => x.IsDeleted == false && x.Status == (int)EnrolleesStatus.Active)
                    .
                    ToList();
            var output = from areply in dependentslist
                         let proder = _providerSvc.GetProvider(areply.Primaryprovider)
                         select new
                         {
                             id = areply.Id,
                             name = areply.Surname + " " + areply.Othernames,
                             dob = Convert.ToDateTime(areply.Dob).ToString("MMM dd yyyy"),
                             sex = Enum.GetName(typeof(Sex), areply.Sex),
                             hospital = proder != null ? proder.Name.ToUpper() : "--",
                             mobile = areply.Mobilenumber,
                             preexisting = areply.Preexistingmedicalhistory,
                             relationship = Enum.GetName(typeof(Relationship), areply.Parentrelationship),
                             img =
                             areply.EnrolleePassport.Imgraw != null
                                 ? Convert.ToBase64String(areply.EnrolleePassport.Imgraw)
                                 : string.Empty,
                             policynum = areply.Policynumber,
                             aboveage = (today.Year - Convert.ToDateTime(areply.Dob).Year) >= 20 ? true : false
                         };


            var response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }

        public JsonResult GetVerificationsJson(int? id)
        {

            var verifications = _helperSvc.Getallverifications();

            var output = new List<VerificationCodeResponse>();
            foreach (var areply in verifications)
            {
                var responses = new VerificationCodeResponse();
                responses.Id = areply.Id.ToString();
                responses.Enrolleeid = areply.EnrolleeId.ToString();
                responses.EnrolleeisChild = _enrolleeService.GetEnrollee(areply.EnrolleeId).Parentid > 0;
                responses.StaffProfileId = _enrolleeService.GetEnrollee(areply.EnrolleeId).Staffprofileid.ToString();
                responses.EnrolleePolicy = _enrolleeService.GetEnrollee(areply.EnrolleeId).Policynumber;
                responses.Enrolleename =
                    _enrolleeService.GetEnrollee(areply.EnrolleeId).Surname + " " +
                    _enrolleeService.GetEnrollee(areply.EnrolleeId).Othernames;
                responses.Dateencountered =
                    Convert.ToDateTime(areply.EncounterDate)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                responses.Verificationcode = areply.VerificationCode;
                responses.Verficationstatus =
                    Enum.GetName(typeof(EnrolleeVerificationCodeStatus), areply.Status);


                var provider = _providerSvc.GetProvider(areply.ProviderId);
                if (provider != null && provider.Name != null)
                {
                    responses.Providerused = provider.Name;
                }
                else
                {
                    responses.Providerused = "--";
                }

                responses.Channel = Enum.GetName(typeof(ChannelType), areply.Channel);
                responses.Purpose = Enum.GetName(typeof(PurposeOfVisit), areply.VisitPurpose);
                responses.Dateauthenticated = areply.DateAuthenticated != null
                    ? Convert.ToDateTime(areply.DateAuthenticated)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern)
                    : "--";
                responses.Dateexpired = areply.Status != EnrolleeVerificationCodeStatus.Expired
                    ? "--"
                    : Convert.ToDateTime(areply.DateExpired)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);

                responses.Showcall = areply.Status == EnrolleeVerificationCodeStatus.Authenticated &&
                                     areply.Pickedup == false;
                responses.ShowEdit = (areply.AttendedTo && areply.PickedUpById == CurrentRequestData.CurrentUser.Id);
                responses.ShowCallToUser = (areply.AttendedTo == false && areply.Pickedup &&
                                            areply.PickedUpById == CurrentRequestData.CurrentUser.Id);

                output.Add(responses);
            }
            //var output = from areply in verifications
            //             select new
            //                 {
            //                     Id = areply.Id,
            //                     enrolleeid=areply.EnrolleeId,
            //                     enrolleeisChild=_enrolleeService.GetEnrollee(areply.EnrolleeId).Parentid > 0 ,
            //                     StaffProfileId=_enrolleeService.GetEnrollee(areply.EnrolleeId).Staffprofileid,
            //                     EnrolleePolicy = _enrolleeService.GetEnrollee(areply.EnrolleeId).Policynumber,
            //                     enrolleename =
            //                 _enrolleeService.GetEnrollee(areply.EnrolleeId).Surname + " " +
            //                 _enrolleeService.GetEnrollee(areply.EnrolleeId).Othernames,
            //                     dateencountered = areply.EncounterDate,
            //                     verificationcode = areply.VerificationCode,
            //                     verficationstatus =
            //                 Enum.GetName(typeof(EnrolleeVerificationCodeStatus), areply.Status),
            //                     providerused = areply.ProviderId > 0 ? _providerSvc.GetProvider(areply.ProviderId).Name : "--",
            //                     channel = Enum.GetName(typeof(ChannelType), areply.Channel),
            //                     purpose=Enum.GetName(typeof(PurposeOfVisit),areply.VisitPurpose),
            //                     dateauthenticated = areply.DateAuthenticated !=null?  Convert.ToDateTime( areply.DateAuthenticated).ToString("dd MM yyyy hh:mm tt") : "--",
            //                     dateexpired = areply.Status != EnrolleeVerificationCodeStatus.Expired ? "--" : Convert.ToDateTime(areply.DateExpired).ToString("dd MM yyyy hh:mm tt"),

            //                     showcall = areply.Status == EnrolleeVerificationCodeStatus.Authenticated  && areply.Pickedup ==false,
            //                     ShowEdit= (areply.AttendedTo && areply.PickedUpById  == CurrentRequestData.CurrentUser.Id ),
            //                     ShowCallToUser = (areply.AttendedTo == false && areply.Pickedup && areply.PickedUpById == CurrentRequestData.CurrentUser.Id),
            //                 };
            //recently authenticated first.

            output = output.ToList();
            var response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }

        public JsonResult QueryVerificationCode()
        {
            var draw = CurrentRequestData.CurrentContext.Request["draw"];
            var echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            var displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            var displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            var sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnnumber =
                CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnName =
                CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(
                    CultureInfo.CurrentCulture);
            var scrPolicynumber = CurrentRequestData.CurrentContext.Request["src_policynumber"];
            var scrVerificationCode = CurrentRequestData.CurrentContext.Request["src_verificationcode"];

            var scrMobilenumber = CurrentRequestData.CurrentContext.Request["scr_mobilenumber"];
            var scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];

            var scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            var scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            var scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];

            var scr_visittype = CurrentRequestData.CurrentContext.Request["scr_visittype"];


            var displayLength2 = CurrentRequestData.CurrentContext.Request["iDisplayLength2"];





            var toltareccount = 0;
            var totalinresult = 0;

            var showexpunge = 0;
            var fromdate = CurrentRequestData.Now;
            var todate = CurrentRequestData.Now;
            var usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = DateTime.ParseExact(scrFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                todate = DateTime.ParseExact(scrToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                usedate = Convert.ToBoolean(scruseDate);
            }

            var visitttt = evsvisittype.All;

            if (Convert.ToInt32(scr_visittype) > 0)
            {
                visitttt = (evsvisittype)Enum.Parse(typeof(evsvisittype), scr_visittype);

            }

            var providerid = 0;
            if (!string.IsNullOrEmpty(scrProvider))
            {
                int.TryParse(scrProvider.Split(',')[0], out providerid);

            }

            var verifications = _enrolleeService.QueryVerificationCode(out toltareccount, out totalinresult, "",
                Convert.ToInt32(displayStart),
                Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrPolicynumber.Trim(), providerid, scrVerificationCode, scrMobilenumber, usedate, fromdate, todate, visitttt);

            var output = new List<VerificationCodeResponse>();
            foreach (var areply in verifications)
            {
                var enrolleee = _enrolleeService.GetEnrollee(areply.EnrolleeId);

                if (enrolleee != null)
                {
                    var responses = new VerificationCodeResponse();
                    responses.Id = areply.Id.ToString();
                    responses.Enrolleeid = areply.EnrolleeId.ToString();

                    responses.EnrolleeisChild = enrolleee != null && enrolleee.Parentid > 0 ? true : false;
                    responses.StaffProfileId = enrolleee != null ? enrolleee.Staffprofileid.ToString() : "0";
                    responses.EnrolleePolicy = enrolleee != null ? enrolleee.Policynumber : "";

                    responses.Enrolleename =
                        enrolleee != null ? enrolleee.Surname + " " +
                        enrolleee.Othernames : "--";
                    responses.Dateencountered =
                        Convert.ToDateTime(areply.EncounterDate)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                    responses.Verificationcode = areply.VerificationCode;
                    responses.Verficationstatus =
                        Enum.GetName(typeof(EnrolleeVerificationCodeStatus), areply.Status);


                    var provider = _providerSvc.GetProvider(areply.ProviderId);
                    if (provider != null && provider.Name != null)
                    {
                        responses.Providerused = provider.Name;
                    }
                    else
                    {
                        responses.Providerused = "--";
                    }
                    responses.visittype = Enum.GetName(typeof(evsvisittype), areply.visittype);
                    responses.Channel = Enum.GetName(typeof(ChannelType), areply.Channel);
                    responses.Purpose = Enum.GetName(typeof(PurposeOfVisit), areply.VisitPurpose);
                    responses.Dateauthenticated = areply.DateAuthenticated != null
                        ? Convert.ToDateTime(areply.DateAuthenticated)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern)
                        : "--";
                    responses.Dateexpired = areply.Status != EnrolleeVerificationCodeStatus.Expired
                        ? "--"
                        : Convert.ToDateTime(areply.DateExpired)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);

                    responses.Showcall = areply.Status == EnrolleeVerificationCodeStatus.Authenticated &&
                                         areply.Pickedup == false;
                    responses.ShowEdit = (areply.AttendedTo && areply.PickedUpById == CurrentRequestData.CurrentUser.Id);
                    responses.ShowCallToUser = (areply.AttendedTo == false && areply.Pickedup &&
                                                areply.PickedUpById == CurrentRequestData.CurrentUser.Id);
                    output.Add(responses);
                }


            }
            //var output = from areply in verifications
            //             select new
            //                 {
            //                     Id = areply.Id,
            //                     enrolleeid=areply.EnrolleeId,
            //                     enrolleeisChild=_enrolleeService.GetEnrollee(areply.EnrolleeId).Parentid > 0 ,
            //                     StaffProfileId=_enrolleeService.GetEnrollee(areply.EnrolleeId).Staffprofileid,
            //                     EnrolleePolicy = _enrolleeService.GetEnrollee(areply.EnrolleeId).Policynumber,
            //                     enrolleename =
            //                 _enrolleeService.GetEnrollee(areply.EnrolleeId).Surname + " " +
            //                 _enrolleeService.GetEnrollee(areply.EnrolleeId).Othernames,
            //                     dateencountered = areply.EncounterDate,
            //                     verificationcode = areply.VerificationCode,
            //                     verficationstatus =
            //                 Enum.GetName(typeof(EnrolleeVerificationCodeStatus), areply.Status),
            //                     providerused = areply.ProviderId > 0 ? _providerSvc.GetProvider(areply.ProviderId).Name : "--",
            //                     channel = Enum.GetName(typeof(ChannelType), areply.Channel),
            //                     purpose=Enum.GetName(typeof(PurposeOfVisit),areply.VisitPurpose),
            //                     dateauthenticated = areply.DateAuthenticated !=null?  Convert.ToDateTime( areply.DateAuthenticated).ToString("dd MM yyyy hh:mm tt") : "--",
            //                     dateexpired = areply.Status != EnrolleeVerificationCodeStatus.Expired ? "--" : Convert.ToDateTime(areply.DateExpired).ToString("dd MM yyyy hh:mm tt"),

            //                     showcall = areply.Status == EnrolleeVerificationCodeStatus.Authenticated  && areply.Pickedup ==false,
            //                     ShowEdit= (areply.AttendedTo && areply.PickedUpById  == CurrentRequestData.CurrentUser.Id ),
            //                     ShowCallToUser = (areply.AttendedTo == false && areply.Pickedup && areply.PickedUpById == CurrentRequestData.CurrentUser.Id),
            //                 };
            //recently authenticated first.

            output = output.ToList();
            var response = Json(output, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetEnrolleesJson()
        {

            var draw = CurrentRequestData.CurrentContext.Request["draw"];
            var echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            var displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            var displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            var sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnnumber =
                CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnName =
                CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(
                    CultureInfo.CurrentCulture);
            var scrPolicynumber = CurrentRequestData.CurrentContext.Request["src_policynumber"];
            var scrOthername = CurrentRequestData.CurrentContext.Request["scr_othername"];
            var scrLastname = CurrentRequestData.CurrentContext.Request["scr_lastname"];
            var scrMobilenumber = CurrentRequestData.CurrentContext.Request["scr_mobilenumber"];
            var scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            var scrCompany = CurrentRequestData.CurrentContext.Request["scr_company"];
            var scrCompanySub = CurrentRequestData.CurrentContext.Request["CompanySubsidiary"];
            var scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            var scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            var scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            var scrshowExpungetype = CurrentRequestData.CurrentContext.Request["scr_expungetype"];
            var scrPrincipalType = CurrentRequestData.CurrentContext.Request["scr_PrincipalType"];
            var scrotherFilter = CurrentRequestData.CurrentContext.Request["scr_otherFilter"];
            var search = CurrentRequestData.CurrentContext.Request["sSearch"];
            var scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];
            var displayLength2 = CurrentRequestData.CurrentContext.Request["iDisplayLength2"];

            var scr_state = CurrentRequestData.CurrentContext.Request["scr_states"];
            var scr_region = CurrentRequestData.CurrentContext.Request["scr_region"];


            var plantype = CurrentRequestData.CurrentContext.Request["planstype"];
            var planmode = CurrentRequestData.CurrentContext.Request["plansmode"];

            var toltareccount = 0;
            var totalinresult = 0;

            var showexpunge = 0;
            var fromdate = CurrentRequestData.Now;
            var todate = CurrentRequestData.Now;
            var usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);
                usedate = Convert.ToBoolean(scruseDate);
            }

            if (scrshowExpungetype != null)
            {
                showexpunge = Convert.ToInt32(scrshowExpungetype);
            }
            var enrolleelist = _enrolleeService.QueryAllenrollee(out toltareccount, out totalinresult, search,
                Convert.ToInt32(displayStart),
                Convert.ToInt32(displayLength), sortColumnnumber, sortOrder, scrPolicynumber.Trim(), scrOthername.Trim(),
                scrOthername.Trim(), scrMobilenumber.Trim(), scrProvider.Trim(), scrCompany, scrCompanySub, usedate,
                fromdate, todate, showexpunge, scr_user, Convert.ToInt32(scrPrincipalType),
                Convert.ToInt32(scrotherFilter), Convert.ToInt32(scr_region), Convert.ToInt32(scr_state), Convert.ToInt32(plantype), Convert.ToInt32(planmode));

            var output = new List<Utility.EnrolleeModel>();
            var today = CurrentRequestData.Now;
            foreach (var areply in enrolleelist)
            {
                var model = new EnrolleeModel();
                model.Id = areply.Id;
                model.Name = areply.Surname + " " + areply.Othernames;
                //Images where disabled because of data.
                //model.Img = areply.EnrolleePassport.Imgraw != null
                //                ? Convert.ToBase64String(areply.EnrolleePassport.Imgraw)
                //                : string.Empty;
                model.PolicyNum = areply.Policynumber;
                model.DoB = Convert.ToDateTime(areply.Dob).ToShortDateString();
                model.Sex = Enum.GetName(typeof(Sex), areply.Sex);
                model.Occupation = areply.Occupation;
                model.Maritalstatus = areply.Parentid > 0
                    ? "NIL"
                    : Enum.GetName(typeof(MaritalStatus), areply.Maritalstatus);
                var provider = _providerSvc.GetProvider(areply.Primaryprovider);
                var state = _helperSvc.GetState(areply.Stateid);
                var company = _companyService.GetCompany(Convert.ToInt32(areply.Companyid));
                model.Hospital = provider != null ? provider.Name.ToUpper() : "Unknown";

                model.Mobile = areply.Mobilenumber;
                model.State = state != null ? state.Name.ToUpper() : "Unknown";
                model.Address = areply.Residentialaddress;
                model.Company = company != null ? company.Name.ToUpper() : "Unknown";
                model.IsDuplicate = areply.RefPolicynumber != null &&
                                    (areply.RefPolicynumber.Length > 10 ? true : false);
                model.Email = areply.Emailaddress ?? "NIL";
                model.DateCreated =
                    Convert.ToDateTime(areply.CreatedOn)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                //get the staff
                var staff = _companyService.Getstaff(areply.Staffprofileid);
                //try
                //{
                //    model.SubscriptionExpirationDate =
                //   Convert.ToDateTime(
                //       _companyService.GetSubscriptionByPlan(
                //           _companyService.Getstaff(areply.Staffprofileid).StaffPlanid).
                //           Expirationdate).ToShortDateString();
                //}
                //catch (Exception)
                //{

                //    model.SubscriptionExpirationDate = "NIL";
                //}


                //New expiration Tingy

                //check if company has any subscription
                var companyhassub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));

                //Stop giving subscription
                //var subsidiaryhassubscription =
                //    _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));


                if (companyhassub)
                {

                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                    if (Ssubscription != null)
                    {
                        model.SubscriptionExpirationDate = Ssubscription != null
                                                   ? Convert.ToDateTime(Ssubscription.Expirationdate).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern)
                                                   : "NIL";
                        model.HasSubscription = Ssubscription != null &&
                                                Ssubscription.Expirationdate > CurrentRequestData.Now;
                    }


                }
                else
                {
                    //no subscription.
                    model.SubscriptionExpirationDate = "NIL";
                }
                var subsidiary = _companyService.Getsubsidiary(
                                 staff.CompanySubsidiary);

                model.Subsidiary = subsidiary != null ? subsidiary.Subsidaryname.ToUpper() : "Unknown";

                model.isrenewal = company != null && company.isRenewal ? true : false;



                var custplan =
                    _companyService.GetCompanyPlan(staff.StaffPlanid);
                var plancover = "Deleted Plan";
                if (custplan != null)
                {
                    plancover = custplan.AllowChildEnrollee
                         ? "FAMILY"
                         : "INDIVIDUAL";


                }
                //_planService.GetPlan(
                //     _companyService.GetCompanyPlan(_companyService.Getstaff(areply.Staffprofileid).StaffPlanid).
                //         Planid);
                var planType = custplan != null ? _planService.GetPlan(custplan.Planid) : null;



                model.HealthPlan = custplan != null ? custplan.Planfriendlyname.ToUpper() + " (" + custplan.Description + ")" : "Deleted Plan";
                //plan.Name.ToUpper() + " " + plancover;
                model.Provider = provider != null ? provider.Name.ToUpper() : "--";




                model.IsChild = areply.Parentid > 0;
                model.IsExpunged = areply.Isexpundged;




                model.StaffProfileId = areply.Staffprofileid;
                model.IdPrinted = areply.IdCardPrinted;
                model.AboveLimit = ((today.Year - Convert.ToDateTime(areply.Dob).Year) >= 70 && model.IsChild == false) ||
                                   ((today.Year - Convert.ToDateTime(areply.Dob).Year) > 21 && model.IsChild == true &&
                                    areply.Parentrelationship == (int)Utility.Relationship.Child)
                    ? true
                    : false;
                output.Add(model);

            }

            //var output = from areply in enrolleelist
            //             select new
            //             {

            //                  Id = areply.Id,

            //                groupp= _companyService.GetCompany(areply.Companyid).Name.ToUpper(),
            //                img = Convert.ToBase64String(areply.EnrolleePassport.Imgraw),
            //                name= areply.Surname +" " +  areply.Othernames,
            //                policynum = areply.Policynumber,
            //                dob = Convert.ToDateTime(areply.Dob).ToString("MMM dd yyyy"),
            //                sex = Enum.GetName(typeof(Sex), areply.Sex),
            //                occupation = areply.Occupation,
            //               MaritalStatus =Enum.GetName(typeof(MaritalStatus),areply.Maritalstatus),
            //                hospital = _providerSvc.GetProvider(areply.Primaryprovider).Name.ToUpper(),
            //                mobile = areply.Mobilenumber,
            //               State = _helperSvc.GetState( areply.Stateid ).Name,
            //               address = areply.Residentialaddress,
            //                subsidiary = _companyService.Getstaff(areply.Staffprofileid).CompanySubsidiary < 1 ?  _companyService.Getsubsidiary( _companyService.Getstaff(areply.Staffprofileid).CompanySubsidiary).Subsidaryname : string.Empty,
            //                healthplan = _planService.GetPlan(_companyService.GetCompanyPlan(_companyService.Getstaff(areply.Staffprofileid).StaffPlanid).Planid).Name.ToUpper(),
            //                provider=_providerSvc.GetProvider(areply.Primaryprovider).Name.ToUpper(),


            //             };



            //if (!string.IsNullOrEmpty(search))
            //{
            //    search = search.ToLower();
            //    var rep =
            //        output.Where(x => x.Company.ToLower().Contains(search) || x.Subsidiary.ToLower().Contains(search) || x.PolicyNum.ToLower().Contains(search) || x.Occupation.ToLower().Contains(search) || x.Name.ToLower().Contains(search) ||x.Mobile.ToLower().Contains(search));
            //    totalinresult = rep.Count();

            //    output = rep.ToList();
            //}

            if (sortOrder == "asc")
            {

                switch (Convert.ToInt32(sortColumnnumber))
                {
                    case 0:
                        output = output.OrderBy(x => x.Id).ToList();
                        break;
                    case 2:
                        output = output.OrderBy(x => x.Name).ToList();
                        break;
                    case 3:
                        output = output.OrderBy(x => x.PolicyNum).ToList();
                        break;
                    case 4:
                        output = output.OrderBy(x => x.DoB).ToList();
                        break;
                    case 5:
                        output = output.OrderBy(x => x.Sex).ToList();
                        break;
                    case 6:
                        output = output.OrderBy(x => x.Occupation).ToList();
                        break;

                    case 7:
                        output = output.OrderBy(x => x.Maritalstatus).ToList();
                        break;

                    case 8:
                        output = output.OrderBy(x => x.State).ToList();
                        break;


                    case 9:
                        output = output.OrderBy(x => x.Address).ToList();
                        break;

                    case 10:
                        output = output.OrderBy(x => x.Company).ToList();
                        break;
                    case 11:
                        output = output.OrderBy(x => x.Subsidiary).ToList();
                        break;
                    case 12:
                        output = output.OrderBy(x => x.HealthPlan).ToList();
                        break;
                    case 13:
                        output = output.OrderBy(x => x.HasSubscription).ToList();
                        break;
                    case 14:
                        output = output.OrderBy(x => x.Mobile).ToList();
                        break;
                    case 15:
                        output = output.OrderBy(x => x.Hospital).ToList();
                        break;

                }



            }
            else
            {
                switch (Convert.ToInt32(sortColumnnumber))
                {
                    case 0:
                        output = output.OrderByDescending(x => x.Id).ToList();
                        break;
                    case 2:
                        output = output.OrderByDescending(x => x.Name).ToList();
                        break;
                    case 3:
                        output = output.OrderByDescending(x => x.PolicyNum).ToList();
                        break;
                    case 4:
                        output = output.OrderByDescending(x => x.DoB).ToList();
                        break;
                    case 5:
                        output = output.OrderByDescending(x => x.Sex).ToList();
                        break;
                    case 6:
                        output = output.OrderByDescending(x => x.Occupation).ToList();
                        break;

                    case 7:
                        output = output.OrderByDescending(x => x.Maritalstatus).ToList();
                        break;

                    case 8:
                        output = output.OrderByDescending(x => x.State).ToList();
                        break;


                    case 9:
                        output = output.OrderByDescending(x => x.Address).ToList();
                        break;

                    case 10:
                        output = output.OrderByDescending(x => x.Company).ToList();
                        break;
                    case 11:
                        output = output.OrderByDescending(x => x.Subsidiary).ToList();
                        break;
                    case 12:
                        output = output.OrderByDescending(x => x.HealthPlan).ToList();
                        break;
                    case 13:
                        output = output.OrderByDescending(x => x.HasSubscription).ToList();
                        break;
                    case 14:
                        output = output.OrderByDescending(x => x.Mobile).ToList();
                        break;
                    case 15:
                        output = output.OrderByDescending(x => x.Hospital).ToList();
                        break;

                }
            }
            var response = Json(output, JsonRequestBehavior.AllowGet);
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


        public JsonResult GetShortMessageJson()
        {

            var draw = CurrentRequestData.CurrentContext.Request["draw"];
            var echo = CurrentRequestData.CurrentContext.Request["sEcho"];
            var displayLength = CurrentRequestData.CurrentContext.Request["iDisplayLength"];
            var displayStart = CurrentRequestData.CurrentContext.Request["iDisplayStart"];
            var sortOrder = CurrentRequestData.CurrentContext.Request["sSortDir_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnnumber =
                CurrentRequestData.CurrentContext.Request["iSortCol_0"].ToString(CultureInfo.CurrentCulture);
            var sortColumnName =
                CurrentRequestData.CurrentContext.Request[string.Format("mDataProp_{0}", sortColumnnumber)].ToString(
                    CultureInfo.CurrentCulture);
            var scrPolicynumber = CurrentRequestData.CurrentContext.Request["src_policynumber"];
            var scrOthername = CurrentRequestData.CurrentContext.Request["scr_othername"];
            var scrLastname = CurrentRequestData.CurrentContext.Request["scr_lastname"];
            var scrMobilenumber = CurrentRequestData.CurrentContext.Request["src_MobileNumber"];
            var scrProvider = CurrentRequestData.CurrentContext.Request["scr_provider"];
            var scrMessage = CurrentRequestData.CurrentContext.Request["scr_Message"];

            var scruseDate = CurrentRequestData.CurrentContext.Request["scr_useDate"];
            var scrFromDate = CurrentRequestData.CurrentContext.Request["datepicker"];
            var scrToDate = CurrentRequestData.CurrentContext.Request["datepicker2"];
            var scrshowExpungetype = CurrentRequestData.CurrentContext.Request["scr_expungetype"];
            var scrPrincipalType = CurrentRequestData.CurrentContext.Request["scr_PrincipalType"];
            var scrotherFilter = CurrentRequestData.CurrentContext.Request["scr_otherFilter"];
            var search = CurrentRequestData.CurrentContext.Request["sSearch"];
            var scr_user = CurrentRequestData.CurrentContext.Request["scr_users"];


            var toltareccount = 0;
            var totalinresult = 0;

            var showexpunge = 0;
            var fromdate = CurrentRequestData.Now;
            var todate = CurrentRequestData.Now;
            var usedate = false;
            if (!string.IsNullOrEmpty(scrFromDate) && !string.IsNullOrEmpty(scrToDate))
            {
                fromdate = Convert.ToDateTime(scrFromDate);
                todate = Convert.ToDateTime(scrToDate);

                usedate = true;
            }


            var shortmsgLst = _helperSvc.QueryShortCodeMsgs(out toltareccount, out totalinresult, search,
                Convert.ToInt32(displayStart),
                Convert.ToInt32(displayLength), scrMobilenumber, scrMessage, usedate, fromdate, todate);







            var response = Json(shortmsgLst, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult DoExportIdcard(FormCollection form, MediaCategory mediaCategory)
        {
            var ids = form["hidden_selectedIDs"];


            if (!string.IsNullOrEmpty(ids))
            {
                string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                    "App_Data");
                var foldername = Guid.NewGuid().ToString();
                var fullpath = Path.Combine(appdatafolder, foldername);
                System.IO.Directory.CreateDirectory(fullpath);


                //write  the excels 

                //export all enrollees to excel
                var test = new DataTable();
                test.Columns.Add("S/N", typeof(string));
                test.Columns.Add("NAME", typeof(string));
                test.Columns.Add("POLICY NUMBER", typeof(string));
                test.Columns.Add("ADDRESS", typeof(string));
                test.Columns.Add("COMPANY", typeof(string));
                test.Columns.Add("HEALTH PLAN", typeof(string));
                test.Columns.Add("LANPP", typeof(string));
                test.Columns.Add("EXPIRATION", typeof(string));

                test.Columns.Add("Image Field", typeof(string));
                var enrolleslist = ids.Split(',');



                var count = 0;
                foreach (var itemid in enrolleslist)
                {



                    try
                    {



                        var item = _enrolleeService.GetEnrollee(Convert.ToInt32(itemid));
                        var staff = _companyService.Getstaff(item.Staffprofileid);
                        var plancover =
                            _companyService.GetCompanyPlan(_companyService.Getstaff(item.Staffprofileid).StaffPlanid).
                                AllowChildEnrollee
                                ? "FAMILY"
                                : "INDIVIDUAL";
                        var healthplan =

                            _planService.GetPlan(
                                _companyService.GetCompanyPlan(_companyService.Getstaff(item.Staffprofileid).StaffPlanid)
                                    .
                                    Planid).Name.ToUpper(); //+ " " + plancover;
                        var provider = _providerSvc.GetProvider(item.Primaryprovider);
                        var Lanpp = provider != null ? provider.Code : "--";
                        var company = item.Companyid;
                        var companysubscription = new Subscription();


                        //check if company has any subscription
                        var companyhassub =
                            _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                        var subsidiaryhassubscription =
                            _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));


                        if (companyhassub || subsidiaryhassubscription)
                        {
                            if (subsidiaryhassubscription)
                            {
                                var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                    staff.CompanySubsidiary);
                                companysubscription = Ssubscription;
                                //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                            }

                            if (companyhassub && subsidiaryhassubscription == false)
                            {
                                var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);
                                companysubscription = Ssubscription;
                            }
                        }



                        var startdate = CurrentRequestData.Now;
                        var expirationdate = CurrentRequestData.Now.AddYears(-1);
                        var imagefeild = string.Format("{0}.jpg", item.Policynumber);


                        if (companysubscription != null)
                        {
                            startdate = Convert.ToDateTime(companysubscription.Startdate);
                            expirationdate = Convert.ToDateTime(companysubscription.Expirationdate);

                        }

                        test.Rows.Add(Convert.ToInt32(count), (item.Surname + " " + item.Othernames).ToUpper(),
                            item.Policynumber.ToUpper(),
                            item.Residentialaddress, _companyService.Getsubsidiary(
                                    _companyService.Getstaff(item.Staffprofileid).CompanySubsidiary).Subsidaryname.
                                ToUpper(), healthplan, Lanpp.ToUpper(),
                            string.Format(new MyCustomDateProvider(), "{0}", expirationdate),
                            imagefeild);


                        //write image to folder
                        var image = ImageFromRawBgraArray(item.EnrolleePassport.Imgraw, 120, 120);
                        var imagurl = Path.Combine(fullpath, string.Format("{0}.jpg", item.Policynumber));
                        image.Save(imagurl, ImageFormat.Jpeg);


                        item.IdCardPrinted = true;
                        //_enrolleeService.UpdateEnrollee(item);

                        if (item.Parentid == 0)
                        {
                            count++;
                        }

                    }
                    catch (Exception ex)
                    {
                        var log = new Log();
                        log.Message = string.Format("There was a problem export this enrollee {0} {1}", itemid,
                            ex.Message, CurrentRequestData.Now.ToLongTimeString(),
                            CurrentRequestData.Now.ToLongDateString());
                        log.Type = LogEntryType.Audit;
                        log.Detail = "IDcard Export Error";

                        _logger.Insert(log);
                    }
                }


                var excelarray = DumpExcelGetByte(test);

                //write excel to folder

                System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

                //zip folder and send to client

                string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

                ZipFile.CreateFromDirectory(fullpath, zipPath);

                //create new downloadfile
                var file = new DownloadFile();

                file.fileName = "ID Card Export " + CurrentRequestData.Now.ToLongDateString() + ".zip";

                file.filelink = zipPath;
                file.filestaus = 1;
                file.createdby = CurrentRequestData.CurrentUser.Id;


                Stream fs = System.IO.File.OpenRead(zipPath);


                if (_fileService.IsValidFileType(file.fileName))
                {
                    ViewDataUploadFilesResult dbFile = _fileService.AddFile(fs, file.fileName,
                        "application/zip, application/octet-stream", fs.Length,

                        mediaCategory);


                    file.filelink = CurrentRequestData.CurrentSite.BaseUrl + dbFile.url;



                }


                _helperSvc.AddDownloadFile(file);


                //deleted the zip file

                System.IO.File.Delete(zipPath);

                //send back to user
                //Response.ContentType = "application/zip";
                //Response.AddHeader("content-disposition", "attachment;  filename=IDCardExport.zip");
                //Response.BinaryWrite(System.IO.File.ReadAllBytes(zipPath));

                _pageMessageSvc.SetSuccessMessage(
                    "Export was successful ,you can download file on this page.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage(
                    "You have not selected any enrollee to print id card for.Kindly select enrollees to print ID Card.");
            }
            return _uniquePageService.RedirectTo<DownloadFilesPage>();
        }


        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;

            return null;
        }


        public static Image ImageFromRawBgraArray(
            byte[] arr, int width, int height)
        {
            Bitmap newBitmap;
            using (MemoryStream memoryStream = new MemoryStream(arr))
            using (Image newImage = Image.FromStream(memoryStream))
                newBitmap = new Bitmap(newImage, 299, 379);
            return newBitmap;
        }

        public string RunUploadEnrollees()
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");

            var folderpath = Path.Combine(appdatafolder, "DropUpload");
            var filename = Path.Combine(folderpath, "FileUpload.xlsx");
            var exception = Path.Combine(folderpath, "ExceptionList.txt");
            var errorlist = new List<string>();
            var duplicatelist = new List<string>();
            System.IO.StreamReader file2 = new System.IO.StreamReader(exception);
            var txt = file2.ReadToEnd();


            byte[] file = System.IO.File.ReadAllBytes(filename);
            var ms = new MemoryStream(file);
            using (var package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    var strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    var count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            var scode = "";
                            var gender = "";
                            var dob = "";

                            var maritalStatus = "";

                            var enrolleeName = "";
                            var phoneNumber = "";
                            var occupation = "";
                            var homeAddress = "";
                            var healthPlan = "";
                            var providerid = "";
                            var provider = "";
                            var policynumber = "";
                            var company = "";
                            var email = "";
                            var expiration = "";
                            var remark = "";
                            var registrationdate = "";
                            var subsidiary = "";

                            var reference = "";
                            if (row != 1 && count <= 2)
                            {
                                scode = worksheet.Cells[row, 1].Text; // This got me the actual value I needed.
                                gender = worksheet.Cells[row, 2].Text;
                                dob = worksheet.Cells[row, 3].Text;
                                maritalStatus = worksheet.Cells[row, 4].Text;
                                enrolleeName = worksheet.Cells[row, 5].Text;
                                phoneNumber = worksheet.Cells[row, 6].Text;
                                occupation = worksheet.Cells[row, 7].Text;
                                homeAddress = worksheet.Cells[row, 8].Text;
                                healthPlan = worksheet.Cells[row, 9].Text;
                                providerid = worksheet.Cells[row, 10].Text;
                                provider = worksheet.Cells[row, 11].Text;
                                policynumber = worksheet.Cells[row, 12].Text;
                                company = worksheet.Cells[row, 13].Text;
                                email = worksheet.Cells[row, 14].Text;
                                expiration = worksheet.Cells[row, 15].Text;
                                remark = worksheet.Cells[row, 16].Text;
                                registrationdate = worksheet.Cells[row, 17].Text;
                                subsidiary = worksheet.Cells[row, 18].Text;
                                reference = worksheet.Cells[row, 19].Text;
                                policynumber = Regex.Replace(policynumber, @"\s+", "");
                                var regexMain = new Regex(@"NHA-\d{12}");
                                var regexDependents = new Regex(@"NHA-\d{12}-\d{1,}");


                                if (!string.IsNullOrEmpty(policynumber) && regexMain.IsMatch(policynumber) ||
                                    regexDependents.IsMatch(policynumber))
                                {


                                    var enrll = _enrolleeService.GetEnrolleeByPolicyNumber(policynumber.ToUpper());
                                    if (txt.Contains(policynumber.ToUpper()) || enrll != null)
                                    {
                                        //duplicate shii

                                        if (enrll != null)
                                        {


                                            if (enrolleeName.ToLower().Contains(enrll.Surname.ToLower()))
                                            {
                                                //update enrollee 
                                                var searchprovider =
                                                    _providerSvc.GetProviderByName(provider.ToUpper());

                                                if (searchprovider != null)
                                                {
                                                    enrll.Primaryprovider = searchprovider.Id;

                                                }
                                                DateTime donbb = DateTime.Now;
                                                var datee = DateTime.TryParse(dob, out donbb);

                                                _enrolleeService.UpdateEnrollee(enrll);
                                            }
                                        }


                                        duplicatelist.Add(string.Format("Duplicate : {0} {1}", policynumber,
                                            enrolleeName));
                                        continue;

                                    }

                                    if (regexMain.IsMatch(policynumber) && policynumber.Length == 16)
                                    {

                                        //check if the policynumber exist already



                                        //principal
                                        var staff = new Staff();


                                        var companyObj = _companyService.GetCompany(Convert.ToInt32(company));
                                        if (companyObj != null)
                                        {
                                            var subside = _companyService.GetAllSubsidiaryofACompany(companyObj.Id);
                                            var subMain =
                                                subside.FirstOrDefault(
                                                    x => x.Subsidaryname.ToLower() == subsidiary.ToLower().Trim());

                                            if (subMain.Id > 0)
                                            {
                                                staff.CompanyId = companyObj.Id.ToString();
                                                staff.CompanySubsidiary = subMain.Id;
                                                staff.StaffFullname = enrolleeName.ToUpper();

                                                //check the plan 
                                                var allplans = _companyService.GetallplanForCompany(companyObj.Id);
                                                var both = allplans.Where(x => x.Planid == 3);
                                                switch (healthPlan)
                                                {
                                                    case "AZUL FAMILY":

                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 3);

                                                        }

                                                        break;

                                                    case "AZUL INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 3);

                                                        }

                                                        break;

                                                    case "BASIC TSHIP":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1002);

                                                        }
                                                        break;

                                                    case "CUSTOMISED TSHIP":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1003);

                                                        }
                                                        break;

                                                    case "ENTERPRISE FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1);

                                                        }
                                                        break;
                                                    case "ENTERPRISE INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1);

                                                        }
                                                        break;
                                                    case "VERDE FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 2);

                                                        }
                                                        break;
                                                    case "VERDE INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 2);

                                                        }
                                                        break;
                                                    case "PLATINO INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 5);

                                                        }
                                                        break;
                                                    case "PLATINO FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 5);

                                                        }
                                                        break;
                                                    case "PLATINO GLOBAL INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 6);

                                                        }
                                                        break;
                                                    case "ROJO FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 4);

                                                        }
                                                        break;
                                                    case "ROJO INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 4);

                                                        }
                                                        break;


                                                    case "COMMUNITY HEALTH":
                                                        if (allplans != null)
                                                        {
                                                            both = allplans.Where(x => x.IsDeleted == false);
                                                        }

                                                        break;

                                                }

                                                try
                                                {


                                                    if (healthPlan.ToUpper().Contains("FAMILY"))
                                                    {
                                                        staff.StaffPlanid =
                                                            both.FirstOrDefault(x => x.AllowChildEnrollee).Id;
                                                    }
                                                    else
                                                    {
                                                        staff.StaffPlanid =
                                                            both.FirstOrDefault(x => x.AllowChildEnrollee == false).Id;
                                                    }


                                                    staff.Createdby = CurrentRequestData.CurrentUser.Id;
                                                    var response = _companyService.AddStaff(staff);

                                                    if (response)
                                                    {

                                                        var form = new FormCollection();
                                                        var names = staff.StaffFullname.Split(' ');
                                                        var sur = names[0];
                                                        var dobb = CurrentRequestData.Now;
                                                        var marrital = (int)Utility.MaritalStatus.Single;
                                                        var sexx = (int)Sex.Male;
                                                        var statee = 33;
                                                        var lgaa = 489;
                                                        var providerr = 1;

                                                        if (!string.IsNullOrEmpty(dob))
                                                        {
                                                            try
                                                            {
                                                                dobb = Convert.ToDateTime(dob);
                                                            }
                                                            catch (Exception)
                                                            {


                                                            }

                                                        }

                                                        if (dobb > Convert.ToDateTime("12/12/2016") ||
                                                            dobb < Convert.ToDateTime("1/12/1900"))
                                                        {
                                                            dobb = CurrentRequestData.Now;
                                                        }

                                                        if (maritalStatus.ToLower().Contains("m"))
                                                        {
                                                            marrital = (int)Utility.MaritalStatus.Married;
                                                        }


                                                        if (gender.ToLower().Contains("f"))
                                                        {
                                                            sexx = (int)Sex.Female;
                                                        }

                                                        var searchprovider =
                                                            _providerSvc.GetProviderByName(provider.ToUpper());

                                                        if (searchprovider != null)
                                                        {
                                                            providerr = searchprovider.Id;
                                                        }
                                                        form.Add("policynumber", policynumber);
                                                        form.Add("surname", sur);
                                                        form.Add("othernames",
                                                            staff.StaffFullname.Replace(sur + " ", ""));
                                                        form.Add("datepicker", dobb.ToShortDateString());
                                                        form.Add("maritalstatusvalue", marrital.ToString());
                                                        form.Add("sex", sexx.ToString());
                                                        form.Add("state", statee.ToString());
                                                        form.Add("lga", lgaa.ToString());
                                                        form.Add("address", homeAddress);
                                                        form.Add("occupation", occupation);
                                                        form.Add("mobilenumber", "0" + phoneNumber);
                                                        form.Add("email", email);
                                                        form.Add("sponsorshiptypevalue", "0");
                                                        form.Add("sponsorshipothername", "");
                                                        form.Add("provider", providerr.ToString());
                                                        form.Add("premedicalcondition", "NIL");
                                                        form.Add("staffidd", staff.Id.ToString());
                                                        form.Add("photoInputFile", "");
                                                        form.Add("reference", reference);
                                                        //proceed to add the 


                                                        //add enrollee

                                                        if (staff.Id > 0)
                                                        {
                                                            AddAutomatics(form, staff);

                                                        }



                                                    }



                                                }
                                                catch (Exception ex)
                                                {

                                                    _helperSvc.Log(LogEntryType.Error, null,
                                                        string.Format(
                                                            "There was a problem with the auto company stuff for company {0} for {1} {2}",
                                                            companyObj.Name, staff.StaffFullname, ex.Message),
                                                        "AUTO COMPANY");


                                                }

                                            }

                                            //staff.
                                        }




                                    }
                                    else
                                    {
                                        //dependants

                                        //var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];
                                        //var relationship = form["relationship"];
                                        //var policynumber = form["policynumberD"];
                                        //var surname = form["surnameD"];
                                        //var othernames = form["othernamesD"];
                                        //var dob = form["datepicker2"];
                                        //var sex = form["sexD"];
                                        //var mobilenumber = form["mobilenumberD"];
                                        //var provider = form["providerD"];
                                        //var premedicalcondition = form["premedicalconditionD"];
                                        //var principalid = form["PrincipalID"];

                                        if (policynumber.Length > 16 && policynumber.Contains('-'))
                                        {
                                            var principalpolicy = policynumber.Substring(0, 16);
                                            var principalenrollee =
                                                _enrolleeService.GetEnrolleeByPolicyNumber(principalpolicy.ToUpper());
                                            if (principalenrollee != null)
                                            {

                                                var relationship = 1;

                                                if (policynumber.EndsWith("-1"))
                                                {
                                                    relationship = 0;
                                                }
                                                var names = enrolleeName.Split(' ');
                                                var sur = names[0];
                                                var dobb = CurrentRequestData.Now;
                                                var marrital = (int)Utility.MaritalStatus.Single;
                                                var sexx = (int)Sex.Male;
                                                var statee = 33;
                                                var lgaa = 489;
                                                var providerr = 1;

                                                if (!string.IsNullOrEmpty(dob))
                                                {
                                                    try
                                                    {
                                                        dobb = Convert.ToDateTime(dob);
                                                    }
                                                    catch (Exception)
                                                    {


                                                    }

                                                }

                                                if (dobb > Convert.ToDateTime("12/12/2016") ||
                                                    dobb < Convert.ToDateTime("1/12/1900"))
                                                {
                                                    dobb = CurrentRequestData.Now;
                                                }
                                                if (gender.ToLower().Contains("f"))
                                                {
                                                    sexx = (int)Sex.Female;
                                                }

                                                var searchprovider =
                                                    _providerSvc.GetProviderByName(provider.ToUpper());

                                                if (searchprovider != null)
                                                {
                                                    providerr = searchprovider.Id;
                                                }
                                                //theres a principal
                                                var form = new FormCollection();
                                                form.Add("photoInputFileD", "");
                                                form.Add("relationship", relationship.ToString());
                                                form.Add("policynumberD", policynumber);
                                                form.Add("surnameD", sur);
                                                form.Add("othernamesD", enrolleeName.Replace(sur + " ", ""));
                                                form.Add("datepicker2", dobb.ToShortDateString());
                                                form.Add("sexD", sexx.ToString());
                                                form.Add("mobilenumberD", "0" + phoneNumber);
                                                form.Add("providerD", providerr.ToString());
                                                form.Add("premedicalcondition", "NIL");
                                                form.Add("principalid", principalenrollee.Id.ToString());
                                                form.Add("reference", reference);
                                                AddDependentAutomatic(form, principalenrollee);

                                            }
                                        }
                                    }
                                }
                            }



                        }
                    }

                }
            }
            var errorlistStr = errorlist.Aggregate(string.Empty,
                (current, item) => current + string.Format("{1} Item : {0}", item, Environment.NewLine));
            var log = new Log();
            log.Message = "Completed the bulk Upload.";
            log.Type = LogEntryType.Audit;
            log.Detail = errorlistStr;
            _logger.Insert(log);

            return "Done!";
        }

        public string RunUploadCustomEnrollees()
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");

            var folderpath = Path.Combine(appdatafolder, "DropUpload");
            var filename = Path.Combine(folderpath, "FileUpload.xlsx");
            var exception = Path.Combine(folderpath, "ExceptionList.txt");
            var errorlist = new List<string>();
            var duplicatelist = new List<string>();
            System.IO.StreamReader file2 = new System.IO.StreamReader(exception);
            var txt = file2.ReadToEnd();


            byte[] file = System.IO.File.ReadAllBytes(filename);
            var ms = new MemoryStream(file);
            using (var package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    var strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    var count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            var scode = "";
                            var gender = "";
                            var dob = "";

                            var maritalStatus = "";

                            var enrolleeName = "";
                            var phoneNumber = "";
                            var occupation = "";
                            var homeAddress = "";
                            var healthPlan = "";
                            var providerid = "";
                            var provider = "";
                            var policynumber = "";
                            var company = "";
                            var email = "";
                            var expiration = "";
                            var remark = "";
                            var registrationdate = "";
                            var subsidiary = "";

                            var reference = "";
                            if (row != 1 && count <= 2)
                            {
                                scode = worksheet.Cells[row, 1].Text; // This got me the actual value I needed.
                                gender = worksheet.Cells[row, 2].Text;
                                dob = worksheet.Cells[row, 3].Text;
                                maritalStatus = worksheet.Cells[row, 4].Text;
                                enrolleeName = worksheet.Cells[row, 5].Text;
                                phoneNumber = worksheet.Cells[row, 6].Text;
                                occupation = worksheet.Cells[row, 7].Text;
                                homeAddress = worksheet.Cells[row, 8].Text;
                                healthPlan = worksheet.Cells[row, 9].Text;
                                providerid = worksheet.Cells[row, 10].Text;
                                provider = worksheet.Cells[row, 11].Text;
                                policynumber = worksheet.Cells[row, 12].Text;
                                company = worksheet.Cells[row, 13].Text;
                                email = worksheet.Cells[row, 14].Text;
                                expiration = worksheet.Cells[row, 15].Text;
                                remark = worksheet.Cells[row, 16].Text;
                                registrationdate = worksheet.Cells[row, 17].Text;
                                subsidiary = worksheet.Cells[row, 18].Text;
                                reference = worksheet.Cells[row, 19].Text;
                                policynumber = Regex.Replace(policynumber, @"\s+", "");
                                var regexMain = new Regex(@"NHA-\d{12}");
                                var regexDependents = new Regex(@"NHA-\d{12}-\d{1,}");


                                if (!string.IsNullOrEmpty(policynumber) && regexMain.IsMatch(policynumber) ||
                                    regexDependents.IsMatch(policynumber))
                                {
                                    var enrll = _enrolleeService.GetEnrolleeByPolicyNumber(policynumber.ToUpper());
                                    if (txt.Contains(policynumber.ToUpper()) || enrll != null)
                                    {
                                        //duplicate shii
                                        if (regexDependents.IsMatch(policynumber))
                                        {

                                        }
                                        else
                                        {
                                            duplicatelist.Add(string.Format("Duplicate : {0} {1}", policynumber,
                                                enrolleeName));


                                            continue;
                                        }


                                    }
                                    if (regexMain.IsMatch(policynumber) && policynumber.Length == 16)
                                    {

                                        //check if the policynumber exist already



                                        //principal
                                        var staff = new Staff();


                                        var companyObj = _companyService.GetCompanyByName(company);
                                        if (companyObj != null)
                                        {
                                            var subside = _companyService.GetAllSubsidiaryofACompany(companyObj.Id);
                                            var subMain =
                                                subside.FirstOrDefault(
                                                    x => x.Subsidaryname.ToLower() == subsidiary.ToLower().Trim());

                                            if (subMain.Id > 0)
                                            {
                                                staff.CompanyId = companyObj.Id.ToString();
                                                staff.CompanySubsidiary = subMain.Id;
                                                staff.StaffFullname = enrolleeName.ToUpper();

                                                //check the plan 
                                                var allplans = _companyService.GetallplanForCompany(companyObj.Id);
                                                var both = allplans.Where(x => x.Planid == 3);
                                                switch (healthPlan)
                                                {
                                                    case "AZUL FAMILY":

                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 3);

                                                        }

                                                        break;

                                                    case "AZUL INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 3);

                                                        }

                                                        break;

                                                    case "BASIC TSHIP":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1002);

                                                        }
                                                        break;

                                                    case "CUSTOMISED TSHIP":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1003);

                                                        }
                                                        break;

                                                    case "ENTERPRISE FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1);

                                                        }
                                                        break;
                                                    case "ENTERPRISE INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 1);

                                                        }
                                                        break;
                                                    case "VERDE FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 2);

                                                        }
                                                        break;
                                                    case "VERDE INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 2);

                                                        }
                                                        break;
                                                    case "PLATINO INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 5);

                                                        }
                                                        break;
                                                    case "PLATINO FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 5);

                                                        }
                                                        break;
                                                    case "PLATINO GLOBAL INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 6);

                                                        }
                                                        break;
                                                    case "ROJO FAMILY":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 4);

                                                        }
                                                        break;
                                                    case "ROJO INDIVIDUAL":
                                                        if (allplans != null)
                                                        {

                                                            both = allplans.Where(x => x.Planid == 4);

                                                        }
                                                        break;


                                                    case "COMMUNITY HEALTH":
                                                        if (allplans != null)
                                                        {
                                                            both = allplans.Where(x => x.IsDeleted == false);
                                                        }

                                                        break;

                                                }

                                                try
                                                {


                                                    if (healthPlan.ToUpper().Contains("FAMILY"))
                                                    {
                                                        staff.StaffPlanid =
                                                            both.FirstOrDefault(x => x.AllowChildEnrollee).Id;
                                                    }
                                                    else
                                                    {
                                                        staff.StaffPlanid =
                                                            both.FirstOrDefault(x => x.AllowChildEnrollee == false).Id;
                                                    }


                                                    staff.Createdby = CurrentRequestData.CurrentUser.Id;
                                                    var response = _companyService.AddStaff(staff);

                                                    if (response)
                                                    {

                                                        var form = new FormCollection();
                                                        var names = staff.StaffFullname.Split(' ');
                                                        var sur = names[0];
                                                        var dobb = CurrentRequestData.Now;
                                                        var marrital = (int)Utility.MaritalStatus.Single;
                                                        var sexx = (int)Sex.Male;
                                                        var statee = 33;
                                                        var lgaa = 489;
                                                        var providerr = 1;

                                                        if (!string.IsNullOrEmpty(dob))
                                                        {
                                                            try
                                                            {
                                                                dobb = Convert.ToDateTime(dob);
                                                            }
                                                            catch (Exception)
                                                            {


                                                            }

                                                        }

                                                        if (dobb > Convert.ToDateTime("12/12/2016") ||
                                                            dobb < Convert.ToDateTime("1/12/1900"))
                                                        {
                                                            dobb = CurrentRequestData.Now;
                                                        }

                                                        if (maritalStatus.ToLower().Contains("m"))
                                                        {
                                                            marrital = (int)Utility.MaritalStatus.Married;
                                                        }


                                                        if (gender.ToLower().Contains("f"))
                                                        {
                                                            sexx = (int)Sex.Female;
                                                        }

                                                        var searchprovider =
                                                            _providerSvc.GetProviderByName(provider.ToUpper());

                                                        if (searchprovider != null)
                                                        {
                                                            providerr = searchprovider.Id;
                                                        }
                                                        form.Add("policynumber", policynumber);
                                                        form.Add("surname", sur);
                                                        form.Add("othernames",
                                                            staff.StaffFullname.Replace(sur + " ", ""));
                                                        form.Add("datepicker", dobb.ToShortDateString());
                                                        form.Add("maritalstatusvalue", marrital.ToString());
                                                        form.Add("sex", sexx.ToString());
                                                        form.Add("state", statee.ToString());
                                                        form.Add("lga", lgaa.ToString());
                                                        form.Add("address", homeAddress);
                                                        form.Add("occupation", occupation);
                                                        form.Add("mobilenumber", "0" + phoneNumber);
                                                        form.Add("email", email);
                                                        form.Add("sponsorshiptypevalue", "0");
                                                        form.Add("sponsorshipothername", "");
                                                        form.Add("provider", providerr.ToString());
                                                        form.Add("premedicalcondition", "NIL");
                                                        form.Add("staffidd", staff.Id.ToString());
                                                        form.Add("photoInputFile", "");
                                                        form.Add("reference", reference);
                                                        //proceed to add the 


                                                        //add enrollee

                                                        if (staff.Id > 0)
                                                        {
                                                            AddAutomatics(form, staff);

                                                        }



                                                    }



                                                }
                                                catch (Exception ex)
                                                {

                                                    _helperSvc.Log(LogEntryType.Error, null,
                                                        string.Format(
                                                            "There was a problem with the auto company stuff for company {0} for {1} {2}",
                                                            companyObj.Name, staff.StaffFullname, ex.Message),
                                                        "AUTO COMPANY");


                                                }

                                            }

                                            //staff.
                                        }




                                    }
                                    else
                                    {
                                        //dependants

                                        //var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];
                                        //var relationship = form["relationship"];
                                        //var policynumber = form["policynumberD"];
                                        //var surname = form["surnameD"];
                                        //var othernames = form["othernamesD"];
                                        //var dob = form["datepicker2"];
                                        //var sex = form["sexD"];
                                        //var mobilenumber = form["mobilenumberD"];
                                        //var provider = form["providerD"];
                                        //var premedicalcondition = form["premedicalconditionD"];
                                        //var principalid = form["PrincipalID"];

                                        if (policynumber.Length > 16 && policynumber.Contains('-'))
                                        {


                                            //check if the policynumber exist already



                                            //principal
                                            var staff = new Staff();


                                            var companyObj = _companyService.GetCompanyByName(company);
                                            if (companyObj != null)
                                            {
                                                var subside = _companyService.GetAllSubsidiaryofACompany(companyObj.Id);
                                                var subMain =
                                                    subside.FirstOrDefault(
                                                        x => x.Subsidaryname.ToLower() == subsidiary.ToLower().Trim());

                                                if (subMain.Id > 0)
                                                {
                                                    staff.CompanyId = companyObj.Id.ToString();
                                                    staff.CompanySubsidiary = subMain.Id;
                                                    staff.StaffFullname = enrolleeName.ToUpper();

                                                    //check the plan 
                                                    var allplans = _companyService.GetallplanForCompany(companyObj.Id);
                                                    var both = allplans.Where(x => x.Planid == 3);
                                                    switch (healthPlan)
                                                    {
                                                        case "AZUL FAMILY":

                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 3);

                                                            }

                                                            break;

                                                        case "AZUL INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 3);

                                                            }

                                                            break;

                                                        case "BASIC TSHIP":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 1002);

                                                            }
                                                            break;

                                                        case "CUSTOMISED TSHIP":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 1003);

                                                            }
                                                            break;

                                                        case "ENTERPRISE FAMILY":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 1);

                                                            }
                                                            break;
                                                        case "ENTERPRISE INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 1);

                                                            }
                                                            break;
                                                        case "VERDE FAMILY":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 2);

                                                            }
                                                            break;
                                                        case "VERDE INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 2);

                                                            }
                                                            break;
                                                        case "PLATINO INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 5);

                                                            }
                                                            break;
                                                        case "PLATINO FAMILY":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 5);

                                                            }
                                                            break;
                                                        case "PLATINO GLOBAL INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 6);

                                                            }
                                                            break;
                                                        case "ROJO FAMILY":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 4);

                                                            }
                                                            break;
                                                        case "ROJO INDIVIDUAL":
                                                            if (allplans != null)
                                                            {

                                                                both = allplans.Where(x => x.Planid == 4);

                                                            }
                                                            break;


                                                        case "COMMUNITY HEALTH":
                                                            if (allplans != null)
                                                            {
                                                                both = allplans.Where(x => x.IsDeleted == false);
                                                            }

                                                            break;

                                                    }

                                                    try
                                                    {


                                                        if (healthPlan.ToUpper().Contains("FAMILY"))
                                                        {
                                                            staff.StaffPlanid =
                                                                both.FirstOrDefault(x => x.AllowChildEnrollee).Id;
                                                        }
                                                        else
                                                        {
                                                            staff.StaffPlanid =
                                                                both.FirstOrDefault(x => x.AllowChildEnrollee == false)
                                                                    .Id;
                                                        }


                                                        staff.Createdby = CurrentRequestData.CurrentUser.Id;
                                                        var response = _companyService.AddStaff(staff);

                                                        if (response)
                                                        {

                                                            var form = new FormCollection();
                                                            var names = staff.StaffFullname.Split(' ');
                                                            var sur = names[0];
                                                            var dobb = CurrentRequestData.Now;
                                                            var marrital = (int)Utility.MaritalStatus.Single;
                                                            var sexx = (int)Sex.Male;
                                                            var statee = 33;
                                                            var lgaa = 489;
                                                            var providerr = 1;

                                                            if (!string.IsNullOrEmpty(dob))
                                                            {
                                                                try
                                                                {
                                                                    dobb = Convert.ToDateTime(dob);
                                                                }
                                                                catch (Exception)
                                                                {


                                                                }

                                                            }

                                                            if (dobb > Convert.ToDateTime("12/12/2016") ||
                                                                dobb < Convert.ToDateTime("1/12/1900"))
                                                            {
                                                                dobb = CurrentRequestData.Now;
                                                            }

                                                            if (maritalStatus.ToLower().Contains("m"))
                                                            {
                                                                marrital = (int)Utility.MaritalStatus.Married;
                                                            }


                                                            if (gender.ToLower().Contains("f"))
                                                            {
                                                                sexx = (int)Sex.Female;
                                                            }

                                                            var searchprovider =
                                                                _providerSvc.GetProviderByName(provider.ToUpper());

                                                            if (searchprovider != null)
                                                            {
                                                                providerr = searchprovider.Id;
                                                            }
                                                            form.Add("policynumber", policynumber);
                                                            form.Add("surname", sur);
                                                            form.Add("othernames",
                                                                staff.StaffFullname.Replace(sur + " ", ""));
                                                            form.Add("datepicker", dobb.ToShortDateString());
                                                            form.Add("maritalstatusvalue", marrital.ToString());
                                                            form.Add("sex", sexx.ToString());
                                                            form.Add("state", statee.ToString());
                                                            form.Add("lga", lgaa.ToString());
                                                            form.Add("address", homeAddress);
                                                            form.Add("occupation", occupation);
                                                            form.Add("mobilenumber", "0" + phoneNumber);
                                                            form.Add("email", email);
                                                            form.Add("sponsorshiptypevalue", "0");
                                                            form.Add("sponsorshipothername", "");
                                                            form.Add("provider", providerr.ToString());
                                                            form.Add("premedicalcondition", "NIL");
                                                            form.Add("staffidd", staff.Id.ToString());
                                                            form.Add("photoInputFile", "");
                                                            form.Add("reference", reference);
                                                            //proceed to add the 


                                                            //add enrollee

                                                            var Penrollee =
                                                                _enrolleeService.GetEnrolleeByPolicyNumber(policynumber);
                                                            if (Penrollee != null)
                                                            {
                                                                _enrolleeService.DeletEnrollee(Penrollee);
                                                            }

                                                            if (staff.Id > 0)
                                                            {
                                                                AddAutomatics(form, staff);

                                                            }


                                                        }



                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        _helperSvc.Log(LogEntryType.Error, null,
                                                            string.Format(
                                                                "There was a problem with the auto company stuff for company {0} for {1} {2}",
                                                                companyObj.Name, staff.StaffFullname, ex.Message),
                                                            "AUTO COMPANY");


                                                    }

                                                }

                                                //staff.
                                            }


                                        }
                                    }



                                }
                            }

                        }


                    }
                    var errorlistStr = errorlist.Aggregate(string.Empty,
                        (current, item) => current + string.Format("{1} Item : {0}", item, Environment.NewLine));
                    var log = new Log();
                    log.Message = "Completed the bulk Upload.";
                    log.Type = LogEntryType.Audit;
                    log.Detail = errorlistStr;
                    _logger.Insert(log);

                }
            }

            return "Done!";
        }

        public string RunUploadEnrolleesCapitation()
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");

            var folderpath = Path.Combine(appdatafolder, "DropUpload");
            var filename = Path.Combine(folderpath, "FileUpload.xlsx");
            var exception = Path.Combine(folderpath, "ExceptionList.txt");
            var errorlist = new List<string>();
            var duplicatelist = new List<string>();
            System.IO.StreamReader file2 = new System.IO.StreamReader(exception);
            var txt = file2.ReadToEnd();


            byte[] file = System.IO.File.ReadAllBytes(filename);
            var ms = new MemoryStream(file);
            using (var package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    var strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    var count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            var scode = "";
                            var gender = "";
                            var dob = "";

                            var maritalStatus = "";

                            var enrolleeName = "";
                            var phoneNumber = "";
                            var occupation = "";
                            var homeAddress = "";
                            var healthPlan = "";
                            var providerid = "";
                            var provider = "";
                            var policynumber = "";
                            var company = "";
                            var email = "";
                            var expiration = "";
                            var remark = "";
                            var registrationdate = "";
                            var subsidiary = "";

                            var reference = "";
                            if (row != 1 && count <= 2)
                            {
                                scode = worksheet.Cells[row, 1].Text; // This got me the actual value I needed.
                                provider = worksheet.Cells[row, 11].Text;
                                enrolleeName = worksheet.Cells[row, 3].Text;
                                policynumber = worksheet.Cells[row, 12].Text;

                                policynumber = Regex.Replace(policynumber, @"\s+", "");
                                var regexMain = new Regex(@"NHA-\d{12}");
                                var regexDependents = new Regex(@"NHA-\d{12}-\d{1,}");


                                if (!string.IsNullOrEmpty(policynumber) && regexMain.IsMatch(policynumber) ||
                                    regexDependents.IsMatch(policynumber))
                                {


                                    var enrll = _enrolleeService.GetEnrolleeByPolicyNumber(policynumber.ToUpper());

                                    if (enrll != null)
                                    {
                                        var newhospital = _providerSvc.GetProviderByName(provider.ToUpper());


                                        enrll.Primaryprovider = newhospital.Id;

                                        _enrolleeService.UpdateEnrollee(enrll);
                                    }


                                }
                            }



                        }
                    }

                }
            }
            var errorlistStr = errorlist.Aggregate(string.Empty,
                (current, item) => current + string.Format("{1} Item : {0}", item, Environment.NewLine));
            var log = new Log();
            log.Message = "Completed the bulk Upload.";
            log.Type = LogEntryType.Audit;
            log.Detail = errorlistStr;
            _logger.Insert(log);

            return "Done!";
        }


        public string DeleteBulkEnrollee()
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");

            var folderpath = Path.Combine(appdatafolder, "DropUpload");
            var filename = Path.Combine(folderpath, "DeleteEnrollee.xlsx");
            var errorlist = new List<string>();
            var duplicatelist = new List<string>();
            // System.IO.StreamReader file2 = new System.IO.StreamReader(exception);
            //var txt = file2.ReadToEnd();


            byte[] file = System.IO.File.ReadAllBytes(filename);
            var ms = new MemoryStream(file);
            using (var package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    var strError = "Your Excel file does not contain any work sheets";
                }
                else
                {
                    var count = 1;
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        var start = worksheet.Dimension.Start;
                        var end = worksheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        {
                            // Row by row...

                            var PolicyNumber = worksheet.Cells[row, 1].Text;


                            //get enrollee
                            //var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(PolicyNumber.Trim());

                            var familytree = _enrolleeService.GetFamilyTreeByPolicyNumber(PolicyNumber.Trim());

                            if (familytree != null && familytree.Count > 0)
                            {
                                var staff = _companyService.Getstaff(familytree[0].Staffprofileid);

                                if (staff != null)
                                {
                                    //delete staff

                                    _companyService.DeleteStaff(staff);

                                }
                                foreach (var item in familytree)
                                {
                                    _enrolleeService.DeletEnrollee(item);
                                }

                            }

                        }
                    }


                }
            }
            return "Done!";
        }

        public JsonResult GetSmsJson(int? id)
        {

            List<Sms> smslogs;
            if (Convert.ToInt32(id).Equals(0))
            {
                //all excluding pending.
                smslogs =
                    _smsservice.Getallmessages()
                        .Where(x => x.Status != (int)SmsStatus.Pending)
                        .OrderBy(x => x.CreatedOn)
                        .ToList();

            }
            else
            {
                //pending
                smslogs =
                    _smsservice.Getallmessages()
                        .Where(x => x.Status == (int)SmsStatus.Pending)
                        .OrderBy(x => x.CreatedOn)
                        .ToList();
            }
            var output = from areply in smslogs
                         select new
                         {
                             Id = areply.Id,
                             fromid = areply.FromId,
                             destination = areply.Msisdn,
                             enrolleepolicy =
                             areply.EnrolleeId > 0 ? _enrolleeService.GetEnrollee(areply.EnrolleeId).Policynumber : "Custom",
                             enrolleename =
                             areply.EnrolleeId > 0
                                 ? (_enrolleeService.GetEnrollee(areply.EnrolleeId).Surname + " " +
                                    _enrolleeService.GetEnrollee(areply.EnrolleeId).Othernames).ToUpper()
                                 : "Custom",
                             message = areply.Message,
                             messageType = Enum.GetName(typeof(SmsType), areply.Type),
                             status = areply.Status,
                             created = Convert.ToDateTime(areply.CreatedOn).ToString("dd/MM/yyyy hh:mm tt"),
                             deleiverydate = Convert.ToDateTime(areply.DeliveryDate).ToString("dd/MM/yyyy hh:mm tt"),
                             datedelivered = Convert.ToDateTime(areply.DateDelivered).ToString("dd/MM/yyyy hh:mm tt"),
                             serverresponse = areply.ServerResponse ?? "--"
                         };

            output = output.OrderByDescending(x => x.Id);


            var response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });



        }

        public JsonResult GeneratePolicyNumberJson(int? count)
        {

            if (count > 0)
            {
                var temp = _helperSvc.GeneratePolicyNumber(Convert.ToInt32(count), true);
                var sncount = 1;
                var output = from areply in temp
                             select new
                             {
                                 sn = sncount++,
                                 policynumber = areply

                             };
                var response = Json(output, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    aaData = response.Data
                });

            }



            return Json(null, JsonRequestBehavior.AllowGet);


        }

        public string returnEnrolleeNumbers()
        {
            return _enrolleeService.GetConcatenatedPhoneNumberString(SMSLeads.OnlyPrincipal);


        }

        [HttpGet]
        public ActionResult AddDependent(int? id)
        {
            var idint = 0;
            var enrollee = new Enrollee();
            if (int.TryParse(Convert.ToString(id), out idint) && idint > 0)
            {
                enrollee = _enrolleeService.GetEnrollee(idint);


            }

            if (enrollee == null)
            {
                _pageMessageSvc.SetErrormessage("Error adding Dependant it seems like the pricipal enrollee does not exist or deleted.");
                return _uniquePageService.RedirectTo<EnrolleeListPage>();
            }

            var relationshiplist = Enum.GetValues(typeof(Relationship));
            ViewBag.relationshiplist = (from object item in relationshiplist
                                        select new DdListItem()
                                        {
                                            Id = Convert.ToString((int)item),
                                            Name = Enum.GetName(typeof(Relationship), item)
                                        }).ToList();
            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();

            var staff = _companyService.Getstaff(enrollee.Staffprofileid);

            PlanVm plan = null;
            var plannn = _companyService.GetCompanyPlan(staff.StaffPlanid);
            plan = _planService.GetPlan(plannn.Planid);
            var providerrrs = _providerSvc.GetallProviderByPlan(plan.Id).OrderBy(x => x.Name);
            var p_provider = new List<GenericReponse2>();
            foreach (var item in providerrrs)
            {
                var P_item = new GenericReponse2();
                P_item.Id = item.Id;
                P_item.Name = item.Name.ToUpper() + " - " + item.Address.ToLower();
                p_provider.Add(P_item);
            }
            ViewBag.providerlist = p_provider;
            return PartialView("AddDependent", enrollee);
        }

        [HttpGet]
        public ActionResult EditDependent(int id)
        {
            var idint = 0;
            var enrollee = new Enrollee();
            if (int.TryParse(Convert.ToString(id), out idint) && idint > 0)
            {
                enrollee = _enrolleeService.GetEnrollee(idint);


            }

            var relationshiplist = Enum.GetValues(typeof(Relationship));
            ViewBag.relationshiplist = (from object item in relationshiplist
                                        select new DdListItem()
                                        {
                                            Id = Convert.ToString((int)item),
                                            Name = Enum.GetName(typeof(Relationship), item)
                                        }).ToList();
            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();

            var staff = _companyService.Getstaff(enrollee.Staffprofileid);
            PlanVm plan = null;
            var plannn = _companyService.GetCompanyPlan(staff.StaffPlanid);
            plan = _planService.GetPlan(plannn.Planid);
            var providerrrs = _providerSvc.GetallProviderByPlan(plan.Id).OrderBy(x => x.Name);
            var p_provider = new List<GenericReponse2>();
            foreach (var item in providerrrs)
            {
                var P_item = new GenericReponse2();
                P_item.Id = item.Id;
                P_item.Name = item.Name.ToUpper() + " - " + item.Address.ToLower();
                p_provider.Add(P_item);
            }
            ViewBag.providerlist = p_provider;
            ViewBag.enrolleeimg = enrollee.EnrolleePassport.Imgraw != null
                ? Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw)
                : string.Empty;
            return PartialView("EditDependent", enrollee);
        }

        [HttpPost]
        public ActionResult EditDependent(Enrollee enrolleemodel, FormCollection form)
        {
            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileED"];
            var relationship = form["relationship"];
            var policynumber = form["policynumberD"];
            var surname = form["surnameD"];
            var othernames = form["othernamesD"];
            var dob = form["datepicker"];
            var sex = form["sexD"];
            var mobilenumber = form["mobilenumberD"];
            var provider = form["providerD"];
            var premedicalcondition = form["premedicalconditionD"];

            var dependentid = form["dependantID"];
            var page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();
            var enrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(dependentid));
            //var principalenrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(principalid));
            //do image work
            var dobtin = Convert.ToDateTime(dob);
            DateTime today = CurrentRequestData.Now;

            var validation = true;
            var errormessage = new StringBuilder();
            if (string.IsNullOrEmpty(surname))
            {
                validation = false;
                errormessage.AppendLine("The surname field is blank.");
            }

            if (string.IsNullOrEmpty(othernames))
            {
                validation = false;
                errormessage.AppendLine("The othername field is blank.");
            }
            var outdate = new DateTime();
            if (string.IsNullOrEmpty(dob) || (!string.IsNullOrEmpty(dob) && DateTime.TryParse(dob, out outdate) == false))
            {
                validation = false;
                errormessage.AppendLine("The dob field is blank or not in the correct format.");
            }

            if (string.IsNullOrEmpty(sex))
            {
                validation = false;
                errormessage.AppendLine("The sex field is blank.");
            }

            if (string.IsNullOrEmpty(provider))
            {
                validation = false;
                errormessage.AppendLine("The provider address  field is empty.");
            }


            if (!validation)
            {
                //the form is not valid return
                _pageMessageSvc.SetErrormessage(string.Format("The form was not properly filled, {0}", errormessage.ToString()));
                return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", enrollee.Staffprofileid));

            }



            if (today.Year - dobtin.Year > 21 &&
                (Relationship)Enum.Parse(typeof(Relationship), relationship) == Relationship.Child)
            {
                //above limit

                _pageMessageSvc.SetErrormessage("The Dependant Age is above 21 , Dependant age must be below 21 years.");

                return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", enrollee.Staffprofileid));
            }
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            enrollee.Age = Convert.ToInt32(0);
            enrollee.Maritalstatus = Convert.ToInt32(MaritalStatus.Single);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            //enrollee.Emailaddress = principalenrollee.Emailaddress;
            //enrollee.Stateid = principalenrollee.Stateid;
            //enrollee.Lgaid = principalenrollee.Lgaid;
            //enrollee.Residentialaddress = principalenrollee.Residentialaddress;
            enrollee.Occupation = "None";
            // enrollee.Sponsorshiptype = principalenrollee.Sponsorshiptype;
            // enrollee.Sponsorshiptypeothername = principalenrollee.Sponsorshiptypeothername;
            //enrollee.Subscriptionplanid = 0;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            //enrollee.Staffprofileid = 0;
            //set enrolle parentd
            //enrollee.Parentid = principalenrollee.Id;
            enrollee.Parentrelationship = Convert.ToInt32(relationship);
            //enrollee.Companyid = principalenrollee.Companyid;
            //enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Status = (int)EnrolleesStatus.Active;


            //principalenrollee.Hasdependents = true;

            if (imgData != null && imgData.Length > 0)
            {
                enrollee.EnrolleePassport.Imgraw = imgData;
            }
            var response = _enrolleeService.UpdateEnrollee(enrollee);
            //var updateresp = _enrolleeService.UpdateEnrollee(enrollee);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Dependent [{0}] was updated successfully.",
                    enrollee.Policynumber.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updating Dependent [{0}] ",
                    enrollee.Policynumber.ToUpper()));
            }





            return
                Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2",
                    _enrolleeService.GetEnrollee(enrollee.Parentid).Staffprofileid));
        }



        [HttpGet]
        public ActionResult SmSConfig()
        {

            var rconfig = _smsservice.GetConfig();
            var config = new SmsConfig();
            if (rconfig != null)
            {
                config = rconfig;
            }
            var smstypelist = Enum.GetValues(typeof(SmsSend));
            ViewBag.MessageMode = (from object item in smstypelist
                                   select new DdListItem()
                                   {
                                       Id = Convert.ToString((int)item),
                                       Name = Enum.GetName(typeof(SmsSend), item)
                                   }).ToList();


            return PartialView("SmsConfig", rconfig);
        }

        [HttpPost]
        public ActionResult SmSConfig(SmsConfig smsConfig)
        {
            var save = false;
            var rconfig = _smsservice.GetConfig();
            if (rconfig == null)
            {
                //save config 
                save = _smsservice.SaveSmsConfig(smsConfig);

            }
            else
            {
                //update config;

                rconfig.UserName = smsConfig.UserName;
                rconfig.Password = string.IsNullOrEmpty(smsConfig.Password) ? rconfig.Password : smsConfig.Password;
                rconfig.Mode = smsConfig.Mode;
                rconfig.PreScheduleText = smsConfig.PreScheduleText;
                rconfig.BdaySmsTemplate = smsConfig.BdaySmsTemplate;
                rconfig.Active = smsConfig.Active;

                save = _smsservice.UpdateSmsConfig(rconfig);
            }


            if (save)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Sms Configuration  was saved successfully."
                ));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  saving sms config"));
            }
            return _uniquePageService.RedirectTo<SmsPage>();

        }


        [HttpGet]
        public ActionResult AttendToVerification(int? id)
        {

            var verification = _helperSvc.Getverification(Convert.ToInt32(id));

            if (verification != null)
            {
                var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                verification.Pickedup = true;
                verification.PickedUpById = CurrentRequestData.CurrentUser.Id;

                _helperSvc.Updateverification(verification);

                if (verification.DateAttendedTo == null)
                {
                    verification.DateAttendedTo = CurrentRequestData.Now;
                }

                ViewBag.EnrolleeFullName = enrollee.Surname + " " + enrollee.Othernames;
                ViewBag.policynumber = enrollee.Policynumber.ToUpper();
                ViewBag.ProviderName = verification.ProviderId > 0
                    ? _providerSvc.GetProvider(verification.ProviderId).Name.ToUpper()
                    : "--";
                var users = _userchat.Getallusers();

                var userlist = from aereply in users
                               select new
                               {
                                   Id = aereply.Id,
                                   Name = aereply.Name,
                               };

                var userr = userlist.ToList();

                userr.Add(new { Id = -1, Name = "Select" });


                ViewBag.usersList = userr.OrderBy(x => x.Id).ToList();




                var services = _serviceSvc.GetallServices();
                //

                if (verification.ServiceAccessed != null)
                {
                    ViewBag.servicesbag = new MultiSelectList(services, "Id", "Name",
                        verification.ServiceAccessed.Split(',').ToArray());
                }
                else
                {
                    ViewBag.servicesbag = new MultiSelectList(services, "Id", "Name");
                }

                return PartialView("AttendToVerification", verification);
            }
            return PartialView("AttendToVerification", new EnrolleeVerificationCode());
        }

        [HttpPost]
        public ActionResult AttendToVerification(EnrolleeVerificationCode enrolleVerification, FormCollection form)
        {
            if (enrolleVerification != null)
            {
                var dbVerification = _helperSvc.Getverification(enrolleVerification.Id);
                if (dbVerification != null)
                {
                    var serviceaccessed = form["ServiceAccessed2"];
                    dbVerification.ServiceAccessed = serviceaccessed;
                    dbVerification.AuthorizationCode = enrolleVerification.AuthorizationCode;
                    if (!string.IsNullOrEmpty(enrolleVerification.AuthorizationCode))
                    {
                        dbVerification.AuthorizationCodeGiven = true;
                        dbVerification.AuthorizedByUserId = enrolleVerification.AuthorizedByUserId;
                        var date = form["datepicker"];

                        dbVerification.DateAuthorized = Convert.ToDateTime(date);

                    }


                    dbVerification.AgentNote = enrolleVerification.AgentNote;
                    dbVerification.AttendedTo = true;
                    //check if the verification was picked up by a different agent
                    if (dbVerification.PickedUpById == CurrentRequestData.CurrentUser.Id)
                    {
                        if (_helperSvc.Updateverification(dbVerification))
                        {
                            _pageMessageSvc.SetSuccessMessage("Verification Code Updated Successfully.");

                        }
                        else
                        {
                            _pageMessageSvc.SetErrormessage("There was a problem updating the verification code.");

                        }


                    }
                    else
                    {
                        _pageMessageSvc.SetErrormessage(
                            string.Format("The Verification is being attended to by another agent. "));

                    }




                }
            }

            return _uniquePageService.RedirectTo<VerificationCodePage>();
        }


        [HttpGet]
        public ActionResult VerificationDetails(int? id)
        {

            var verification = _helperSvc.Getverification(Convert.ToInt32(id));

            if (verification != null)
            {
                var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);

                ViewBag.EnrolleeFullName = enrollee.Surname + " " + enrollee.Othernames;
                ViewBag.policynumber = enrollee.Policynumber.ToUpper();
                ViewBag.ProviderName = verification.ProviderId > 0
                    ? _providerSvc.GetProvider(verification.ProviderId).Name.ToUpper()
                    : "--";


                var servicelist = !string.IsNullOrEmpty(verification.ServiceAccessed)
                    ? verification.ServiceAccessed.Split(',').Select(
                            serv => _serviceSvc.GetService(Convert.ToInt32(serv)).Name + ",")
                        .ToList()
                        .Aggregate("", (current, dd) => current + "" + dd)
                    : "";

                ViewBag.ServiceAccessed = servicelist;


                return PartialView("VerificationDetails", verification);
            }
            return PartialView("AttendToVerification", new EnrolleeVerificationCode());
        }

        [HttpGet]
        public ActionResult QuickSms()
        {
            var contactlist = Enum.GetValues(typeof(SMSLeads));
            var listt = (from object item in contactlist
                         select new DdListItem()
                         {
                             Id = Convert.ToString((int)item),
                             Name = Enum.GetName(typeof(SMSLeads), item)
                         }).ToList();
            listt.Insert(0, new DdListItem() { Id = "-1", Name = "None" });
            ViewBag.CotactList = listt;



            return PartialView("QuickSms", new Sms());
        }

        [HttpPost]
        public ActionResult QuickSms(Sms sms, FormCollection form)
        {

            var contactlist = form["ContactList"];

            if (Convert.ToInt32(contactlist) > 0)
            {

                var listt =
                    _enrolleeService.GetConcatenatedPhoneNumberString(
                        (SMSLeads)Enum.Parse(typeof(SMSLeads), contactlist));
                sms.Msisdn = sms.Msisdn + listt;
            }
            sms.DeliveryDate = CurrentRequestData.Now;
            sms.DateDelivered = CurrentRequestData.Now;
            sms.CreatedBy = CurrentRequestData.CurrentUser.Id;
            sms.Status = SmsStatus.Pending;
            sms.Type = SmsType.Quick;
            var resp = _smsservice.SendSms(sms);
            if (resp)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(
                    string.Format("SMS was sent to {0} successfully.",
                        sms.Msisdn));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(
                    string.Format("There was a problem  sending SMS to  {0} ",
                        sms.Msisdn));
            }
            return _uniquePageService.RedirectTo<SmsPage>();
        }

        [ValidateAntiForgeryToken]
        public ActionResult AddDependent(FormCollection form)
        {



            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];
            var relationship = form["relationship"];
            var policynumber = form["policynumberD"];
            var surname = form["surnameD"];
            var othernames = form["othernamesD"];
            var dob = form["datepicker2"];
            var sex = form["sexD"];
            var mobilenumber = form["mobilenumberD"];
            var provider = form["providerD"];
            var premedicalcondition = form["premedicalconditionD"];
            var principalid = form["PrincipalID"];
            var page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();

            var principalenrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(principalid));
            var validation = true;
            var errormessage = new StringBuilder();
            if (string.IsNullOrEmpty(surname))
            {
                validation = false;
                errormessage.AppendLine("The surname field is blank.");
            }

            if (string.IsNullOrEmpty(othernames))
            {
                validation = false;
                errormessage.AppendLine("The othername field is blank.");
            }
            var outdate = new DateTime();
            if (string.IsNullOrEmpty(dob) || (!string.IsNullOrEmpty(dob) && DateTime.TryParse(dob, out outdate) == false))
            {
                validation = false;
                errormessage.AppendLine("The dob field is blank or not in the correct format.");
            }

            if (string.IsNullOrEmpty(sex))
            {
                validation = false;
                errormessage.AppendLine("The sex field is blank.");
            }

            if (string.IsNullOrEmpty(provider))
            {
                validation = false;
                errormessage.AppendLine("The provider address  field is empty.");
            }


            if (!validation)
            {
                //the form is not valid return
                _pageMessageSvc.SetErrormessage(string.Format("The form was not properly filled, {0}", errormessage.ToString()));
                return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", principalenrollee.Staffprofileid));

            }



            //Do Validation

            //check the age of the dependant
            var dobtin = Convert.ToDateTime(dob);

            //dependant its 21
            DateTime today = CurrentRequestData.Now;

            if (today.Year - dobtin.Year > 21 &&
                (Relationship)Enum.Parse(typeof(Relationship), relationship) == Relationship.Child)
            {
                //above limit

                _pageMessageSvc.SetErrormessage("The Dependant Age is above 21 , Dependant age must be below 21 years.");

                return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", principalenrollee.Staffprofileid));

            }

            //do image work
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }
            else
            {
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData = System.IO.File.ReadAllBytes(path);


            }

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                var dependents =
                    _enrolleeService.GetDependentsEnrollee(Convert.ToInt32(principalid))
                        .Where(x => x.IsDeleted == false)
                        .ToList();

                enrollee.Policynumber = string.Format(principalenrollee.Policynumber + "-{0}",
                    Convert.ToInt32(dependents.Count + 1));

            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            enrollee.Age = Convert.ToInt32(0);
            enrollee.Maritalstatus = Convert.ToInt32(MaritalStatus.Single);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = principalenrollee.Emailaddress;
            enrollee.Stateid = principalenrollee.Stateid;
            enrollee.Lgaid = principalenrollee.Lgaid;
            enrollee.Residentialaddress = principalenrollee.Residentialaddress;
            enrollee.Occupation = "None";
            enrollee.Sponsorshiptype = principalenrollee.Sponsorshiptype;
            enrollee.Sponsorshiptypeothername = principalenrollee.Sponsorshiptypeothername;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = principalenrollee.Staffprofileid;
            //set enrolle parentd
            enrollee.Parentid = principalenrollee.Id;
            enrollee.Parentrelationship = Convert.ToInt32(relationship);
            enrollee.Companyid = principalenrollee.Companyid;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Status = (int)EnrolleesStatus.Active;
            enrollee.Createdby = CurrentRequestData.CurrentUser.Id;
            if (_enrolleeService.GetEnrolleeByPolicyNumber(enrollee.Policynumber) != null)
            {
                _pageMessageSvc.SetErrormessage(
                    string.Format(
                        "There was a problem  adding Dependent [{0}] A Dependant with the same policy Number exist on the system.",
                        enrollee.Policynumber.ToUpper()));
                return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", principalenrollee.Staffprofileid));

            }
            var response = _enrolleeService.AddEnrollee(enrollee, imgData);

            principalenrollee.Hasdependents = true;
            var updateresp = _enrolleeService.UpdateEnrollee(principalenrollee);

            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Dependent [{0}] was added successfully.",
                    enrollee.Policynumber.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  adding Dependent [{0}] ",
                    enrollee.Policynumber.ToUpper()));
            }





            return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", principalenrollee.Staffprofileid));

        }

        public bool AddDependentAutomatic(FormCollection form, Enrollee principalenrollee)
        {



            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];
            var relationship = form["relationship"];
            var policynumber = form["policynumberD"];
            var surname = form["surnameD"];
            var othernames = form["othernamesD"];
            var dob = form["datepicker2"];
            var sex = form["sexD"];
            var mobilenumber = form["mobilenumberD"];
            var provider = form["providerD"];
            var premedicalcondition = form["premedicalconditionD"];
            var principalid = form["PrincipalID"];
            var reference = form["reference"];
            //var page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();

            //var principalenrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(principalid));
            //do image work
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }
            else
            {
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData = System.IO.File.ReadAllBytes(path);


            }

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                var dependents =
                    _enrolleeService.GetDependentsEnrollee(Convert.ToInt32(principalid))
                        .Where(x => x.IsDeleted == false)
                        .ToList();

                enrollee.Policynumber = string.Format(principalenrollee.Policynumber + "-{0}",
                    Convert.ToInt32(dependents.Count + 1));

            }

            if (_enrolleeService.GetEnrolleeByPolicyNumber(enrollee.Policynumber) != null)
            {
                //return and do not add
                return false;
            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            enrollee.Age = Convert.ToInt32(0);
            enrollee.Maritalstatus = Convert.ToInt32(MaritalStatus.Single);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = principalenrollee.Emailaddress;
            enrollee.Stateid = principalenrollee.Stateid;
            enrollee.Lgaid = principalenrollee.Lgaid;
            enrollee.Residentialaddress = principalenrollee.Residentialaddress;
            enrollee.Occupation = "None";
            enrollee.Sponsorshiptype = principalenrollee.Sponsorshiptype;
            enrollee.Sponsorshiptypeothername = principalenrollee.Sponsorshiptypeothername;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = principalenrollee.Staffprofileid;
            //set enrolle parentd
            enrollee.Parentid = principalenrollee.Id;
            enrollee.Parentrelationship = Convert.ToInt32(relationship);
            enrollee.Companyid = principalenrollee.Companyid;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Status = (int)EnrolleesStatus.Active;
            if (!string.IsNullOrEmpty(reference))
            {
                enrollee.RefPolicynumber = reference;
            }
            var response = _enrolleeService.AddEnrollee(enrollee, imgData);

            principalenrollee.Hasdependents = true;
            var updateresp = _enrolleeService.UpdateEnrollee(principalenrollee);

            return response;


        }

        public bool AddDependentAutomaticStaff(byte[] image, int relationship, string policynumber, string surname,
            string othernames, DateTime dob, int sex, string mobilenumber, int provider, string premedicalcondition,
            int principalid, Enrollee principalenrollee)
        {



            // var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];
            //var relationship = form["relationship"];
            //var policynumber = form["policynumberD"];
            //var surname = form["surnameD"];
            //var othernames = form["othernamesD"];
            //var dob = form["datepicker2"];
            //var sex = form["sexD"];
            //var mobilenumber = form["mobilenumberD"];
            //var provider = form["providerD"];
            //var premedicalcondition = form["premedicalconditionD"];
            //var principalid = form["PrincipalID"];
            //var reference = "";
            //var page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();

            //var principalenrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(principalid));
            //do image work
            byte[] imgData = image;

            //if (image != null && image.ContentLength > 0)
            //{
            //    Image image2 = Image.FromStream(image.InputStream);

            //    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
            //    var memoryStream = new MemoryStream();
            //    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    imgData = memoryStream.ToArray();

            //}
            //else
            //{
            //    var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

            //    imgData = System.IO.File.ReadAllBytes(path);


            //}

            var enrollee = new Enrollee();

            if (!string.IsNullOrEmpty(policynumber))
            {
                enrollee.Policynumber = policynumber;
            }
            else
            {
                var dependents =
                    _enrolleeService.GetDependentsEnrollee(Convert.ToInt32(principalid))
                        .Where(x => x.IsDeleted == false)
                        .ToList();

                enrollee.Policynumber = string.Format(principalenrollee.Policynumber + "-{0}",
                    Convert.ToInt32(dependents.Count + 1));

            }

            if (_enrolleeService.GetEnrolleeByPolicyNumber(enrollee.Policynumber) != null)
            {
                //return and do not add
                return false;
            }

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            enrollee.Age = Convert.ToInt32(0);
            enrollee.Maritalstatus = Convert.ToInt32(MaritalStatus.Single);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = principalenrollee.Emailaddress;
            enrollee.Stateid = principalenrollee.Stateid;
            enrollee.Lgaid = principalenrollee.Lgaid;
            enrollee.Residentialaddress = principalenrollee.Residentialaddress;
            enrollee.Occupation = "None";
            enrollee.Sponsorshiptype = principalenrollee.Sponsorshiptype;
            enrollee.Sponsorshiptypeothername = principalenrollee.Sponsorshiptypeothername;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = principalenrollee.Staffprofileid;
            //set enrolle parentd
            enrollee.Parentid = principalenrollee.Id;
            enrollee.Parentrelationship = Convert.ToInt32(relationship);
            enrollee.Companyid = principalenrollee.Companyid;
            enrollee.Subscriptionplanid = principalenrollee.Subscriptionplanid;
            enrollee.Status = (int)EnrolleesStatus.Active;
            enrollee.Createdby = 1;
            //if (!string.IsNullOrEmpty(reference))
            //{
            //    enrollee.RefPolicynumber = reference;
            //}
            var response = _enrolleeService.AddEnrollee(enrollee, imgData);

            principalenrollee.Hasdependents = true;
            var updateresp = _enrolleeService.UpdateEnrollee(principalenrollee);

            return response;


        }

        [ValidateAntiForgeryToken]
        public ActionResult UpdateEnrollee(FormCollection form)
        {

            //var images = form["photoInputFile"];
            //Get the form items.
            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFile"];
            var stafffullname = form["Stafffullname"];
            var policynumber = form["policynumber"];
            var surname = form["surname"];
            var othernames = form["othernames"];
            var dob = form["datepicker"];
            var age = form["age"];
            var maritalstatus = form["maritalstatusvalue"];
            var sex = form["sex"];
            var state = form["state"];
            var lga = form["lga"];
            var address = form["address"];
            var occupation = form["occupation"];
            var mobilenumber = form["mobilenumber"];
            var email = form["email"];
            var sponsorshiptype = form["sponsorshiptypevalue"];
            var sponsorshipothername = form["sponsorshipothername"];
            var subscriptiontype = form["subscriptiontypevalue"];
            var company = form["company"];
            var provider = form["provider"];
            var premedicalcondition = form["premedicalcondition"];
            var staffid = form["staffidd"];
            var enrolleeid = form["enrolleeid"];
            var mode = form["Mode"];
            var printid = form["idCardPrinted"];

            var enrollee = new Enrollee();
            var validation = true;
            var errormessage = new StringBuilder();


            if (string.IsNullOrEmpty(surname))
            {
                validation = false;
                errormessage.AppendLine("The surname field is blank.");
            }

            if (string.IsNullOrEmpty(othernames))
            {
                validation = false;
                errormessage.AppendLine("The othername field is blank.");
            }
            var outdate = new DateTime();
            if (string.IsNullOrEmpty(dob) || (!string.IsNullOrEmpty(dob) && DateTime.TryParse(dob, out outdate) == false))
            {
                validation = false;
                errormessage.AppendLine("The dob field is blank or not in the correct format.");
            }


            if (string.IsNullOrEmpty(maritalstatus))
            {
                validation = false;
                errormessage.AppendLine("The marital status field is blank.");
            }

            if (string.IsNullOrEmpty(sex))
            {
                validation = false;
                errormessage.AppendLine("The sex field is blank.");
            }
            if (string.IsNullOrEmpty(state) || (!string.IsNullOrEmpty(state) && Convert.ToInt32(state) < 1))
            {
                validation = false;
                errormessage.AppendLine("The state field is empty.");
            }
            if (string.IsNullOrEmpty(lga) || (!string.IsNullOrEmpty(state) && Convert.ToInt32(lga) < 1))
            {
                validation = false;
                errormessage.AppendLine("The LGA field is empty.");
            }

            if (string.IsNullOrEmpty(address))
            {
                validation = false;
                errormessage.AppendLine("The residential address  field is empty.");
            }
            if (string.IsNullOrEmpty(provider))
            {
                validation = false;
                errormessage.AppendLine("The provider address  field is empty.");
            }


            if (!validation)
            {
                //the form is not valid return
                _pageMessageSvc.SetErrormessage(string.Format("The form was not properly filled, {0}", errormessage.ToString()));

                return Redirect(
                          string.Format(
                              _uniquePageService.GetUniquePage<EnrolleeDetailsPage>().AbsoluteUrl +
                              "?id={0}&Mode={1}&enrolleeID={2}", staffid, mode, enrollee.Id));
            }
            var idint = 0;
            if (int.TryParse(enrolleeid.ToString(), out idint))
            {
                enrollee = _enrolleeService.GetEnrollee(idint);


            }

            var today = CurrentRequestData.Now;
            var dobtin = Convert.ToDateTime(dob);
            if (today.Year - dobtin.Year >= 70)
            {
                //above limit

                _pageMessageSvc.SetErrormessage("The Enrollee Age is above 70 , Dependant age must be below 70 years.");

                return
                    Redirect(
                        string.Format(
                            _uniquePageService.GetUniquePage<EnrolleeDetailsPage>().AbsoluteUrl +
                            "?id={0}&Mode={1}&enrolleeID={2}", staffid, mode, enrollee.Id));
            }
            //do image work
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }




            //if (!string.IsNullOrEmpty(policynumber))
            //{
            //    enrollee.Policynumber = policynumber;
            //}
            //else
            //{
            //    enrollee.Policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();

            //}

            enrollee.Surname = surname.ToUpper();
            enrollee.Othernames = othernames.ToUpper();
            enrollee.Dob = Convert.ToDateTime(dob);
            enrollee.Age = Convert.ToInt32(age);
            enrollee.Maritalstatus = Convert.ToInt32(maritalstatus);
            enrollee.Sex = Convert.ToInt32(sex);
            enrollee.Mobilenumber = mobilenumber;
            enrollee.Emailaddress = email;
            enrollee.Stateid = Convert.ToInt32(state);
            enrollee.Lgaid = Convert.ToInt32(lga);
            enrollee.Residentialaddress = address;
            enrollee.Occupation = occupation;
            enrollee.Sponsorshiptype = Convert.ToInt32(sponsorshiptype);
            enrollee.Sponsorshiptypeothername = sponsorshipothername;
            //enrollee.Subscriptionplanid = staff.StaffPlanid;
            enrollee.Preexistingmedicalhistory = premedicalcondition;
            enrollee.Primaryprovider = Convert.ToInt32(provider);
            enrollee.Staffprofileid = Convert.ToInt32(staffid);
            //enrollee.Companyid = Convert.ToInt32(staff.CompanyId);
            //enrollee.Status = (int)EnrolleesStatus.Active;

            //update only is theres a content.
            if (imgData != null && imgData.Length > 0)
            {
                enrollee.EnrolleePassport.Imgraw = imgData;
            }

            var value = printid.Split(',')[0];
            enrollee.IdCardPrinted = Convert.ToBoolean(value);

            var response = _enrolleeService.UpdateEnrollee(enrollee);








            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Enrollee [{0}] was updated successfully.",
                    enrollee.Policynumber.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  updated enrollee [{0}] ",
                    enrollee.Policynumber.ToUpper()));
            }


            //redirect to details page
            return
                Redirect(
                    string.Format(
                        _uniquePageService.GetUniquePage<EnrolleeDetailsPage>().AbsoluteUrl +
                        "?id={0}&Mode={1}&enrolleeID={2}", staffid, mode, enrollee.Id));

        }

        [HttpGet]
        public ActionResult ExpungeDependent(int id)
        {
            var idint = 0;
            var enrollee = new Enrollee();
            if (int.TryParse(Convert.ToString(id), out idint) && idint > 0)
            {
                enrollee = _enrolleeService.GetEnrollee(idint);


            }

            ViewBag.relationship = Enum.GetName(typeof(Relationship), enrollee.Parentrelationship);


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();
            ViewBag.providerlist = _providerSvc.GetallProvider().OrderBy(x => x.Name);
            ViewBag.enrolleeimg = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw);
            return PartialView("ExpungeDependent", enrollee);
        }

        public static void DumpExcel(DataTable tbl)
        {
            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("EnrolleeList");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(tbl, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:C1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                //Write it back to the client
                CurrentRequestData.CurrentContext.Response.ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                CurrentRequestData.CurrentContext.Response.AddHeader("content-disposition",
                    "attachment;  filename=EnrolleeListDatabase.xlsx");
                CurrentRequestData.CurrentContext.Response.BinaryWrite(pck.GetAsByteArray());


            }
        }

        private byte[] DumpExcelGetByte(DataTable tbl)
        {
            using (var pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("EnrolleeList");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(tbl, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:C1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid; //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                }

                //Example how to Format Column 1 as numeric 
                using (ExcelRange col = ws.Cells[2, 1, 2 + tbl.Rows.Count, 1])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                //Write it back to the client
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                // Response.AddHeader("content-disposition", "attachment;  filename=EnrolleeListDatabase.xlsx");
                //Response.BinaryWrite(pck.GetAsByteArray());

                return pck.GetAsByteArray();


            }
        }

        [HttpPost]
        public ActionResult ExpungeDependent(FormCollection form)
        {
            var dependentid = form["dependantID"];
            var expungeNote = form["ExpungeNote"];

            var enrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(dependentid));
            var response = false;
            if (enrollee != null)
            {
                enrollee.ExpungeNote = expungeNote;
                enrollee.Status = (int)EnrolleesStatus.Inactive;
                enrollee.Isexpundged = true;
                enrollee.Expungedby = CurrentRequestData.CurrentUser.Id;
                enrollee.Dateexpunged = CurrentRequestData.Now;

                response = _enrolleeService.UpdateEnrollee(enrollee);

            }


            if (response)
            {


                //successfule
                //Set the success message for user to see 
                _pageMessageSvc.SetSuccessMessage(string.Format("Dependent [{0}] was expunged successfully.",
                    enrollee.Policynumber.ToUpper()));
            }
            else
            {
                _pageMessageSvc.SetErrormessage(string.Format("There was a problem  expunging Dependent [{0}] ",
                    enrollee.Policynumber.ToUpper()));
            }
            var page = _uniquePageService.GetUniquePage<EnrolleeDetailsPage>();

            return Redirect(string.Format(page.AbsoluteUrl + "?id={0}&tabid=2", enrollee.Staffprofileid));
        }





        [HttpPost]
        public ActionResult GenerateVerificationCode(FormCollection form)
        {
            var verificationcode = form["Verificationcode"];
            var enrolleeId = form["EnrolleeId"];
            var verification = new EnrolleeVerificationCode();

            var enrollee = _enrolleeService.GetEnrollee(Convert.ToInt32(enrolleeId));

            var hassub = false;
            if (enrollee != null)
            {

                var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                var sub_sub = _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                //var validsub = comp_sub || sub_sub;

                if (sub_sub || comp_sub)
                {
                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid, staff.CompanySubsidiary);
                    var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);
                    if (Ssubscription != null &&
                        (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }


                    if (csubscription != null &&
                        (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }
                    //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                    //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                }




                if (!hassub)
                {

                    _pageMessageSvc.SetErrormessage(
                        string.Format("Enrollee Subscription has expired.  [{0}] ",
                            enrollee.Policynumber.ToUpper()));

                    return _uniquePageService.RedirectTo<VerificationCodePage>();
                }
            }

            if (enrollee != null && hassub)
            {
                var sms = new Sms();
                sms.FromId = "NHA";
                sms.DeliveryDate = CurrentRequestData.Now;
                sms.Message =
                    string.Format("Your Verification Code is : {0} Kindly tender the code at the hospital.Thank you",
                        verificationcode);
                sms.DateDelivered = CurrentRequestData.Now;
                sms.CreatedBy = CurrentRequestData.CurrentUser.Id;
                sms.Msisdn = enrollee.Mobilenumber;
                sms.Status = SmsStatus.Pending;
                sms.Type = SmsType.Verification;

                var resp = _smsservice.SendSms(sms);

                if (resp)
                {


                    //successfule
                    //Set the success message for user to see 
                    _pageMessageSvc.SetSuccessMessage(
                        string.Format("Verification SMS was successfully  Sent to Enrollee [{0}]  Code {1}.",
                            enrollee.Policynumber.ToUpper(), sms.Message));
                }
                else
                {
                    _pageMessageSvc.SetErrormessage(
                        string.Format("There was a problem  sending verification code to enrollee  [{0}] ",
                            enrollee.Policynumber.ToUpper()));
                }


                verification.EnrolleeId = enrollee.Id;
                verification.VerificationCode = verificationcode;
                verification.EncounterDate = CurrentRequestData.Now;
                verification.CreatedBy = CurrentRequestData.CurrentUser.Id;
                verification.Note = "Verification code was sent to enrollee for hospital access.";
                verification.Status = EnrolleeVerificationCodeStatus.Pending;
                _helperSvc.Addverification(verification);


            }


            return _uniquePageService.RedirectTo<VerificationCodePage>();
        }



        #region MobileApp

        //All mobile app methods and commands
        public JsonResult SetupMobileApp(string policyNumber, string mobileNumber, string email)
        {
            var response = new MobilesignupResponse();

            if (!string.IsNullOrEmpty(policyNumber) && !string.IsNullOrEmpty(mobileNumber) &&
                _enrolleeService.CheckEnrolleePhoneNumber(mobileNumber, policyNumber))
            {
                //if all is well here

                var code = _helperSvc.GenerateFourDigitCode();


                //theres an error 
                var sms = new Sms();
                sms.FromId = "NHA";
                sms.DeliveryDate = CurrentRequestData.Now;
                sms.Message =
                    string.Format(
                        "You recently signed-Up for Novo Health Africa  Mobile App ,Kindly use this code {0} to verify your account.Thank you",
                        code
                    );
                sms.DateDelivered = CurrentRequestData.Now;
                sms.CreatedBy = 1;
                sms.Msisdn = mobileNumber;
                sms.Status = SmsStatus.Pending;
                sms.Type = SmsType.Signup;
                var resp = _smsservice.SendSms(sms);


                var msignup = new MobileSignup();
                msignup.PolicyNumber = policyNumber;
                msignup.Email = (string)email;
                msignup.CodeGenerated = code;
                msignup.Status = 1;
                msignup.PhoneNumber = mobileNumber;
                msignup.Smsid = sms.Id.ToString();


                if (_enrolleeService.AddMobileSignup(msignup))
                {

                    response.Code = "1";
                    response.Message = string.Format("Verification code was sent to {0}", mobileNumber);
                    response.VerificationCode = code;


                }
                else
                {

                    response.Code = "-1";
                    response.Message = string.Format("There was an error ");
                    response.VerificationCode = code;
                }




            }
            else
            {


                response.Code = "-1";
                response.Message = string.Format("There was an error,Kindly confirm the data you entered.Thank you");
                response.VerificationCode = string.Empty;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MobileGetProfile(string policyNumber)
        {
            var response = new MobileProfileResponse();
            var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(policyNumber);

            if (enrollee != null)
            {
                response.Code = "1";
                response.Message = "User Profile was retrieved Successfully";
                response.Passport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw);
                response.EnrolleeFullName = enrollee.Surname + " " + enrollee.Othernames;
                response.Dob = Convert.ToDateTime(enrollee.Dob).ToShortDateString();
                response.MaritalStatus = Enum.GetName(typeof(MaritalStatus), enrollee.Maritalstatus);
                response.Sex = Enum.GetName(typeof(Sex), enrollee.Sex);
                response.Email = enrollee.Emailaddress;
                response.Phone = enrollee.Mobilenumber;
                response.Address = enrollee.Residentialaddress;
                response.Company = _companyService.Getstaff(enrollee.Staffprofileid).CompanySubsidiary > 0
                    ? _companyService.Getsubsidiary(
                            _companyService.Getstaff(enrollee.Staffprofileid).CompanySubsidiary).
                        Subsidaryname
                    : string.Empty;
                response.Provider = _providerSvc.GetProvider(enrollee.Primaryprovider).Name.ToUpper();

                var plancover =
                    _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                        AllowChildEnrollee
                        ? "FAMILY"
                        : "INDIVIDUAL";
                response.Plan =
                    _planService.GetPlan(
                        _companyService.GetCompanyPlan(_companyService.Getstaff(enrollee.Staffprofileid).StaffPlanid).
                            Planid).Name.ToUpper() + " " + plancover;
                response.Subscriptionstatus = enrollee.Hasactivesubscription;
                response.IsPrincipal = enrollee.Parentid <= 0;
                response.IsExpunged = enrollee.Isexpundged;



            }
            else
            {
                response.Code = "-1";
                response.Message = "The Policy Number is invalid,kindly check the policy number.";
            }


            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOtherPolicyNumbersTiedToNumber(string mobileNumber)
        {

            var enrollees = _enrolleeService.GetEnrolleesTiedToPhone(mobileNumber);
            IList<MobileEnrolleeTied> response;
            response = enrollees;
            if (enrollees != null && enrollees.Count > 0)
            {
                var items = _enrolleeService.GetFamilyTreeByPolicyNumber(enrollees.First().PolicyNumber);
                response.Clear();
                foreach (var item in items)
                {
                    var refpolicy = _enrolleeService.GetEnrolleeByReferencePolicyNumber(item.Policynumber);


                    var itom = new MobileEnrolleeTied()
                    {

                        //check if the enrollee exist in the refpolicynumber thing.



                        PolicyNumber = refpolicy == null ? item.Policynumber : refpolicy.RefPolicynumber,
                        FullName =
                            refpolicy == null
                                ? item.Surname + " " + item.Othernames
                                : refpolicy.Surname + " " + refpolicy.Othernames
                    };


                    response.Add(itom);

                }
            }


            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public string GenerateTempVerificationCode()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < 200; i++)
            {
                var code = _helperSvc.GenerateVerificationCode();
                output.Append(Environment.NewLine + code);

            }

            return output.ToString();
        }

        public JsonResult GetPurposeOfVisit()
        {

            var response = Enum.GetNames(typeof(PurposeOfVisit));
            var response2 = from areply in response
                            select new MobileVisitPurpose()
                            {
                                PurposeName = areply.ToUpper()
                            };

            return Json(response2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GenerateVerificationCodeJson(string enrolleePoliceNum, string purpose)
        {
            string errormsg;
            var refpolicy = _enrolleeService.GetEnrolleeByReferencePolicyNumber(enrolleePoliceNum);

            var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(enrolleePoliceNum);
            var hassub = false;
            if (enrollee != null)
            {

                var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                var sub_sub = _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                //var validsub = comp_sub || sub_sub;

                if (sub_sub || comp_sub)
                {
                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid, staff.CompanySubsidiary);

                    var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                    if (Ssubscription != null &&
                        (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }


                    if (csubscription != null &&
                        (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }
                    //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                    //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                }




                if (!hassub)
                {

                    errormsg =
                        "Your subscription has expired kindly contact your HR.Thank you.";
                }
            }
            if (enrollee != null && enrollee.Isexpundged == false && hassub)
            {


                //generate the verification code
                var verificationcode = _helperSvc.GenerateVerificationCode();
                var verification = new EnrolleeVerificationCode();
                verification.EnrolleeId = enrollee.Id;
                verification.VerificationCode = verificationcode;
                verification.EncounterDate = CurrentRequestData.Now;
                verification.CreatedBy = 1;
                verification.Channel = (int)ChannelType.MobileApp;
                verification.RequestPhoneNumber = string.Empty;
                verification.Note = "Verification code was sent to enrollee for hospital access.";
                verification.Status = EnrolleeVerificationCodeStatus.Pending;
                verification.VisitPurpose = (int)Enum.Parse(typeof(PurposeOfVisit), purpose, true);
                _helperSvc.Addverification(verification);

                var resp = new
                {
                    enrolleepolicy = enrollee.Policynumber.ToUpper(),
                    enrolleefullname = enrollee.Surname + " " + enrollee.Othernames,
                    verificationcode = verificationcode,
                    code = 1
                };

                return Json(resp, JsonRequestBehavior.AllowGet);

            }
            if (enrollee != null && enrollee.Isexpundged)
            {
                errormsg =
                    "The Policy Number has been expunged from the system.Kindly contact Novo Health Africa for clarification.";
            }
            else
            {
                errormsg =
                    "The Policy Number  does not exist.Kindly check the policy number and try again.";
            }



            var eresp = new
            {

                code = 0,
                Error = errormsg
            };

            return Json(eresp, JsonRequestBehavior.AllowGet);





        }

        public JsonResult GenerateVerificationCodeFromProvider(string providerid, string enrolleePoliceNum,
            string mobileNumber)
        {
            var log = new Log();
            log.Message = string.Format("New Verification Generated from Provider End. {0},{1},{4} @ {3} {2}",
                providerid, enrolleePoliceNum, CurrentRequestData.Now.ToLongTimeString(),
                CurrentRequestData.Now.ToLongDateString(), mobileNumber);
            log.Type = LogEntryType.Audit;
            log.Detail = "PostSms";

            _logger.Insert(log);

            string errormsg;
            var provider = _providerSvc.GetProvider(Convert.ToInt32(providerid));

            if (provider == null)
            {
                errormsg = "Provider ID does not exist,  kindly check the provider ID";
            }

            var refpolicy = _enrolleeService.GetEnrolleeByReferencePolicyNumber(enrolleePoliceNum);

            var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(enrolleePoliceNum);
            var hassub = false;


            if (enrollee != null)
            {

                var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                var sub_sub = _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                //var validsub = comp_sub || sub_sub;

                if (sub_sub || comp_sub)
                {
                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid, staff.CompanySubsidiary);

                    var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                    if (Ssubscription != null &&
                        (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }


                    if (csubscription != null &&
                        (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        hassub = true;
                    }
                    //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                    //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                }




                if (!hassub)
                {

                    errormsg =
                        "Your subscription has expired kindly contact your HR.Thank you.";
                }
            }
            if (enrollee != null && enrollee.Isexpundged == false && hassub)
            {


                //generate the verification code
                var verificationcode = _helperSvc.GenerateVerificationCode();
                var verification = new EnrolleeVerificationCode();
                verification.EnrolleeId = enrollee.Id;
                verification.VerificationCode = verificationcode;
                verification.EncounterDate = CurrentRequestData.Now;
                verification.CreatedBy = 1;
                verification.Channel = (int)ChannelType.MobileApp;
                verification.RequestPhoneNumber = string.Empty;
                verification.Note = "Verification code was sent to enrollee for hospital access.";
                verification.Status = EnrolleeVerificationCodeStatus.Pending;
                verification.VisitPurpose = 0;
                _helperSvc.Addverification(verification);

                var resp = new
                {
                    enrolleepolicy = enrollee.Policynumber.ToUpper(),
                    enrolleefullname = enrollee.Surname + " " + enrollee.Othernames,
                    verificationcode = verificationcode,
                    rcode = 0,
                    rmsg = "Kindly tender the code to the provider for authentication."
                };

                return Json(resp, JsonRequestBehavior.AllowGet);

            }
            if (enrollee != null && enrollee.Isexpundged)
            {
                errormsg =
                    "The Policy Number has been expunged from the system.Kindly contact Novo Health Africa for clarification.";
            }
            else
            {
                errormsg =
                    "The Policy Number  does not exist.Kindly check the policy number and try again.";
            }



            var eresp = new
            {

                code = 99,
                rmsg = errormsg
            };

            return Json(eresp, JsonRequestBehavior.AllowGet);





        }

        public JsonResult GetProviderDetails(string providerID)
        {
            if (!string.IsNullOrEmpty(providerID))
            {
                var provider = _providerSvc.GetProvider(Convert.ToInt32(providerID));
                if (provider != null)
                {
                    var resp = new
                    {
                        Id = provider.Id,
                        fullname = provider.Name.ToUpper(),
                        providerinitials = provider.Name.ToUpper().Substring(0, 1),
                        rcode = 0,
                        rmess = "successful"
                    };
                    return Json(resp, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var resp = new
                    {
                        rcode = 99,
                        rmess = "Invalid Provider Id."
                    };
                    return Json(resp, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var resp = new
                {
                    rcode = 99,
                    rmess = "Invalid Provider Id."
                };
                return Json(resp, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public string PostSms(string number, string message)
        {
            //var num = CurrentRequestData.CurrentContext.Request["number"];
            //var mess = CurrentRequestData.CurrentContext.Request["message"];

            //return Redirect("http://197.255.55.54/Enrollee/PostSms2?number=" + number + "&message=" + message); 
            return PostSms2(number, message);
        }


        public string PostSms2(string number, string message)
        {
            var smsresponse = string.Empty;
            var log = new Log();
            log.Message = string.Format("New Sms Message received. {0},{1} @ {3} {2}", number, message,
                CurrentRequestData.Now.ToLongTimeString(), CurrentRequestData.Now.ToLongDateString());
            log.Type = LogEntryType.Audit;
            log.Detail = "PostSms";

            _logger.Insert(log);


            var regexMain = new Regex(@"NHA-\d{12}");
            var regexDependents = new Regex(@"NHA-\d{12}-\d{1,}");


            //using regex to check it


            //log the guy 
            if (string.IsNullOrEmpty(number) || string.IsNullOrEmpty(message))
            {
                return "Invalid command, send NHA PolicyNo to 30812 to get a verification code.";
            }
            var shortmsg = new ShortCodeMsg();
            shortmsg.Mobile = number;
            shortmsg.Msg = message;
            shortmsg.MsgTime = DateTime.Today;
            shortmsg.Status = false;

            _enrolleeService.AddShortMessage(shortmsg);

            //var result = _enrolleeService.GetUnProccessedShortMessages();

            var result = new List<ShortCodeMsg>();
            result.Add(shortmsg);
            //process shortmessag
            if (result.Any())
            {
                //theres result 

                foreach (var item in result)
                {
                    string errormsg = string.Empty;

                    //check the message type
                    var msg = item.Msg.Split(' ');
                    int count = 0;

                    var msgclean = msg.Where(token => !string.IsNullOrWhiteSpace(token)).ToList();

                    Match match1 = regexMain.Match(message.ToUpper());
                    Match match2 = regexDependents.Match(message.ToUpper());



                    var msgWithoutspace = Regex.Replace(item.Msg, @"\s+", "");
                    var matchPrincipal = regexMain.IsMatch(msgWithoutspace);
                    var matchDependant = regexDependents.IsMatch(msgWithoutspace);

                    var guesspolicynumber = string.Empty;
                    if (matchPrincipal)
                    {
                        var value1 = regexMain.Match(msgWithoutspace);
                        guesspolicynumber = value1.Value;
                    }


                    if (matchDependant)
                    {
                        var value2 = regexDependents.Match(msgWithoutspace);
                        guesspolicynumber = value2.Value;
                    }




                    if (msgclean.Count == 2 &&
                        msgclean[0].ToLower().Equals("nha") | !string.IsNullOrEmpty(guesspolicynumber))
                    {


                        string policynumber;

                        if (!string.IsNullOrEmpty(guesspolicynumber))
                        {
                            policynumber = guesspolicynumber;
                        }
                        else
                        {
                            policynumber = msgclean[1].ToString();
                        }

                        //do for verification
                        var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(policynumber);

                        if (enrollee == null)
                        {
                            errormsg =
                                "The Policy Number Does not exist kindly check the policy number and try again.Call 012906240,012900047 for help .Thank you";
                            //theres an error 
                            var sms = new Sms
                            {
                                FromId = "Novo Health",
                                DeliveryDate = CurrentRequestData.Now,
                                Message = string.Format("{0}",
                                    errormsg),
                                DateDelivered = CurrentRequestData.Now,
                                CreatedBy = 1,
                                Msisdn = item.Mobile,
                                Status = SmsStatus.Pending,
                                Type = SmsType.Verification
                            };
                            smsresponse = sms.Message;
                            var resp = _smsservice.SendSms(sms);
                            return errormsg;
                        }
                        var enrolleeFamilyTree = _enrolleeService.GetFamilyTreeByPolicyNumber(policynumber);


                        string numberShort;

                        if (item.Mobile.Length > 11)
                        {
                            numberShort = item.Mobile.Substring(3, 10);

                        }
                        else
                        {
                            numberShort = item.Mobile.Substring(1, 10);
                        }

                        var phonenumerExist = true;
                        //enrolleeFamilyTree.Any(x => x.Mobilenumber.Contains(numberShort));This was removed to allow every number generate code by sms15_7_2016

                        //_enrolleeService.CheckEnrolleePhoneNumber(item.Mobile, policynumber)
                        var hassub = false;
                        if (enrollee != null)
                        {

                            var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                            var comp_sub =
                                _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                            var sub_sub =
                                _companyService.checkifSubsidiaryhasSubscrirption(
                                    Convert.ToInt32(staff.CompanySubsidiary));
                            //var validsub = comp_sub || sub_sub;

                            if (sub_sub || comp_sub)
                            {
                                var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                    staff.CompanySubsidiary);

                                var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                                if (Ssubscription != null &&
                                    (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                                {
                                    hassub = true;
                                }


                                if (csubscription != null &&
                                    (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                                {
                                    hassub = true;
                                }
                                //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                                //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                            }




                            if (!hassub)
                            {

                                errormsg =
                                    "Your subscription has expired kindly contact your HR.Thank you.";
                            }
                        }
                        if (enrollee.Isexpundged == false && phonenumerExist && hassub)
                        {
                            //generate the verification code
                            var verificationcode = _helperSvc.GenerateVerificationCode();
                            var verification = new EnrolleeVerificationCode();
                            verification.EnrolleeId = enrollee.Id;
                            verification.VerificationCode = verificationcode;
                            verification.EncounterDate = CurrentRequestData.Now;
                            verification.CreatedBy = 1;
                            verification.Channel = (int)ChannelType.ShortCode;
                            verification.RequestPhoneNumber = item.Mobile;
                            verification.Note = "Verification code was sent to enrollee for hospital access.";
                            verification.Status = EnrolleeVerificationCodeStatus.Pending;
                            _helperSvc.Addverification(verification);

                            //compose response sms for the niggar
                            var sms = new Sms();
                            sms.FromId = "Novo Health";
                            sms.DeliveryDate = CurrentRequestData.Now;
                            sms.Message =
                                string.Format(
                                    "Your Verification Code is : {0} Kindly tender the code at the hospital,The code is valid till {1} .Thank you",
                                    verificationcode,
                                    verification.CreatedOn.AddHours(23).AddMinutes(59).ToLongDateString());
                            sms.DateDelivered = CurrentRequestData.Now;
                            sms.CreatedBy = 1;
                            sms.Msisdn = item.Mobile;
                            sms.Status = SmsStatus.Pending;
                            sms.Type = SmsType.Verification;
                            sms.EnrolleeId = enrollee.Id;
                            smsresponse = sms.Message;
                            var resp = _smsservice.SendSms(sms);
                        }
                        if (enrollee.Isexpundged)
                        {
                            errormsg =
                                "The policy number has been expunged from the system.Kindly contact Novo Health Africa for clarification.";
                        }
                        if (!phonenumerExist)
                        {
                            errormsg =
                                string.Format("Your phone number {0} is not associated with the policy number {1}",
                                    item.Mobile, policynumber);
                        }


                        if (!string.IsNullOrEmpty(errormsg))
                        {
                            //theres an error 
                            var sms = new Sms();
                            sms.FromId = "Novo Health";
                            sms.DeliveryDate = CurrentRequestData.Now;
                            sms.Message =
                                string.Format("{0}",
                                    errormsg);
                            sms.DateDelivered = CurrentRequestData.Now;
                            sms.CreatedBy = 1;
                            sms.Msisdn = item.Mobile;
                            sms.Status = SmsStatus.Pending;
                            sms.Type = SmsType.Verification;
                            smsresponse = sms.Message;
                            var resp = _smsservice.SendSms(sms);
                        }

                    }
                    else if (msgclean.Count == 4 && msgclean[0].ToLower().Equals("nha") &&
                             msgclean[1].ToLower().Equals("auth"))
                    {
                        //authentication
                        var tempverificationCode = msgclean[2];
                        var tempproviderId = msgclean[3];

                        var providerId = 0;
                        string verificationCode;
                        if (tempverificationCode.Length > tempproviderId.Length)
                        {
                            providerId = Convert.ToInt32(tempproviderId);
                            verificationCode = tempverificationCode;
                        }
                        else
                        {
                            providerId = Convert.ToInt32(tempverificationCode);
                            verificationCode = tempproviderId;

                        }

                        var reply = string.Empty;

                        var verification =
                            _helperSvc.GetverificationByVerificationCode(verificationCode);


                        var provider = _providerSvc.GetProvider(providerId);

                        if (verification != null && provider != null)
                        {
                            var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                            var hassub = false;
                            if (enrollee != null)
                            {

                                var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                                var comp_sub =
                                    _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                                var sub_sub =
                                    _companyService.checkifSubsidiaryhasSubscrirption(
                                        Convert.ToInt32(staff.CompanySubsidiary));
                                //var validsub = comp_sub || sub_sub;

                                if (sub_sub || comp_sub)
                                {
                                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                        staff.CompanySubsidiary);

                                    var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                                    if (Ssubscription != null &&
                                        (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                                    {
                                        hassub = true;
                                    }


                                    if (csubscription != null &&
                                        (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                                    {
                                        hassub = true;
                                    }
                                    //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                                    //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                                }




                                if (!hassub)
                                {

                                    reply =
                                        "Your subscription has expired kindly contact your HR.Thank you.";
                                }
                            }
                            //generate the verification code

                            if (verification.Status == EnrolleeVerificationCodeStatus.Expired)
                            {
                                //Expired 
                                reply =
                                    "The Verification Code has expired,code is valid for 24 hours only, Kindly generate a new code. Thank you";


                            }

                            if (enrollee.Isexpundged)
                            {
                                reply =
                                    "The Enrollee has been expunged from our list.Kindly contact our customer service for further clarification.Thank you";

                            }

                            if (verification.Status == EnrolleeVerificationCodeStatus.Authenticated)
                            {
                                reply =
                                    "The Verification Code have been used.";

                            }

                            if (string.IsNullOrEmpty(reply))
                            {

                                var enrolleepolicy = enrollee.Policynumber.ToUpper();
                                var enrolleefullname = enrollee.Surname + " " + enrollee.Othernames;


                                //enrolleepassport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw),
                                //code = 1
                                var respmsg =
                                    string.Format(
                                        "Verification code {0}  is valid,for [ {1} ] {2} ,kindly attend to the bearer.",
                                        verificationCode, enrolleepolicy, enrolleefullname);

                                //Update the verification
                                verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                                verification.DateAuthenticated = CurrentRequestData.Now;
                                verification.ProviderId = Convert.ToInt32(providerId);
                                verification.Note = "Verification Code was authenticated.";
                                verification.AuthChannel = (int)ChannelType.ShortCode;


                                _helperSvc.Updateverification(verification);

                                //send sms
                                var sms = new Sms();
                                sms.FromId = "Novo Health";
                                sms.DeliveryDate = CurrentRequestData.Now;
                                sms.Message = respmsg;

                                sms.DateDelivered = CurrentRequestData.Now;
                                sms.CreatedBy = 1;
                                sms.Msisdn = item.Mobile;
                                sms.Status = SmsStatus.Pending;
                                sms.Type = SmsType.Verification;
                                sms.EnrolleeId = enrollee.Id;
                                smsresponse = sms.Message;
                                var resp = _smsservice.SendSms(sms);
                                //response = Json(resp, JsonRequestBehavior.AllowGet);
                            }


                        }
                        else
                        {

                            if (verification == null)
                            {
                                reply = "Invalid Verification Code.";
                            }


                            if (provider == null)
                            {
                                reply = "Invalid UPN.";
                            }

                            //response = Json(resp, JsonRequestBehavior.AllowGet);
                        }


                        //return error
                        if (!string.IsNullOrEmpty(reply))
                        {
                            //theres an error 
                            var sms = new Sms();
                            sms.FromId = "Novo Health";
                            sms.DeliveryDate = CurrentRequestData.Now;
                            sms.Message =
                                string.Format("{0}",
                                    reply);
                            sms.DateDelivered = CurrentRequestData.Now;
                            sms.CreatedBy = 1;
                            sms.Msisdn = item.Mobile;
                            sms.Status = SmsStatus.Pending;
                            sms.Type = SmsType.Verification;
                            smsresponse = sms.Message;
                            var resp = _smsservice.SendSms(sms);
                        }


                    }
                    else if (msgclean.Count == 3 && msgclean[0].ToLower().Equals("nha") &&
                             msgclean[2].ToLower().Contains("nha"))
                    {
                        //policy number shit
                        var upn = msgclean[1];
                        var policynumber = msgclean[2];

                        //Generate for the enrollee then authenticate

                        var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(policynumber.Trim());

                        var provider = _providerSvc.GetProvider(Convert.ToInt32(upn));
                        var reply = string.Empty;
                        var hassub = false;
                        if (enrollee != null)
                        {

                            var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                            var comp_sub =
                                _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                            var sub_sub =
                                _companyService.checkifSubsidiaryhasSubscrirption(
                                    Convert.ToInt32(staff.CompanySubsidiary));
                            //var validsub = comp_sub || sub_sub;

                            if (sub_sub || comp_sub)
                            {
                                var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                    staff.CompanySubsidiary);

                                var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                                if (Ssubscription != null &&
                                    (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                                {
                                    hassub = true;
                                }


                                if (csubscription != null &&
                                    (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                                {
                                    hassub = true;
                                }
                                //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                                //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                            }




                            if (!hassub)
                            {

                                reply =
                                    "Your subscription has expired kindly contact your HR.Thank you.";
                            }
                        }

                        if (enrollee != null && enrollee.Isexpundged == false && provider != null && hassub)
                        {
                            //generate the verification code
                            var verificationcode = _helperSvc.GenerateVerificationCode();
                            var verification = new EnrolleeVerificationCode();
                            verification.EnrolleeId = enrollee.Id;
                            verification.VerificationCode = verificationcode;
                            verification.EncounterDate = CurrentRequestData.Now;
                            verification.CreatedBy = 1;
                            verification.Channel = (int)ChannelType.ShortCode;
                            verification.RequestPhoneNumber = item.Mobile;
                            verification.Note = "Verification code was sent to enrollee for hospital access.";
                            verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                            verification.DateAuthenticated = CurrentRequestData.Now;
                            verification.AuthChannel = (int)ChannelType.ShortCode;
                            verification.ProviderId = Convert.ToInt32(upn);
                            verification.Note = "Verification Code was authenticated.";

                            _helperSvc.Addverification(verification);


                            //authentication
                            var tempverificationCode = verification.VerificationCode;
                            var tempproviderId = verification.ProviderId.ToString();

                            var providerId = 0;
                            string verificationCode;
                            if (tempverificationCode.Length > tempproviderId.Length)
                            {
                                providerId = Convert.ToInt32(tempproviderId);
                                verificationCode = tempverificationCode;
                            }
                            else
                            {
                                providerId = Convert.ToInt32(tempverificationCode);
                                verificationCode = tempproviderId;

                            }

                            //var reply = string.Empty;


                            if (verification != null)
                            {


                                //generate the verification code

                                if (verification.Status == EnrolleeVerificationCodeStatus.Expired)
                                {
                                    //Expired 
                                    reply =
                                        "The Verification Code has expired,code is valid for 24 hours only, Kindly generate a new code. Thank you";


                                }

                                if (enrollee.Isexpundged)
                                {
                                    reply =
                                        "The Enrollee has been expunged from our list.Kindly contact our customer service for further clarification.Thank you";

                                }

                                //if (verification.Status == EnrolleeVerificationCodeStatus.Authenticated)
                                //{
                                //    reply =
                                //       "The Verification Code have been used.";

                                //}

                                if (string.IsNullOrEmpty(reply))
                                {

                                    var enrolleepolicy = enrollee.Policynumber.ToUpper();
                                    var enrolleefullname = enrollee.Surname + " " + enrollee.Othernames;


                                    //enrolleepassport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw),
                                    //code = 1
                                    var respmsg =
                                        string.Format(
                                            "Verification code {0}  is valid,for [ {1} ] {2} ,kindly attend to the bearer.",
                                            verificationCode, enrolleepolicy, enrolleefullname);

                                    //Update the verification
                                    verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                                    verification.DateAuthenticated = CurrentRequestData.Now;
                                    verification.AuthChannel = (int)ChannelType.ShortCode;
                                    verification.ProviderId = Convert.ToInt32(providerId);
                                    verification.Note = "Verification Code was authenticated.";


                                    _helperSvc.Updateverification(verification);

                                    //send sms
                                    var sms = new Sms();
                                    sms.FromId = "Novo Health";
                                    sms.DeliveryDate = CurrentRequestData.Now;
                                    sms.Message = respmsg;

                                    sms.DateDelivered = CurrentRequestData.Now;
                                    sms.CreatedBy = 1;
                                    sms.Msisdn = item.Mobile;
                                    sms.Status = SmsStatus.Pending;
                                    sms.Type = SmsType.Verification;
                                    sms.EnrolleeId = enrollee.Id;
                                    smsresponse = sms.Message;
                                    var resp = _smsservice.SendSms(sms);
                                    //response = Json(resp, JsonRequestBehavior.AllowGet);
                                }


                            }
                            else
                            {
                                reply = "Invalid Verification Code.";

                                //response = Json(resp, JsonRequestBehavior.AllowGet);
                            }


                            //return error
                            if (!string.IsNullOrEmpty(reply))
                            {
                                //theres an error 
                                var sms = new Sms();
                                sms.FromId = "Novo Health";
                                sms.DeliveryDate = CurrentRequestData.Now;
                                sms.Message =
                                    string.Format("{0}",
                                        reply);
                                sms.DateDelivered = CurrentRequestData.Now;
                                sms.CreatedBy = 1;
                                sms.Msisdn = item.Mobile;
                                sms.Status = SmsStatus.Pending;
                                sms.Type = SmsType.Verification;
                                smsresponse = sms.Message;
                                var resp = _smsservice.SendSms(sms);
                            }
                            //_helperSvc.Updateverification(verification);
                        }
                        else
                        {


                            reply =
                                "Invalid command. Example Send NHA NHA-123456789012 to 30812. Call 012906240,012900047 for help .Thank you";

                            var sms = new Sms();
                            sms.FromId = "Novo Health";
                            sms.DeliveryDate = CurrentRequestData.Now;
                            sms.Message =
                                string.Format("{0}",
                                    reply);
                            sms.DateDelivered = CurrentRequestData.Now;
                            sms.CreatedBy = 1;
                            sms.Msisdn = item.Mobile;
                            sms.Status = SmsStatus.Pending;
                            sms.Type = SmsType.Verification;
                            smsresponse = sms.Message;
                            var resp = _smsservice.SendSms(sms);
                            return reply;
                        }

                    }
                    else
                    {
                        var reply =
                            "Invalid command. Example Send NHA NHA-123456789012 to 30812. Call 012906240,012900047 for help .Thank you";

                        var sms = new Sms();
                        sms.FromId = "Novo Health";
                        sms.DeliveryDate = CurrentRequestData.Now;
                        sms.Message =
                            string.Format("{0}",
                                reply);
                        sms.DateDelivered = CurrentRequestData.Now;
                        sms.CreatedBy = 1;
                        sms.Msisdn = item.Mobile;
                        sms.Status = SmsStatus.Pending;
                        sms.Type = SmsType.Verification;
                        smsresponse = sms.Message;
                        var resp = _smsservice.SendSms(sms);
                        return reply;
                    }

                    //var eresponse = Json(eresp, JsonRequestBehavior.AllowGet);
                    //return Json(new
                    //{
                    //    aaData = eresponse.Data
                    //});


                    //update item

                    item.Status = true;
                    _enrolleeService.UpdateShortMessage(item);

                }


            }


            //return "Thank You for using E-VS Powered by Novo Health Africa,you will receive a response shortly.";
            return smsresponse;
        }
        #endregion



        public JsonResult AuthenticateVerificationCode(string verificationCode, int? providerId)
        {
            var reply = string.Empty;
            JsonResult response;
            var verification =
                _helperSvc.Getallverifications().Where(x => x.VerificationCode == verificationCode).Take(1).
                    SingleOrDefault();

            var provider = _providerSvc.GetProvider(Convert.ToInt32(providerId));


            if (verification != null && provider != null)
            {

                var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                //generate the verification code

                if (verification.Status == EnrolleeVerificationCodeStatus.Expired)
                {
                    //Expired 
                    reply = "The Verification Code has expired, Kindly generate a new code. Thank you";


                }

                if (enrollee.Isexpundged)
                {
                    reply =
                        "The Enrollee has been expunged from our list.Kindly contact our customer service for futher clarification.Thank you";

                }

                if (verification.Status == EnrolleeVerificationCodeStatus.Authenticated)
                {
                    reply =
                        "The Verification Code have been used.";

                }

                if (string.IsNullOrEmpty(reply))
                {
                    var resp = new
                    {
                        enrolleepolicy = enrollee.Policynumber.ToUpper(),
                        enrolleefullname = enrollee.Surname + " " + enrollee.Othernames,
                        enrolleepassport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw),
                        code = 1
                    };

                    //Update the verification
                    verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                    verification.DateAuthenticated = CurrentRequestData.Now;
                    verification.AuthChannel = (int)ChannelType.MobileApp;
                    verification.ProviderId = Convert.ToInt32(providerId);
                    verification.Note = "Verification Code was authenticated.";


                    _helperSvc.Updateverification(verification);

                    response = Json(resp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var resp = new
                    {

                        code = 0,
                        errormessage = reply
                    };
                    response = Json(resp, JsonRequestBehavior.AllowGet);
                }



            }
            else
            {
                var resp = new
                {

                    code = 0,
                    errormessage = "Invalid Verification Code."
                };
                response = Json(resp, JsonRequestBehavior.AllowGet);

                if (provider == null)
                {
                    var resp2 = new
                    {

                        code = 0,
                        errormessage = "Invalid UPN"
                    };
                    response = Json(resp2, JsonRequestBehavior.AllowGet);
                }


            }



            return Json(new
            {
                aaData = response.Data
            });


        }

        public JsonResult AuthenticateVerificationCodeNew(string verificationCode, int? providerId)
        {
            var reply = string.Empty;
            JsonResult response;
            var verification =
                _helperSvc.Getallverifications().Where(x => x.VerificationCode == verificationCode).Take(1).
                    SingleOrDefault();


            var providerMain = _providerSvc.GetProvider(Convert.ToInt32(providerId));

            if (verification != null && providerMain != null)
            {

                var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                //generate the verification code

                if (verification.Status == EnrolleeVerificationCodeStatus.Expired)
                {
                    //Expired 
                    reply = "The Verification Code has expired, Kindly generate a new code. Thank you";


                }

                if (enrollee.Isexpundged)
                {
                    reply =
                        "The Enrollee has been expunged from our list.Kindly contact our customer service for futher clarification.Thank you";

                }

                if (verification.Status == EnrolleeVerificationCodeStatus.Authenticated &&
                    verification.ProviderId != providerId)
                {
                    reply =
                        "The Verification Code have been used.";

                }

                if (string.IsNullOrEmpty(reply))
                {
                    var providername = "";
                    var provider = _providerSvc.GetProvider(Convert.ToInt32(verification.ProviderId));
                    if (provider != null)
                    {
                        providername = provider.Name;
                    }
                    else
                    {
                        providername = "NIL";
                    }

                    //Update the verification

                    if (verification.Status != EnrolleeVerificationCodeStatus.Authenticated)
                    {
                        verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                        verification.DateAuthenticated = CurrentRequestData.Now;
                        verification.AuthChannel = (int)ChannelType.MobileApp;
                        verification.ProviderId = Convert.ToInt32(providerId);
                        verification.Note = "Verification Code was authenticated.";
                        _helperSvc.Updateverification(verification);
                    }



                    var resp = new VerificationResult()
                    {
                        Fullname = enrollee.Surname + " " + enrollee.Othernames,

                        Gender = Enum.GetName(typeof(Sex), enrollee.Sex),
                        PolicyNumber = enrollee.Policynumber.ToUpper(),
                        Passport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw),
                        Company =
                            _companyService.Getsubsidiary(
                                    _companyService.Getstaff(enrollee.Staffprofileid).CompanySubsidiary)
                                .Subsidaryname.ToUpper(),
                        Verificationcode = verificationCode,
                        Hospital = providername,
                        Dategenerated =
                            Convert.ToDateTime(verification.EncounterDate)
                                .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern),
                        Dateauthenticated =
                            Convert.ToDateTime(verification.DateAuthenticated)
                                .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern),

                        rcode = "0",
                        rmsg = "The Verification Code is Valid,Kindly attend to the bearer whose details appears above."
                    };




                    response = Json(resp, JsonRequestBehavior.AllowGet);

                    return response;
                }
                else
                {
                    var resp = new
                    {

                        rcode = 99,
                        rmsg = reply
                    };
                    response = Json(resp, JsonRequestBehavior.AllowGet);
                    return response;
                }



            }
            else
            {
                var resp = new
                {

                    rcode = "99",
                    rmsg = "Invalid verification code."
                };
                response = Json(resp, JsonRequestBehavior.AllowGet);

                return response;
            }


        }


        public string AutomaticStaffProfile()
        {

            var id = CurrentRequestData.CurrentContext.Request["id"];
            var allcompany = _companyService.GetallCompany();
            var narrative = new StringBuilder();
            var companyTitle = string.Empty;
            narrative.AppendLine(string.Format("Automatic Upload from Portal Report  {0}", CurrentRequestData.Now.ToLongDateString()));

            var sendemail = false;
            foreach (var company in allcompany)
            {

                if (company.Id == 1183)
                {

                }
                companyTitle = string.Format("Narrative for {0}", company.Name.ToUpper());

                var idd = company.Id;

                if (!string.IsNullOrEmpty(id))
                {
                    idd = Convert.ToInt32(id);
                }

                var _stafflist = new List<Staff>();


                _stafflist = _companyService.QueryFidelityComplete(idd, -1).ToList();




                var count = 0;
                foreach (var staff in _stafflist)
                {

                    var tempenrollee = _enrolleeService.GetTempEnrolleebystaffid(staff.Id.ToString());

                    if (tempenrollee != null)
                    {
                        //not null 

                        if (validateTempEnrollee(ref tempenrollee) && (staff.StaffFullname.ToLower().Contains(tempenrollee.Surname.ToLower()) || staff.StaffFullname.ToLower().Contains(tempenrollee.Othernames.ToLower())))
                        {
                            //valid
                            try
                            {
                                if (AddAutomaticsWebEnrollee(tempenrollee, staff))
                                {
                                    count++;
                                }


                            }
                            catch (Exception Ex)
                            {
                                var log = new Log();
                                log.Message = string.Format("There was a problem adding automatic staff profile.{0} TempEnrollee ID {1}",
                                    Ex.Message, tempenrollee.Id);
                                log.Type = LogEntryType.Audit;
                                log.Detail = "Staff Profile Error";

                                _logger.Insert(log);
                            }




                        }
                    }

                }


                if (count > 0)
                {
                    //add to narrative
                    sendemail = true;

                    narrative.AppendLine(string.Format("A total of ({0}) Enrollees where added to {1}", count, companyTitle));
                }
            }



            if (sendemail)
            {//_helperSvc.PushUserNotification 
                var emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = "temidayok@novohealthafrica.org";
                emailmsg.Subject = "Automatic Upload Report";
                emailmsg.FromName = "NovoHub Report";
                emailmsg.Body = narrative.ToString();
                emailmsg.IsHtml = true;
                _emailSender.AddToQueue(emailmsg);


            }

            return "Done";
        }

        public bool validateTempEnrollee(ref TempEnrollee temp)
        {
            var valid = true;

            var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");
            byte[] imgData = null;
            imgData = System.IO.File.ReadAllBytes(path);


            if (temp != null)
            {
                //check the dob for the guys
                if (temp.Dob.Year == 1917)
                {
                    temp.Dob = CurrentRequestData.Now;

                }


                if (string.IsNullOrEmpty(temp.Othernames) || string.IsNullOrEmpty(temp.Surname))
                {
                    valid = false;
                }

                if (temp.Dob == null)
                {
                    valid = false;
                }

                if (string.IsNullOrEmpty(temp.Residentialaddress))
                {
                    valid = false;
                }

                if (string.IsNullOrEmpty(temp.Primaryprovider))
                {
                    valid = false;
                }

                if (temp.Imgraw == imgData)
                {
                    valid = false;
                }

                if (temp.addspouse)
                {
                    //validate for spouse

                    if (temp.S_Imgraw == imgData)
                    {
                        //default img

                        //valid = false;
                    }

                }

                //check birthday




                if (temp.addchild1)
                {
                    var old = (CurrentRequestData.Now.Year - Convert.ToDateTime(temp.child1_Dob).Year) > 21;
                    if (temp.child1_Imgraw == imgData || old)
                    {
                        //default img

                        //valid = false;
                    }



                }
                if (temp.addchild2 && temp.child2_Dob != null)
                {
                    var old = (CurrentRequestData.Now.Year - Convert.ToDateTime(temp.child2_Dob).Year) > 21;
                    if (temp.child2_Imgraw == imgData || old)
                    {
                        //default img

                        //valid = false;
                    }



                }

                if (temp.addchild3 && temp.child3_Dob != null)
                {
                    var old = (CurrentRequestData.Now.Year - Convert.ToDateTime(temp.child3_Dob).Year) > 21;
                    if (temp.child3_Imgraw == imgData || old)
                    {
                        //default img

                        //valid = false;
                    }



                }

                if (temp.addchild4 && temp.child4_Dob != null)
                {
                    var old = (CurrentRequestData.Now.Year - Convert.ToDateTime(temp.child4_Dob).Year) > 21;
                    if (temp.child4_Imgraw == imgData || old)
                    {
                        //default img

                        //valid = false;
                    }



                }
            }
            else
            {
                valid = false;
            }




            return valid;
        }

        [HttpGet]
        public ActionResult GenerateVerificationCode(int id)
        {

            var enrollee = _enrolleeService.GetEnrollee(id);
            var model = new EnrolleeVerificationCode();
            if (enrollee != null)
            {
                @ViewBag.EnrolleePolicy = enrollee.Policynumber;

                model.VerificationCode = _helperSvc.GenerateVerificationCode();
                model.EnrolleeId = enrollee.Id;

            }
            return PartialView("GenerateVerificationCode", model);
        }



        [HttpGet]
        public void ExportTable()
        {
            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
                "App_Data");
            var foldername = Guid.NewGuid().ToString();
            var fullpath = Path.Combine(appdatafolder, foldername);
            System.IO.Directory.CreateDirectory(fullpath);


            //export all enrollees to excel
            var test = new DataTable();
            test.Columns.Add("S/CODE", typeof(string));
            test.Columns.Add("GENDER", typeof(string));
            test.Columns.Add("DOB", typeof(string));
            test.Columns.Add("MARITAL STATUS", typeof(string));
            test.Columns.Add("ENROLLEE NAME", typeof(string));
            test.Columns.Add("PHONE NUMBER", typeof(string));
            test.Columns.Add("OCCUPATION", typeof(string));
            test.Columns.Add("HOME ADDRESS", typeof(string));
            test.Columns.Add("HEALTH PLAN", typeof(string));
            test.Columns.Add("PROVIDER ID", typeof(string));
            test.Columns.Add("PROVIDER", typeof(string));
            test.Columns.Add("POLICY NUMBER", typeof(string));
            test.Columns.Add("COMPANY", typeof(string));
            test.Columns.Add("EMAIL", typeof(string));
            test.Columns.Add("EXPIRATION", typeof(string));
            test.Columns.Add("REMARK", typeof(string));
            test.Columns.Add("REGISTRATION DATE", typeof(string));
            test.Columns.Add("SUBSIDIARY", typeof(string));


            var end = false;

            //
            var count = 0;

            var totalrecord = 100;

            var enrollees = _enrolleeService.GetallenrolleeRange(out totalrecord, count, 31000);
            count = test.Rows.Count + 1;

            foreach (var item in enrollees)
            {


                try
                {


                    var plancover =
                        _companyService.GetCompanyPlan(_companyService.Getstaff(item.Staffprofileid).StaffPlanid).
                            AllowChildEnrollee
                            ? "FAMILY"
                            : "INDIVIDUAL";
                    var healthplan =

                        _planService.GetPlan(
                            _companyService.GetCompanyPlan(_companyService.Getstaff(item.Staffprofileid).StaffPlanid).
                                Planid).Name.ToUpper() + " " + plancover;
                    var provider = _providerSvc.GetProvider(item.Primaryprovider);
                    var company = _companyService.GetCompany(item.Companyid).Name.ToUpper();
                    var companysubscription =
                        _companyService.GetSubscriptionByPlan(_companyService.Getstaff(item.Staffprofileid).StaffPlanid);
                    var startdate = CurrentRequestData.Now;
                    var expirationdate = CurrentRequestData.Now;
                    if (companysubscription != null)
                    {
                        startdate = Convert.ToDateTime(companysubscription.Startdate);
                        expirationdate = Convert.ToDateTime(companysubscription.Expirationdate);

                    }

                    test.Rows.Add(Convert.ToInt32(count), Enum.GetName(typeof(Sex), item.Sex),
                        Convert.ToDateTime(item.Dob).ToString("MM/dd/yyyy")
                        ,
                        Enum.GetName(typeof(MaritalStatus)
                            , item.Maritalstatus), (item.Surname + " " + item.Othernames).ToUpper(),
                        item.Mobilenumber,
                        item.Occupation, item.Residentialaddress, healthplan, provider.Code, provider.Name,
                        item.Policynumber, company,
                        item.Emailaddress, expirationdate.ToString("MM/dd/yyyy"), item.Preexistingmedicalhistory,
                        startdate.ToString("MM/dd/yyyy"),
                        _companyService.Getsubsidiary(
                            _companyService.Getstaff(item.Staffprofileid).CompanySubsidiary).Subsidaryname.ToUpper());


                }
                catch (Exception ex)
                {

                }

            }



            var excelarray = DumpExcelGetByte(test);

            //write excel to folder

            System.IO.File.WriteAllBytes(Path.Combine(fullpath, foldername + ".xlsx"), excelarray);

            //zip folder and send to client

            string zipPath = Path.Combine(appdatafolder, string.Format("{0}.zip", foldername));

            ZipFile.CreateFromDirectory(fullpath, zipPath);

            //send back to user
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment;  filename=IDCardExport.zip");
            Response.BinaryWrite(System.IO.File.ReadAllBytes(zipPath));

        }

        [HttpGet]

        public JsonResult GetEnrolleeDetailsClaim(string policyNumber)
        {
            var enrollee = _enrolleeService.GetEnrolleeByPolicyNumber(policyNumber);

            var response = new EnrolleeDetailsClaim();
            if (enrollee == null)
            {
                response.respCode = 99;
                response.errorMsg =
                    string.Format("The Policy Number {0} is invalid,Kindly check the policy number and try again ",
                        policyNumber);
            }
            else
            {
                var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                var plan = _companyService.GetCompanyPlan(staff.StaffPlanid);
                response.respCode = 0;
                response.EnrolleeName = (enrollee.Surname + " " + enrollee.Othernames).ToUpper();
                response.EnrolleeGender = Enum.GetName(typeof(Sex), enrollee.Sex);
                response.EnrolleeCompany = _companyService.GetCompany(enrollee.Companyid).Name ?? "--";
                response.CompanyId = enrollee.Companyid.ToString();
                response.EnrolleeSubsidiary = _companyService.GetCompany(enrollee.Companyid).Name ?? "--";
                response.EnrolleePlan = plan.Planfriendlyname.ToUpper();
                response.isexpunged = enrollee.Isexpundged;
                response.Id = enrollee.Id.ToString();
            }


            return Json(response, JsonRequestBehavior.AllowGet);

        }

        public ActionResult EnrolleeRegID(int id, EnrolleeRegIDPage page)
        {

            var tempenrollee = _enrolleeService.GetTempEnrollee(id);



            if (tempenrollee != null)
            {
                page.Enrollee = tempenrollee;
                var company = _companyService.GetCompany(tempenrollee.Companyid);
                ViewBag.companyname = company != null ? company.Name.ToUpper() : "Unknown";
                ViewBag.PassportP = Convert.ToBase64String(tempenrollee.Imgraw);
                ViewBag.PassportS = Convert.ToBase64String(tempenrollee.S_Imgraw);
                ViewBag.PassportC1 = Convert.ToBase64String(tempenrollee.child1_Imgraw);
                ViewBag.PassportC2 = Convert.ToBase64String(tempenrollee.child2_Imgraw);
                ViewBag.PassportC3 = Convert.ToBase64String(tempenrollee.child3_Imgraw);
                ViewBag.PassportC4 = Convert.ToBase64String(tempenrollee.child4_Imgraw);

                if (company.Id == 132)
                {
                    ViewBag.expirition =
                                       Convert.ToDateTime(tempenrollee.CreatedOn.AddMonths(3))
                                           .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                }
                else
                {
                    ViewBag.expirition =
                                       Convert.ToDateTime(tempenrollee.CreatedOn.AddDays(28))
                                           .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                }






            }

            return View(page);
        }

        public ActionResult SubmitPortalRegistration(FormCollection Form)
        {
            //get personal details
            var previousid = Form["previousid"];
            var stafflistidstr = Form["stafflistid"];
            var tempenrolleeid = 0;
            var stafflistid = 0;

            var policynumber = _helperSvc.GeneratePolicyNumber(1, true).FirstOrDefault();

            var firstname = Form["inputfirstname"];
            var lastname = Form["inputLastName"];
            var dob = Form["datepicker"];
            var maritalstatus = Form["maritalstatusvalue"];
            var sex = Form["sex"];
            var state = Form["state"];
            var LGA = Form["inputLGA"];
            var stateResident = Form["stateResident"];
            var address = Form["address"];
            var occupation = Form["occupation"];
            var mobilenumber = Form["mobilenumber"];
            var mobilenumber2 = Form["mobilenumber2"];
            var EmailForm = Form["EmailForm"];
            var company = Form["company_id"];
            var stateoffice = Form["stateoffice"];
            var staffid = Form["staffid"];
            var branchid = Form["branchid"];
            var provider = Form["provider"];
            var premedicalcondition = Form["premedicalcondition"];


            //spouse info
            var spousefirstname = Form["inputSpousefirstname"];
            var SpouseLastName = Form["inputSpouseLastName"];
            var spousedob = Form["Spousedatepicker"];
            var Spousesex = Form["Spousesex"];
            var Spousemobilenumber = Form["mobilenumberSpouse"];
            var spouseemail = Form["EmailFormspouse"];
            var spouseprovider = Form["ProviderSpouse"];
            var spousepremedical = Form["premedicalconditionSpouse"];

            //child1 


            var firstnamechild1 = Form["inputfirstnameChild1"];
            var lastnamechild1 = Form["inputSLastNameChild1"];
            var DOBChild1 = Form["Child1datepicker"];
            var genderchild1 = Form["Child1Gender"];
            var mobilenumchild1 = Form["mobilenumberChild1"];
            var EmailFormchild1 = Form["EmailFormchild1"];
            var providerchild1 = Form["child1provider"];
            var premedicalconditionchild1 = Form["premedicalconditionchild1"];

            //child2
            var firstnamechild2 = Form["inputfirstnameChild2"];
            var lastnamechild2 = Form["inputLastNameChild2"];
            var DOBChild2 = Form["Child2datepicker"];
            var genderchild2 = Form["Child2sex"];
            var mobilenumchild2 = Form["mobilenumberChild2"];
            var EmailFormchild2 = Form["EmailFormchild2"];
            var providerchild2 = Form["child2provider"];
            var premedicalconditionchild2 = Form["premedicalconditionchild2"];



            //child3
            var firstnamechild3 = Form["inputfirstnameChild3"];
            var lastnamechild3 = Form["inputLastNameChild3"];
            var DOBChild3 = Form["Child3datepicker"];
            var genderchild3 = Form["Child3sex"];
            var mobilenumchild3 = Form["mobilenumberChild3"];
            var EmailFormchild3 = Form["EmailFormchild3"];
            var providerchild3 = Form["child3provider"];
            var premedicalconditionchild3 = Form["premedicalconditionchild3"];

            //child4
            var firstnamechild4 = Form["inputfirstnameChild4"];
            var lastnamechild4 = Form["inputLastNameChild4"];
            var DOBChild4 = Form["Child4datepicker"];
            var genderchild4 = Form["Child4sex"];
            var mobilenumchild4 = Form["mobilenumberChild4"];
            var EmailFormchild4 = Form["EmailFormchild4"];
            var providerchild4 = Form["child4provider"];
            var premedicalconditionchild4 = Form["premedicalconditionchild4"];


            //checkboxes
            var spousecheckbox = string.IsNullOrEmpty(Form["spousecheckbox"])
                ? false
                : Form["spousecheckbox"].Contains("on") ? true : false;
            var child1checkbox = string.IsNullOrEmpty(Form["child1checkbox"])
                ? false
                : Form["child1checkbox"].Contains("on") ? true : false;
            var child2checkbox = string.IsNullOrEmpty(Form["child2checkbox"])
                ? false
                : Form["child2checkbox"].Contains("on") ? true : false;
            var child3checkbox = string.IsNullOrEmpty(Form["child3checkbox"])
                ? false
                : Form["child3checkbox"].Contains("on") ? true : false;
            var child4checkbox = string.IsNullOrEmpty(Form["child4checkbox"])
                ? false
                : Form["child4checkbox"].Contains("on") ? true : false;



            //do image work

            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFile"];
            var image_s = CurrentRequestData.CurrentContext.Request.Files["photoInputFileSpouse"];
            var image_c1 = CurrentRequestData.CurrentContext.Request.Files["photoInputFileChild1"];
            var image_c2 = CurrentRequestData.CurrentContext.Request.Files["photoInputFilechild2"];
            var image_c3 = CurrentRequestData.CurrentContext.Request.Files["photoInputFilechild3"];
            var image_c4 = CurrentRequestData.CurrentContext.Request.Files["photoInputFilechild4"];

            //do image work
            byte[] imgData = null;
            byte[] imgData_s = null;
            byte[] imgData_c1 = null;
            byte[] imgData_c2 = null;
            byte[] imgData_c3 = null;
            byte[] imgData_c4 = null;

            bool updateimg1 = true;
            bool updateimg2 = true;
            bool updateimg3 = true;
            bool updateimg4 = true;
            bool updateimg5 = true;
            bool updateimg6 = true;



            if (image != null && image.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }


            }
            else
            {

                updateimg1 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData = System.IO.File.ReadAllBytes(path);


            }

            //spouse
            if (image_s != null && image_s.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image_s.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData_s = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }


            }
            else
            {
                updateimg2 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData_s = System.IO.File.ReadAllBytes(path);


            }

            //child1
            if (image_c1 != null && image_c1.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image_c1.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData_c1 = memoryStream.ToArray();

                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }

            }
            else
            {
                updateimg3 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData_c1 = System.IO.File.ReadAllBytes(path);


            }

            //child2
            if (image_c2 != null && image_c2.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image_c2.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData_c2 = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }


            }
            else
            {
                updateimg4 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData_c2 = System.IO.File.ReadAllBytes(path);


            }

            //child3
            if (image_c3 != null && image_c3.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image_c3.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData_c3 = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }


            }
            else
            {
                updateimg5 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData_c3 = System.IO.File.ReadAllBytes(path);


            }


            //child4
            if (image_c4 != null && image_c4.ContentLength > 0)
            {

                try
                {
                    Image image2 = Image.FromStream(image_c4.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData_c4 = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    _pageMessageSvc.SetErrormessage("The uploaded passport is invalid,kindly check the format uploaded and reupload . (Jpeg and Png are the acceptable formats.)");
                    return Redirect(CurrentRequestData.CurrentContext.Request.UrlReferrer.AbsoluteUri.ToString());

                }


            }
            else
            {
                updateimg6 = false;
                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                imgData_c4 = System.IO.File.ReadAllBytes(path);


            }
            var enrolleetemp = new TempEnrollee();
            if (!string.IsNullOrEmpty(previousid) && int.TryParse(previousid, out tempenrolleeid) && tempenrolleeid > 0)
            {
                enrolleetemp = _enrolleeService.GetTempEnrollee(tempenrolleeid);

            }

            if (!string.IsNullOrEmpty(stafflistidstr) && int.TryParse(stafflistidstr, out stafflistid) &&
                stafflistid > 0)
            {
                enrolleetemp.Staffprofileid = stafflistid;
            }


            enrolleetemp.Surname = lastname;
            enrolleetemp.Othernames = firstname;
            enrolleetemp.Dob = !string.IsNullOrEmpty(dob)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(dob.Substring(6, 4)),
                    Convert.ToInt32(dob.Substring(3, 2)), Convert.ToInt32(dob.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.Maritalstatus = Convert.ToInt32(maritalstatus);
            enrolleetemp.Sex = Convert.ToInt32(sex);
            enrolleetemp.Stateoforiginid = Convert.ToInt32(state);
            enrolleetemp.Lga = LGA;
            enrolleetemp.Stateofresidence = Convert.ToInt32(stateResident);
            enrolleetemp.Residentialaddress = address;
            enrolleetemp.Occupation = occupation;
            enrolleetemp.Mobilenumber = mobilenumber;
            enrolleetemp.Mobilenumber2 = mobilenumber2;
            enrolleetemp.Emailaddress = EmailForm;
            if (tempenrolleeid < 1)
            {
                enrolleetemp.Companyid = Convert.ToInt32(company);
            }
            else
            {
                //do nothing
                //enrolleetemp.Companyid = Convert.ToInt32(company);
            }

            enrolleetemp.officestate = Convert.ToInt32(stateoffice);
            enrolleetemp.staffid = staffid;
            enrolleetemp.BranchID = branchid;
            enrolleetemp.Primaryprovider = provider;
            enrolleetemp.Preexistingmedicalhistory = premedicalcondition;

            //spouse 
            enrolleetemp.s_Surname = SpouseLastName;
            enrolleetemp.s_Othernames = spousefirstname;
            enrolleetemp.s_Dob = !string.IsNullOrEmpty(spousedob)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(spousedob.Substring(6, 4)),
                    Convert.ToInt32(spousedob.Substring(3, 2)), Convert.ToInt32(spousedob.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.S_Sex = Convert.ToInt32(Spousesex);
            enrolleetemp.S_email = spouseemail;
            enrolleetemp.S_mobile = Spousemobilenumber;
            enrolleetemp.S_hospital = spouseprovider;
            enrolleetemp.S_medicalhistory = spousepremedical;

            //child1
            enrolleetemp.child1_Surname = lastnamechild1;
            enrolleetemp.child1_Othernames = firstnamechild1;
            enrolleetemp.child1_Dob = !string.IsNullOrEmpty(DOBChild1)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(DOBChild1.Substring(6, 4)),
                    Convert.ToInt32(DOBChild1.Substring(3, 2)), Convert.ToInt32(DOBChild1.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.child1_Sex = Convert.ToInt32(genderchild1);
            enrolleetemp.child1_email = EmailFormchild1;
            enrolleetemp.child1_mobile = mobilenumchild1;
            enrolleetemp.child1_hospital = providerchild1;
            enrolleetemp.child1_medicalhistory = premedicalconditionchild1;


            //child2
            enrolleetemp.child2_Surname = lastnamechild2;
            enrolleetemp.child2_Othernames = firstnamechild2;
            enrolleetemp.child2_Dob = !string.IsNullOrEmpty(DOBChild2)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(DOBChild2.Substring(6, 4)),
                    Convert.ToInt32(DOBChild2.Substring(3, 2)), Convert.ToInt32(DOBChild2.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.child2_Sex = Convert.ToInt32(genderchild2);
            enrolleetemp.child2_email = EmailFormchild2;
            enrolleetemp.child2_mobile = mobilenumchild2;
            enrolleetemp.child2_hospital = providerchild2;
            enrolleetemp.child2_medicalhistory = premedicalconditionchild2;


            //child3
            enrolleetemp.child3_Surname = lastnamechild3;
            enrolleetemp.child3_Othernames = firstnamechild3;
            enrolleetemp.child3_Dob = !string.IsNullOrEmpty(DOBChild3)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(DOBChild3.Substring(6, 4)),
                    Convert.ToInt32(DOBChild3.Substring(3, 2)), Convert.ToInt32(DOBChild3.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.child3_Sex = Convert.ToInt32(genderchild3);
            enrolleetemp.child3_email = EmailFormchild3;
            enrolleetemp.child3_mobile = mobilenumchild3;
            enrolleetemp.child3_hospital = providerchild3;
            enrolleetemp.child3_medicalhistory = premedicalconditionchild3;

            //child4
            enrolleetemp.child4_Surname = lastnamechild4;
            enrolleetemp.child4_Othernames = firstnamechild4;
            enrolleetemp.child4_Dob = !string.IsNullOrEmpty(DOBChild4)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(DOBChild4.Substring(6, 4)),
                    Convert.ToInt32(DOBChild4.Substring(3, 2)), Convert.ToInt32(DOBChild4.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            enrolleetemp.child4_Sex = Convert.ToInt32(genderchild4);
            enrolleetemp.child4_email = EmailFormchild4;
            enrolleetemp.child4_mobile = mobilenumchild4;
            enrolleetemp.child4_hospital = providerchild4;
            enrolleetemp.child4_medicalhistory = premedicalconditionchild4;

            if (string.IsNullOrEmpty(enrolleetemp.Policynumber))
            {
                enrolleetemp.Policynumber = policynumber;
            }


            if (tempenrolleeid < 1)
            {
                enrolleetemp.Imgraw = imgData;
                enrolleetemp.S_Imgraw = imgData_s;
                enrolleetemp.child1_Imgraw = imgData_c1;
                enrolleetemp.child2_Imgraw = imgData_c2;
                enrolleetemp.child3_Imgraw = imgData_c3;
                enrolleetemp.child4_Imgraw = imgData_c4;
            }


            //imgs
            if (updateimg1 && tempenrolleeid > 0)
            {
                enrolleetemp.Imgraw = imgData;
            }


            if (updateimg2 && tempenrolleeid > 0)
            {
                enrolleetemp.S_Imgraw = imgData_s;
            }

            if (updateimg3 && tempenrolleeid > 0)
            {
                enrolleetemp.child1_Imgraw = imgData_c1;
            }

            if (updateimg4 && tempenrolleeid > 0)
            {
                enrolleetemp.child2_Imgraw = imgData_c2;
            }


            if (updateimg5 && tempenrolleeid > 0)
            {
                enrolleetemp.child3_Imgraw = imgData_c3;
            }


            if (updateimg6 && tempenrolleeid > 0)
            {
                enrolleetemp.child4_Imgraw = imgData_c4;
            }





            enrolleetemp.addspouse = !(string.IsNullOrEmpty(spousefirstname) & string.IsNullOrEmpty(SpouseLastName));
            enrolleetemp.addchild1 = !(string.IsNullOrEmpty(firstnamechild1) & string.IsNullOrEmpty(lastnamechild1));
            enrolleetemp.addchild2 = !(string.IsNullOrEmpty(firstnamechild2) & string.IsNullOrEmpty(lastnamechild2));
            enrolleetemp.addchild3 = !(string.IsNullOrEmpty(firstnamechild3) & string.IsNullOrEmpty(lastnamechild3));
            enrolleetemp.addchild4 = !(string.IsNullOrEmpty(firstnamechild4) & string.IsNullOrEmpty(lastnamechild4));

            var status = _enrolleeService.AddTempEnrollee(enrolleetemp);



            _pageMessageSvc.SetSuccessMessage(
                "Form was submitted successfully, You can make corrections at anytime by logging in with your phone number or email.Thank you");


            //do email to the recepient.

            if (!string.IsNullOrEmpty(enrolleetemp.Emailaddress))
            {
                var fullname = enrolleetemp.Surname + " " + enrolleetemp.Othernames;
                var emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = enrolleetemp.Emailaddress;
                emailmsg.Subject = " Successful Registration on Novo Health Africa Portal ";
                emailmsg.FromName = "NOVO Health Africa";
                emailmsg.Body = string.Format("Dear {0},{1} Your registration on our portal  was successful.{1} Thank you.", fullname, Environment.NewLine);

                _emailSender.AddToQueue(emailmsg);
            }

            //return _uniquePageService.RedirectTo<EnrolleeRegPage>();


            return Redirect(string.Format(_uniquePageService.GetUniquePage<EnrolleeRegIDPage>().AbsoluteUrl + "?id={0}",
                enrolleetemp.Id));


        }

        public RedirectResult Portal(int? Companyid)
        {
            return
                Redirect(
                    string.Format(_uniquePageService.GetUniquePage<EnrolleeRegHomePage>().AbsoluteUrl + "?companyid={0}",
                        Companyid));


            //return Redirect("http://197.255.55.54/portal/" + "?companyid=" + Convert.ToString(Companyid));
            //if (Convert.ToInt32(Companyid) == 1183)
            //{
            //    //return
            //    //    Redirect(
            //    //        string.Format(
            //    //            _uniquePageService.GetUniquePage<EnrolleeRegPage>().AbsoluteUrl + "?companyid={0}",
            //    //            Companyid));
            //}
            //else
            //{

            //}


        }

        public ActionResult EnrolleeReg(EnrolleeRegPage page, int? Companyid)
        {

            //return Redirect("http://197.255.55.54/portal/" + "?companyid=" + Convert.ToString(Companyid));
            //EnrolleeRegPage page = new Pages.EnrolleeRegPage();
            //page.Companyid = Convert.ToInt32(Companyid);
            var company = _companyService.GetCompany(Convert.ToInt32(Companyid));
            page.Companyid = Convert.ToInt32(Companyid);
            ViewBag.CompanyName = company != null ? company.Name : "--";
            var maritallist = Enum.GetValues(typeof(MaritalStatus));
            ViewBag.MaritalStatus = (from object item in maritallist
                                     select new DdListItem()
                                     {
                                         Id = Convert.ToString((int)item),
                                         Name = Enum.GetName(typeof(MaritalStatus), item)
                                     }).ToList();


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();


            var sponsorlist = Enum.GetValues(typeof(Sponsorshiptype));
            ViewBag.Sponsorshiptype = (from object item in sponsorlist
                                       select new DdListItem()
                                       {
                                           Id = Convert.ToString((int)item),
                                           Name = _helperSvc.GetDescription((Sponsorshiptype)item)
                                       }).ToList();



            //ViewBag.Company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId)).Name.ToUpper();
            //ViewBag.CompanySubsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary).Subsidaryname.ToUpper();



            var states = _helperSvc.GetallStates();


            ViewBag.providerlist = _providerSvc.GetallProvider();
            //Load the States
            var stateout = new List<State>();

            stateout.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (var item in states)
            {

                stateout.Add(item);
            }
            ViewBag.statess = stateout;

            return View(page);
        }

        public JsonResult GetEnrolleePolicyNumberName(string phrase)
        {
            var items = new List<EnrolleePolicyName>();
            if (!string.IsNullOrEmpty(phrase) && phrase.Length > 7)
            {
                items = _enrolleeService.GetEnrolleePolicyNumberName(phrase.ToUpper()).ToList<EnrolleePolicyName>();
            }



            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult gettempEnrollee(string staffid)
        {
            //return the temp id
            var tempenrollee = _enrolleeService.GetTempEnrolleebystaffid(staffid);
            if (tempenrollee != null)
            {
                var today = CurrentRequestData.Now;
                var principaldob = "";
                var spousedob = "";
                var childdob1 = "";
                var childdob2 = "";
                var childdob3 = "";
                var childdob4 = "";

                if (tempenrollee.Dob.Year != 1917)
                {
                    principaldob = tempenrollee.Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.s_Dob.Year != 1917)
                {
                    spousedob = tempenrollee.s_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child1_Dob.Year != 1917)
                {
                    childdob1 = tempenrollee.child1_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child2_Dob.Year != 1917)
                {
                    childdob2 = tempenrollee.child2_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child3_Dob.Year != 1917)
                {
                    childdob3 = tempenrollee.child3_Dob.ToString("dd/MM/yyyy");
                }

                if (tempenrollee.child4_Dob.Year != 1917)
                {
                    childdob4 = tempenrollee.child4_Dob.ToString("dd/MM/yyyy");
                }


                var response = new
                {
                    Id = tempenrollee.Id,
                    enrollee = tempenrollee,
                    mainimage = Convert.ToBase64String(tempenrollee.Imgraw),
                    spouseimg = Convert.ToBase64String(tempenrollee.S_Imgraw),
                    child1img = Convert.ToBase64String(tempenrollee.child1_Imgraw),
                    child2img = Convert.ToBase64String(tempenrollee.child2_Imgraw),
                    child3img = Convert.ToBase64String(tempenrollee.child3_Imgraw),
                    child4img = Convert.ToBase64String(tempenrollee.child4_Imgraw),
                    dob = principaldob,
                    spousedob = spousedob,
                    child1dob = childdob1,
                    child2dob = childdob2,
                    child3dob = childdob3,
                    child4dob = childdob4,

                    companyname =
                    _companyService.GetCompany(tempenrollee.Companyid) != null
                        ? _companyService.GetCompany(tempenrollee.Companyid).Name
                        : "--",

                };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                tempenrollee = new Entities.TempEnrollee();
            }

            return Json(tempenrollee, JsonRequestBehavior.AllowGet);

        }

        public JsonResult getstafffbyCompanystaffid(string staffid)
        {

            var staff = _companyService.GetstaffByCompanyStaffId(staffid);

            if (staff != null)
            {
                var response = new
                {
                    staff = staff,
                    exist = true,

                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }

            return Json("{}", JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult EnrolleeRegHome(EnrolleeRegHomePage page, int? Companyid)
        {
            //return Redirect("http://197.255.55.54/portal/" + "?companyid=" + Convert.ToString(Companyid));

            var company = _companyService.GetCompany(Convert.ToInt32(Companyid));
            var items = _companyService.GetAllStaffInCompany(Convert.ToInt32(Companyid));

            //var items2 = new List<Staff>();



            ViewBag.StaffList = items;
            ViewBag.CompanyName = company != null ? company.Name.ToUpper() : "--";
            ViewBag.CompanyID = Companyid;
            return View(page);
        }
        [HttpPost]
        public ActionResult EnrolleeRegHome(FormCollection form)
        {

            var companyID = form["CompanyId"];
            var staffid = form["StaffFullname"];

            if (string.IsNullOrEmpty(companyID) || string.IsNullOrWhiteSpace(staffid))
            {
                //
                _pageMessageSvc.SetErrormessage("There was an error ,kindly check your input.");
                return _uniquePageService.RedirectTo<EnrolleeRegHomePage>();

            }

            //check if the staff _still exist
            var stafff =
                _companyService.Getstaff(Convert.ToInt32(staffid));
            if (stafff != null)
            {
                if (stafff.HasProfile)
                {

                    var enrollees = _enrolleeService.GetEnrolleesByStaffId(stafff.Id);
                    var enrolleemodel = enrollees.Where(x => x.Parentid == 0).SingleOrDefault();
                    if (enrolleemodel != null)
                    {
                        //Session["EnrolleeGuid"] = enrolleemodel.Guid.ToString();
                        _pageMessageSvc.SetErrormessage("You already have a profile kindly login with your policy number.Thank you");

                        return _uniquePageService.RedirectTo<EnrolleePortalUserHomePage>();

                        //return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleemodel.Guid.ToString());
                    }


                }

                //redirect to the regPage
                return
                 Redirect(string.Format(_uniquePageService.GetUniquePage<EnrolleeRegPublicPage>().AbsoluteUrl + "?Companyid={0} &staffIdnum={1}",
                    companyID, staffid));
            }
            _pageMessageSvc.SetErrormessage("There was an error ,the staff doea not exist on the system.Kindly contact admin.");
            return _uniquePageService.RedirectTo<EnrolleeRegHomePage>();

        }

        public ActionResult EnrolleeRegPublic(EnrolleeRegPublicPage page, int? Companyid, int? staffIdnum)
        {

            //check if the staff exist already

            var tempenrollee = _enrolleeService.GetTempEnrolleebystaffProfileid((Convert.ToInt32(staffIdnum)));

            if (tempenrollee != null)
            {
                //old guy exist niccur
                var today = CurrentRequestData.Now;
                var principaldob = "";
                var spousedob = "";
                var childdob1 = "";
                var childdob2 = "";
                var childdob3 = "";
                var childdob4 = "";

                if (tempenrollee.Dob.Year != 1917)
                {
                    principaldob = tempenrollee.Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.s_Dob.Year != 1917)
                {
                    spousedob = tempenrollee.s_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child1_Dob.Year != 1917)
                {
                    childdob1 = tempenrollee.child1_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child2_Dob.Year != 1917)
                {
                    childdob2 = tempenrollee.child2_Dob.ToString("dd/MM/yyyy");
                }
                if (tempenrollee.child3_Dob.Year != 1917)
                {
                    childdob3 = tempenrollee.child3_Dob.ToString("dd/MM/yyyy");
                }

                if (tempenrollee.child4_Dob.Year != 1917)
                {
                    childdob4 = tempenrollee.child4_Dob.ToString("dd/MM/yyyy");
                }

                ViewBag.OldGuy = new TempEnrolleeResponse();
                var response = new TempEnrolleeResponse
                {
                    Id = tempenrollee.Id,
                    enrollee = tempenrollee,
                    mainimage = Convert.ToBase64String(tempenrollee.Imgraw),
                    spouseimg = Convert.ToBase64String(tempenrollee.S_Imgraw),
                    child1img = Convert.ToBase64String(tempenrollee.child1_Imgraw),
                    child2img = Convert.ToBase64String(tempenrollee.child2_Imgraw),
                    child3img = Convert.ToBase64String(tempenrollee.child3_Imgraw),
                    child4img = Convert.ToBase64String(tempenrollee.child4_Imgraw),
                    dob = principaldob,
                    spousedob = spousedob,
                    child1dob = childdob1,
                    child2dob = childdob2,
                    child3dob = childdob3,
                    child4dob = childdob4,

                    companyname =
                    _companyService.GetCompany(tempenrollee.Companyid) != null
                        ? _companyService.GetCompany(tempenrollee.Companyid).Name
                        : "--",

                };

                ViewBag.OldGuy = response;
            }
            //EnrolleeRegPage page = new Pages.EnrolleeRegPage();
            //page.Companyid = Convert.ToInt32(Companyid);
            var company = _companyService.GetCompany(Convert.ToInt32(Companyid));
            var staff = _companyService.Getstaff(Convert.ToInt32(staffIdnum));

            ViewBag.Staffullname = staff.StaffFullname.ToUpper();
            page.Companyid = Convert.ToInt32(Companyid);
            ViewBag.CompanyName = company != null ? company.Name : "--";
            var maritallist = Enum.GetValues(typeof(MaritalStatus));
            ViewBag.MaritalStatus = (from object item in maritallist
                                     select new DdListItem()
                                     {
                                         Id = Convert.ToString((int)item),
                                         Name = Enum.GetName(typeof(MaritalStatus), item)
                                     }).ToList();


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();


            var sponsorlist = Enum.GetValues(typeof(Sponsorshiptype));
            ViewBag.Sponsorshiptype = (from object item in sponsorlist
                                       select new DdListItem()
                                       {
                                           Id = Convert.ToString((int)item),
                                           Name = _helperSvc.GetDescription((Sponsorshiptype)item)
                                       }).ToList();



            //ViewBag.Company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId)).Name.ToUpper();
            //ViewBag.CompanySubsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary).Subsidaryname.ToUpper();



            var states = _helperSvc.GetallStates();


            ViewBag.providerlist = _providerSvc.GetallProvider();
            //Load the States
            var stateout = new List<State>();

            stateout.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (var item in states)
            {

                stateout.Add(item);
            }
            ViewBag.statess = stateout;

            ViewBag.staffIdnum = staffIdnum;
            var plan = new CompanyPlan();

            ViewBag.Plan = "Unknown";
            if (staff != null)
            {
                plan = _companyService.GetCompanyPlan(staff.StaffPlanid);
                ViewBag.Plan = plan.Planfriendlyname.ToUpper();
            }


            //get hospitallist based on plan

            var hospitallist = _providerSvc.GetallProviderByPlan(plan.Planid);
            ViewBag.HospitalList = hospitallist.OrderBy(x => x.Name).ToList();



            return View(page);
        }

        public JsonResult GetStatebyZone(int id)
        {
            var rawresp = _helperSvc.GetStatesinZone(id);

            if (id < 0)
            {
                //reset to all  states if its 
                rawresp = _helperSvc.GetallStates();
            }
            var resp = new List<GenericReponse2>();

            foreach (var item in rawresp)
            {
                var newobj = new GenericReponse2();
                newobj.Id = item.Id;
                newobj.Name = item.Name.ToString();

                resp.Add(newobj);
            }


            return Json(resp, JsonRequestBehavior.AllowGet);


        }

        public ActionResult HomePage(HomePage page)
        {

            ViewBag.CompanyCount = _companyService.CompanyCount();
            ViewBag.EnrolleeCount = _enrolleeService.EnrolleeCount();
            ViewBag.ProviderCount = _providerSvc.ProviderCount();
            ViewBag.UsersCount = _helperSvc.usersCount();
            var start = CurrentRequestData.Now.StartOfWeek(DayOfWeek.Monday);

            var datete = Convert.ToDateTime(CurrentRequestData.Now);

            var dd = 1;
            var month = datete.Month;
            var year = datete.Year;
            var time = "00:01";
            var startdate = Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}", month, dd, year, time));
            var theplan = new Dictionary<int, int>();
            ViewBag.AdmissionCount = _claimService.GetNumberofAdmission(start, CurrentRequestData.Now, ref theplan);
            var narrations = "";

            foreach (var item in theplan)
            {

                if (item.Value > 0)
                {
                    var theeeshii = _planService.GetPlan(item.Key);
                    var namee = theeeshii != null ? theeeshii.Name : "--";
                    narrations = narrations + "," + namee + " - " + item.Value.ToString();

                }


            }

            ViewBag.plannara = narrations;

            var result = _claimService.GetutilizationReport(startdate, CurrentRequestData.Now);
            var itmes = result.OrderByDescending(x => x.Value);
            var hresult = _claimService.GetProviderutilizationReport(startdate, CurrentRequestData.Now);
            var itmes2 = hresult.OrderByDescending(x => x.Value);
            var utlist = new List<homechart>();
            var uHtlist = new List<homechart>();
            var colorcode = new Dictionary<int, string>();
            var colorcode2 = new Dictionary<int, string>();

            colorcode.Add(1, "#f56954,#f56954");
            colorcode.Add(2, "#00a65a,#00a65a");
            colorcode.Add(3, "#f39c12,#f39c12");
            colorcode.Add(4, "#00c0ef,#00c0ef");
            colorcode.Add(5, "#3c8dbc,#3c8dbc");
            colorcode.Add(6, "#d2d6de,#d2d6de");


            colorcode2.Add(1, "#F4D03F,#F4D03F");
            colorcode2.Add(2, "#2874A6,#2874A6");
            colorcode2.Add(3, "#707B7C,#707B7C");
            colorcode2.Add(4, "#5B2C6F,#5B2C6F");
            colorcode2.Add(5, "#E74C3C,#E74C3C");
            colorcode2.Add(6, "#D0D3D4,#D0D3D4");

            var count = 1;
            foreach (var item in itmes)
            {
                var objj = new homechart();
                var company = _companyService.GetCompany(Convert.ToInt32(item.Key));

                if (company != null && count < 7)
                {
                    objj.label = company.Name.ToUpper();
                    objj.color = colorcode[count].Split(',')[0];
                    objj.highlight = colorcode[count].Split(',')[1];
                    objj.value = item.Value;
                    utlist.Add(objj);
                    count++;

                }

            }



            ViewBag.utilReport = utlist;
            var count2 = 1;
            foreach (var item in itmes2)
            {
                var objj = new homechart();
                var provider = _providerSvc.GetProvider(Convert.ToInt32(item.Key));

                if (provider != null && count2 < 7 && !string.IsNullOrEmpty(provider.Name))
                {
                    objj.label = provider.Name.ToUpper();
                    objj.color = colorcode2[count2].Split(',')[0];
                    objj.highlight = colorcode2[count2].Split(',')[1];
                    objj.value = item.Value;
                    uHtlist.Add(objj);
                    count2++;

                }

            }

            ViewBag.utilHReport = uHtlist;


            return View(page);
        }



        public ActionResult SaveBeneficiary(FormCollection form)
        {

            var s_fullname = form["fullname_sponsor"];
            var s_gender = form["gender"];
            var s_Occupation = form["Occupation"];
            var s_country = form["country"];
            var s_state = form["state"];
            var s_Address = form["Address"];
            var s_email = form["email"];
            var s_phonenumber = form["phonenumber"];
            var s_subType = form["sub_type"];
            var beneficiary_count = form["beneficiary_count"];
            var biodun = form["sponsor's_hair"];
            var sponsor = new ConnectCareSponsor();
            sponsor.fullname = s_fullname;
            sponsor.gender = s_gender;
            sponsor.occupation = s_Occupation;
            sponsor.country = s_country;
            sponsor.state = s_state;
            sponsor.address = s_Address;
            sponsor.email = s_email;
            sponsor.phonenumber = s_phonenumber;
            sponsor.SubscriptionType = s_subType;


            var beneficiarycount = 1;


            _helperSvc.addSponsor(sponsor);


            if (int.TryParse(beneficiary_count, out beneficiarycount))
            {
                for (int i = 1; i <= beneficiarycount; i++)
                {
                    var b_fullname = form["fullnamebene_" + i.ToString()];
                    var b_gender = form["genderbene_" + i.ToString()];
                    var b_relationship = form["bene_relationship_" + i.ToString()];
                    var b_country = form["bene_country_" + i.ToString()];
                    var b_state = form["bene_state_" + i.ToString()];
                    var b_Address = form["bene_Address_" + i.ToString()];
                    var b_city = form["bene_city_" + i.ToString()];
                    var b_email = form["bene_email_" + i.ToString()];
                    var b_phonenumber = form["bene_phonenumber_" + i.ToString()];
                    var b_age = form["bene_age_" + i.ToString()];

                    var beneficiary = new ConnectCareBeneficiary();

                    //beneficiary.sponsorid = sponsor.Id;
                    beneficiary.fullname = b_fullname;
                    beneficiary.gender = b_gender;
                    beneficiary.relationship = b_relationship;
                    beneficiary.country = b_country;
                    beneficiary.state = b_state;
                    beneficiary.address = b_Address;
                    beneficiary.city = b_city;
                    beneficiary.email = b_email;
                    beneficiary.phonenumber = b_phonenumber;
                    var bene_age = 0;
                    int.TryParse(b_age, out bene_age);

                    beneficiary.BeneficiaryCat = b_age;
                    beneficiary.age = bene_age;



                    _helperSvc.addBeneficiary(beneficiary);


                }
            }


            string appdatafolder = Path.Combine(CurrentRequestData.CurrentContext.Request.PhysicalApplicationPath,
               "App_Data");

            var folderpath = Path.Combine(appdatafolder, "DropUpload");
            var filename = Path.Combine(folderpath, "connectcareemail.txt");

            var template = System.IO.File.ReadAllText(filename);

            var bodyy = template.Replace("@fullname@", s_fullname.ToUpper());



            if (!string.IsNullOrEmpty(s_email))
            {
                //_helperSvc.PushUserNotification 
                var emailmsg = new QueuedMessage();
                emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg.ToAddress = s_email;
                emailmsg.Subject = "ConnectCare - Complete Registration";
                emailmsg.FromName = "Novo Health Africa";
                emailmsg.Body = bodyy;
                emailmsg.IsHtml = true;

                _emailSender.AddToQueue(emailmsg);


                //_helperSvc.PushUserNotification 
                var emailmsg2 = new QueuedMessage();
                emailmsg2.FromAddress = _mailSettings.SystemEmailAddress;
                emailmsg2.ToAddress = "connectcare@novohealthafrica.org";
                emailmsg2.Subject = "ConnectCare - New Registration";
                emailmsg2.FromName = "Novo Health Africa";
                emailmsg2.Body = bodyy;
                emailmsg2.IsHtml = true;

                _emailSender.AddToQueue(emailmsg2);
            }

            return Redirect("https://novohubonline.com/connectcare/ConnectReg.html?status=1");
        }

        public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
                base.OnActionExecuting(filterContext);
            }
        }
        [AllowCrossSiteJson]
        public JsonResult GetEnrolleeDetailsbyStaffId(int staffIdnum)
        {
            //CurrentRequestData.CurrentContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");

            var staff = _companyService.Getstaff(staffIdnum);

            if (staff != null)
            {
                var plan = _companyService.GetCompanyPlan(staff.StaffPlanid);
                var hospitallist = _providerSvc.GetallProviderByPlan(plan.Planid);
                var company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId));
                var hospitallistout = new List<GenericReponse2>();

                foreach (var item in hospitallist)
                {
                    hospitallistout.Add(new GenericReponse2 { Id = item.Id, Name = item.Name.ToUpper() + " - " + item.Address.ToLower() });

                }

                var resp = new
                {
                    Id = staff.Id,
                    staffname = staff.StaffFullname,
                    plan = plan.Planfriendlyname.ToUpper(),
                    company = company.Name.ToUpper(),
                    provider = hospitallistout,
                    respcode = 0,
                    respmsg = "successful"

                };

                return Json(resp, JsonRequestBehavior.AllowGet);

            }
            return Json(new
            {
                respcode = 99,
                respmsg = "Staff Does not exist."
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult EnrolleePortalLogin(EnrolleePortalLoginPage page)
        {


            return View(page);
        }

        [HttpGet]
        public ActionResult EnrolleePortalLogout()
        {

            Session["EnrolleeGuid"] = string.Empty;
            _pageMessageSvc.SetSuccessMessage("You have logged out successfully.");

            return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();


        }

        public ActionResult AddPendingDependant(FormCollection form)
        {


            var enrolleeid = form["enrolleeGUID"];
            var relationship = form["relationship"];
            var surnameD = form["surnameD"];
            var othernamesD = form["othernamesD"];
            var dob = form["datepicker2"];
            var sexD = form["sexD"];
            var mobilenumberD = form["mobilenumberD"];
            var providerD = form["providerD"];
            var premedicalconditionD = form["premedicalconditionD"];
            var additionalnote = form["additionalnote"];


            var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileD"];

            //validate the shit
            if (!string.IsNullOrEmpty(enrolleeid) && !string.IsNullOrEmpty(relationship) && !string.IsNullOrEmpty(surnameD) && !string.IsNullOrEmpty(othernamesD)
                && !string.IsNullOrEmpty(mobilenumberD))
            {

            }
            else
            {
                _pageMessageSvc.SetErrormessage("Kindly fill the dependant form properly.");

                return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleeid);
            }




            var dobtin = !string.IsNullOrEmpty(dob)
                ? Utility.Tools.ParseMilitaryTime("0101", Convert.ToInt32(dob.Substring(6, 4)),
                    Convert.ToInt32(dob.Substring(3, 2)), Convert.ToInt32(dob.Substring(0, 2)))
                : CurrentRequestData.Now.AddYears(-100);
            DateTime today = CurrentRequestData.Now;

            if (today.Year - dobtin.Year > 21 &&
                (Relationship)Enum.Parse(typeof(Relationship), relationship) == Relationship.Child)
            {
                //above limit

                _pageMessageSvc.SetErrormessage("The Dependant Age is above 21 , Dependant age must be below 21 years.");

                return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleeid);

            }
            byte[] imgData = null;

            if (image != null && image.ContentLength > 0)
            {
                Image image2 = Image.FromStream(image.InputStream);

                //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                var memoryStream = new MemoryStream();
                image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = memoryStream.ToArray();

            }

            var dependant = new PendingDependant();
            if (imgData != null && imgData.Length > 0)
            {
                dependant.ImgRaw = imgData;
            }

            dependant.firstName = othernamesD;
            dependant.lastname = surnameD;
            dependant.dob = dobtin;
            dependant.sex = Convert.ToInt32(sexD);
            dependant.hospital = Convert.ToInt32(providerD);
            dependant.mobile = mobilenumberD;
            dependant.preexisting = premedicalconditionD;
            dependant.relationship = Convert.ToInt32(relationship);
            dependant.Note = additionalnote;
            dependant.principalGuid = enrolleeid;

            if (_enrolleeService.AddTempDependant(dependant))
            {
                _pageMessageSvc.SetSuccessMessage("You have  added a dependant succesfully,kindly note that the dependant is subjected to approval.We will contact you shortly.");
            }
            else
            {
                _pageMessageSvc.SetErrormessage("There was a problem adding the dependant.kindly try again.");
            }
            return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleeid);

        }

        [HttpGet]
        public ActionResult EnrolleePortalUserHome(EnrolleePortalUserHomePage page, string EnrolleeId, string logout)
        {
            //set the session id

            if (!string.IsNullOrEmpty(logout) && logout == "logout")
            {
                Session["EnrolleeGuid"] = string.Empty;
                _pageMessageSvc.SetSuccessMessage("You have logged out successfully.");

                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();
            }

            var enrollleidsaved = (string)Session["EnrolleeGuid"];

            if (!string.IsNullOrEmpty(enrollleidsaved) && enrollleidsaved.Equals(EnrolleeId, StringComparison.CurrentCultureIgnoreCase))
            {

                //logeed in 

            }
            else
            {
                _pageMessageSvc.SetErrormessage("You are not logged in ,Kindly login to access portal.");
                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();

            }





            var enrolleemodel = _enrolleeService.GetEnrolleeGuid(EnrolleeId);



            var staff = new Staff();
            var idint = 0;

            staff = _companyService.Getstaff(enrolleemodel.Staffprofileid);

            ViewBag.Stafffullname = staff != null ? staff.StaffFullname.ToUpper() : "--";
            ViewBag.StaffID = staff.Id;
            ViewBag.companyPlanId = staff.StaffPlanid;


            var maritallist = Enum.GetValues(typeof(MaritalStatus));
            ViewBag.MaritalStatus = (from object item in maritallist
                                     select new DdListItem()
                                     {
                                         Id = Convert.ToString((int)item),
                                         Name = Enum.GetName(typeof(MaritalStatus), item)
                                     }).ToList();


            var sexlist = Enum.GetValues(typeof(Sex));
            ViewBag.Sexx = (from object item in sexlist
                            select new DdListItem()
                            {
                                Id = Convert.ToString((int)item),
                                Name = Enum.GetName(typeof(Sex), item)
                            }).ToList();


            var sponsorlist = Enum.GetValues(typeof(Sponsorshiptype));
            ViewBag.Sponsorshiptype = (from object item in sponsorlist
                                       select new DdListItem()
                                       {
                                           Id = Convert.ToString((int)item),
                                           Name = _helperSvc.GetDescription((Sponsorshiptype)item)
                                       }).ToList();




            ViewBag.Company = _companyService.GetCompany(Convert.ToInt32(staff.CompanyId)).Name.ToUpper();
            ViewBag.CompanySubsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary).Subsidaryname.ToUpper();
            PlanVm plan = null;
            var plannn = _companyService.GetCompanyPlan(staff.StaffPlanid);
            plan = _planService.GetPlan(plannn.Planid);

            ViewBag.SubscriptionType = plan.Name.ToUpper();


            var states = _helperSvc.GetallStates();
            ViewBag.canaddependant = plannn.AllowChildEnrollee;

            ViewBag.providerlist = _providerSvc.GetallProviderByPlan(plan.Id).OrderBy(x => x.Name);


            //Load the States
            var statess = new List<State>();
            statess.Add(new State() { Id = -1, Name = "--SELECT--" });
            foreach (var item in states)
            {

                statess.Add(item);
            }

            ViewBag.Statesss = statess;





            ViewBag.idCardPrintedValue = enrolleemodel.IdCardPrinted;

            page.Enrolleemodel = enrolleemodel;

            ViewBag.enrolleeimg = Convert.ToBase64String(enrolleemodel.EnrolleePassport.Imgraw);
            //var plan = _companyService.Getallplan().Where(x => x.Companyid == enrolleemodel.Companyid).Take(1);
            ViewBag.DependentsEnabled = (bool)
                _companyService.GetCompanyPlan(
                        _companyService.Getstaff(enrolleemodel.Staffprofileid).StaffPlanid).
                    AllowChildEnrollee;
            ViewBag.LGAS = _helperSvc.GetLgainstate(enrolleemodel.Stateid);
            ViewBag.companyPlanId = staff.StaffPlanid;



            ViewBag.enrolleeimg = Convert.ToBase64String(enrolleemodel.EnrolleePassport.Imgraw);
            //
            ViewBag.Staff_StaffId = staff.StaffId;

            ViewBag.DateAddedEnrolle =
                Convert.ToDateTime(enrolleemodel.CreatedOn)
                    .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
            ViewBag.AddedByEnrollee = enrolleemodel.Createdby > 0
                ? _userservice.GetUser(enrolleemodel.Createdby).Name
                : "Auto Upload";
            ViewBag.companyPlanId = staff.StaffPlanid;
            if (enrolleemodel.Isexpundged)
            {
                ViewBag.ExpungedByEnrollee = _userservice.GetUser(enrolleemodel.Expungedby).Name;
                ViewBag.DateExpungedEnrollee =
                    Convert.ToDateTime(enrolleemodel.Dateexpunged)
                        .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
            }






            //dependants
            var today = CurrentRequestData.Now;


            var dependentslist =
                _enrolleeService.GetDependentsEnrollee(Convert.ToInt32(enrolleemodel.Id)).Where(
                        x => x.IsDeleted == false && x.Isexpundged == false && x.Status == (int)EnrolleesStatus.Active)
                    .
                    ToList();


            var dependlistformedhist = new List<GenericReponse2>();
            // add the principal
            var iteem2 = new GenericReponse2();
            iteem2.Id = enrolleemodel.Id;
            iteem2.Name = enrolleemodel.Surname + " " + enrolleemodel.Othernames + " (" + enrolleemodel.Policynumber + " )";

            dependlistformedhist.Add(iteem2);

            foreach (var item in dependentslist)
            {
                var iteem = new GenericReponse2();
                iteem.Id = item.Id;
                iteem.Name = item.Surname + " " + item.Othernames + " (" + item.Policynumber + " )";


                dependlistformedhist.Add(iteem);

            }

            ViewBag.medhistpolicynum = dependlistformedhist;


            var output = new List<DependantInfomation>();
            var counttt = 0;
            foreach (var areply in dependentslist)
            {
                var proder = _providerSvc.GetProvider(areply.Primaryprovider);
                var tiko = new DependantInfomation
                {
                    Id = areply.Id,
                    Name = areply.Surname + " " + areply.Othernames,
                    dob = Convert.ToDateTime(areply.Dob).ToString("MMM dd yyyy"),
                    sex = Enum.GetName(typeof(Sex), areply.Sex),
                    hospital = proder != null ? proder.Name.ToUpper() : "--",
                    mobile = areply.Mobilenumber,
                    providerID = enrolleemodel.Primaryprovider,
                    preexisting = areply.Preexistingmedicalhistory,
                    relationship = Enum.GetName(typeof(Relationship), areply.Parentrelationship),
                    img =
                                           areply.EnrolleePassport.Imgraw != null
                                               ? Convert.ToBase64String(areply.EnrolleePassport.Imgraw)
                                               : string.Empty,
                    policynum = areply.Policynumber,
                    aboveage = (today.Year - Convert.ToDateTime(areply.Dob).Year) >= 20 ? true : false,
                    ispending = false
                };

                output.Add(tiko);
                counttt++;
            }


            var pendingdependant = _enrolleeService.getTempDependant(enrolleemodel.Guid.ToString());

            foreach (var areply in pendingdependant)
            {
                var proder = _providerSvc.GetProvider(areply.hospital);
                var tiko = new DependantInfomation
                {
                    Id = areply.Id,
                    Name = areply.lastname + " " + areply.firstName,
                    dob = Convert.ToDateTime(areply.dob).ToString("MMM dd yyyy"),
                    sex = Enum.GetName(typeof(Sex), areply.sex),
                    hospital = proder != null ? proder.Name.ToUpper() : "--",
                    mobile = areply.mobile,
                    providerID = enrolleemodel.Primaryprovider,
                    preexisting = areply.preexisting,
                    relationship = Enum.GetName(typeof(Relationship), areply.relationship),
                    img =
                             areply.ImgRaw != null
                                 ? Convert.ToBase64String(areply.ImgRaw)
                                 : string.Empty,
                    policynum = "Pending",
                    aboveage = (today.Year - Convert.ToDateTime(areply.dob).Year) >= 20 ? true : false,
                    ispending = true
                };

                output.Add(tiko);


            }




            ViewBag.Dependants = output.OrderBy(x => x.policynum).ToList();
            ViewBag.Dependantscounts = counttt;

            //edit dependants
            var relationshiplist = Enum.GetValues(typeof(Relationship));
            ViewBag.relationshiplist = (from object item in relationshiplist
                                        select new DdListItem()
                                        {
                                            Id = Convert.ToString((int)item),
                                            Name = Enum.GetName(typeof(Relationship), item)
                                        }).ToList();

            ViewBag.Sex = (from object item in sexlist
                           select new DdListItem()
                           {
                               Id = Convert.ToString((int)item),
                               Name = Enum.GetName(typeof(Sex), item)
                           }).ToList();


            var providerrrs = _providerSvc.GetallProviderByPlan(plannn.Planid);
            var p_provider = new List<GenericReponse2>();
            foreach (var item in providerrrs)
            {
                var P_item = new GenericReponse2();
                P_item.Id = item.Id;
                P_item.Name = item.Name.ToUpper() + " - " + item.Address.ToLower();
                p_provider.Add(P_item);
            }
            ViewBag.providerlist = p_provider;




            //this guy handles temp id card 

            var hassub = false;
            //initialize with 100 days ago
            var expirationdate = CurrentRequestData.Now.AddDays(-100);

            if (enrolleemodel != null)
            {


                var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                var sub_sub =
                    _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                //var validsub = comp_sub || sub_sub;

                if (sub_sub || comp_sub)
                {
                    var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                        staff.CompanySubsidiary);

                    var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                    if (Ssubscription != null &&
                        (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        expirationdate = Convert.ToDateTime(Ssubscription.Expirationdate);
                        hassub = true;
                    }

                    //check for company subscription only if the sub doesn't have
                    if (csubscription != null && !hassub &&
                        (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                    {
                        expirationdate = Convert.ToDateTime(csubscription.Expirationdate);
                        hassub = true;
                    }
                    //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                    //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                }

                //give 3 months expiration based on the subscription
                var newexpiration = CurrentRequestData.Now.AddDays(90);

                if (newexpiration < expirationdate)
                {
                    expirationdate = newexpiration;

                }

                ViewBag.Subscriptionexp = expirationdate.ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);

                ViewBag.principaltempId = new DependantInfomation
                {
                    Id = enrolleemodel.Id,
                    Name = enrolleemodel.Surname + " " + enrolleemodel.Othernames,
                    dob = Convert.ToDateTime(enrolleemodel.Dob).ToString("MMM dd yyyy"),
                    sex = Enum.GetName(typeof(Sex), enrolleemodel.Sex),
                    hospital = _providerSvc.GetProvider(enrolleemodel.Primaryprovider).Name.ToUpper(),
                    providerID = enrolleemodel.Primaryprovider,
                    mobile = enrolleemodel.Mobilenumber,
                    preexisting = enrolleemodel.Preexistingmedicalhistory,
                    relationship = Enum.GetName(typeof(Relationship), enrolleemodel.Parentrelationship),
                    img =
                             enrolleemodel.EnrolleePassport.Imgraw != null
                                 ? Convert.ToBase64String(enrolleemodel.EnrolleePassport.Imgraw)
                                 : string.Empty,
                    policynum = enrolleemodel.Policynumber,
                    aboveage = (today.Year - Convert.ToDateTime(enrolleemodel.Dob).Year) >= 20 ? true : false
                };

                //check if can update passport

                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                var imgData = System.IO.File.ReadAllBytes(path);
                ViewBag.CanUpdatePassport = false;

                if (enrolleemodel.EnrolleePassport.Imgraw.SequenceEqual(imgData))
                {
                    ViewBag.CanUpdatePassport = true;
                }



            }

            return View(page);
        }

        [HttpPost]

        public ActionResult EnrolleePortalLogin(FormCollection form)
        {
            var enrolleemodel = !string.IsNullOrEmpty(form["policynumberTxt"]) ? _enrolleeService.GetEnrolleeByPolicyNumber(form["policynumberTxt"]) : null;
            if (enrolleemodel != null)
            {

                if (enrolleemodel.Parentid > 0)
                {
                    _pageMessageSvc.SetErrormessage("You are a dependant,Only principal enrollee can use this portal.");
                    return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();
                }

                if (enrolleemodel.Isexpundged)
                {
                    _pageMessageSvc.SetErrormessage("There are some issues with your policy number, Kindly contact Us. Thank you");
                    return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();
                }

                Session["EnrolleeGuid"] = enrolleemodel.Guid.ToString();


                return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleemodel.Guid.ToString());
            }
            else
            {
                _pageMessageSvc.SetErrormessage("The policy number is invalid.");
                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();

            }

            return null;
        }
        public JsonResult GetEnrolleebyMobile(string mobileno)
        {
            var response = new object();
            response = new
            {
                respcode = 99,
                respmsg = "We couldn't find the phone number in our database."
            };
            if (!string.IsNullOrEmpty(mobileno))
            {
                var enrollees = _enrolleeService.GetEnrolleesbyPhone(mobileno);


                if (enrollees.Any())
                {

                    var enrollee = new Enrollee();

                    if (enrollees.Count > 1)
                    {
                        var parent = enrollees.Where(x => x.Parentid == 0).FirstOrDefault();
                        enrollee = parent;

                    }
                    enrollee = enrollees.FirstOrDefault();

                    var staff = _companyService.Getstaff(enrollee.Staffprofileid);
                    var company = _companyService.GetCompany(enrollee.Companyid).Name.ToUpper();
                    var subsidiary = _companyService.Getsubsidiary(staff.CompanySubsidiary).Subsidaryname.ToUpper();
                    var plan = _companyService.GetCompanyPlan(staff.StaffPlanid).Planfriendlyname.ToUpper();


                    response = new
                    {

                        Id = enrollee.Id,
                        fullname = (enrollee.Surname + " " + enrollee.Othernames).ToUpper(),
                        policynumber = enrollee.Policynumber.ToUpper(),
                        company = company,
                        subsidiary = subsidiary,
                        Plan = plan,
                        isexpunged = enrollee.Isexpundged,
                        dateexpunged = Convert.ToDateTime(enrollee.Dateexpunged).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern),
                        respcode = 00,
                        respmsg = "Successfull"
                    };


                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetEVSCodeDetails(string evcode)
        {
            var response = new VerificationCodeResponse();
            response.Id = "-1";


            if (!string.IsNullOrEmpty(evcode))
            {
                var areply = _helperSvc.GetverificationByVerificationCode(evcode);

                if (areply != null)
                {
                    var responses = new VerificationCodeResponse();
                    var enrolee = _enrolleeService.GetEnrollee(areply.EnrolleeId);
                    responses.Id = areply.Id.ToString();
                    responses.Enrolleeid = areply.EnrolleeId.ToString();
                    responses.EnrolleeisChild = enrolee != null ? enrolee.Parentid > 0 : false;
                    responses.StaffProfileId = enrolee != null ? enrolee.Staffprofileid.ToString() : "";
                    responses.EnrolleePolicy = enrolee != null ? enrolee.Policynumber : "--";
                    responses.Enrolleename =
                       enrolee != null ? enrolee.Surname + " " + enrolee.Othernames : "";
                    responses.Dateencountered =
                        Convert.ToDateTime(areply.EncounterDate)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);
                    responses.Verificationcode = areply.VerificationCode;
                    responses.Verficationstatus =
                        Enum.GetName(typeof(EnrolleeVerificationCodeStatus), areply.Status);


                    var provider = _providerSvc.GetProvider(areply.ProviderId);
                    if (provider != null && provider.Name != null)
                    {
                        responses.Providerused = provider.Name;
                    }
                    else
                    {
                        responses.Providerused = "--";
                    }

                    responses.Channel = Enum.GetName(typeof(ChannelType), areply.Channel);
                    responses.Purpose = Enum.GetName(typeof(PurposeOfVisit), areply.VisitPurpose);
                    responses.Dateauthenticated = areply.DateAuthenticated != null
                        ? Convert.ToDateTime(areply.DateAuthenticated)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern)
                        : "--";
                    responses.Dateexpired = areply.Status != EnrolleeVerificationCodeStatus.Expired
                        ? "--"
                        : Convert.ToDateTime(areply.DateExpired)
                            .ToString(CurrentRequestData.CultureInfo.DateTimeFormat.FullDateTimePattern);

                    responses.Showcall = areply.Status == EnrolleeVerificationCodeStatus.Authenticated &&
                                         areply.Pickedup == false;
                    responses.ShowEdit = (areply.AttendedTo && areply.PickedUpById == CurrentRequestData.CurrentUser.Id);
                    responses.ShowCallToUser = (areply.AttendedTo == false && areply.Pickedup &&
                                                areply.PickedUpById == CurrentRequestData.CurrentUser.Id);

                    response = responses;

                }

            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateEnrolleeFromPortal(FormCollection form)
        {

            var userguid = (string)Session["EnrolleeGuid"];

            if (!string.IsNullOrEmpty(userguid))
            {
                var state = form["state"];
                var lga = form["lga"];
                var address = form["address"];
                var occupation = form["occupation"];
                var phonenumber = form["mobilenumber"];
                var email = form["email"];
                var companystaffid = form["companystaffid"];
                var provider = form["provider"];
                var premed = form["premedicalcondition"];
                var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileP"];
                var enrollee = _enrolleeService.GetEnrolleeGuid(userguid);
                //do image work
                byte[] imgData = null;

                if (image != null && image.ContentLength > 0)
                {
                    Image image2 = Image.FromStream(image.InputStream);

                    //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                    var memoryStream = new MemoryStream();
                    image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imgData = memoryStream.ToArray();

                    //update passport

                    enrollee.EnrolleePassport.Imgraw = imgData;


                }


                if (!string.IsNullOrEmpty(state))
                {
                    enrollee.Stateid = Convert.ToInt32(state);

                }

                if (!string.IsNullOrEmpty(lga))
                {
                    enrollee.Lgaid = Convert.ToInt32(lga);

                }

                if (!string.IsNullOrEmpty(address))
                {
                    enrollee.Residentialaddress = address;

                }

                if (!string.IsNullOrEmpty(occupation))
                {
                    enrollee.Occupation = occupation;

                }
                if (!string.IsNullOrEmpty(email))
                {
                    enrollee.Emailaddress = email;

                }

                if (!string.IsNullOrEmpty(companystaffid))
                {

                    var staff = _companyService.Getstaff(enrollee.Staffprofileid);

                    if (staff != null)
                    {
                        staff.StaffId = companystaffid;
                        _companyService.UpdateStaff(staff);
                    }
                }


                if (!string.IsNullOrEmpty(provider))
                {
                    var providerid = 0;
                    if (int.TryParse(provider, out providerid))
                    {
                        enrollee.Primaryprovider = providerid;

                    }
                }

                _enrolleeService.UpdateEnrollee(enrollee);
                _pageMessageSvc.SetSuccessMessage("Profile have been updated sucessfully.");
                return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrollee.Guid.ToString());
            }
            else
            {
                _pageMessageSvc.SetErrormessage("You need to login before you can update.");
                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();
            }


        }

        public ActionResult UpdateDependantPortal(FormCollection form)
        {

            var userguid = (string)Session["EnrolleeGuid"];

            if (!string.IsNullOrEmpty(userguid))
            {

                var dependantid = form["dependantIDee"];
                var provider = form["providerDeee"];

                var image = CurrentRequestData.CurrentContext.Request.Files["photoInputFileED"];
                var enrolleeprin = _enrolleeService.GetEnrolleeGuid(userguid);


                var dependdants = _enrolleeService.GetDependentsEnrollee(enrolleeprin.Id);
                var dependant = dependdants.Where(x => x.Id == Convert.ToInt32(dependantid)).FirstOrDefault();
                //check if can update passport
                var tempdependant = new PendingDependant();
                if (dependant == null)
                {
                    tempdependant = _enrolleeService.getTempDependant(Convert.ToInt32(dependantid));

                }

                var path = Server.MapPath("~/Apps/Core/Content/Images/placeholder-photo.png");

                var imgData2 = System.IO.File.ReadAllBytes(path);
                var canpassport = false;

                if (dependant != null)
                {
                    if (dependant.EnrolleePassport.Imgraw.SequenceEqual(imgData2))
                    {
                        canpassport = true;
                    }


                    //do image work
                    byte[] imgData = null;

                    if (image != null && image.ContentLength > 0 && canpassport)
                    {
                        Image image2 = Image.FromStream(image.InputStream);

                        //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                        var memoryStream = new MemoryStream();
                        image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imgData = memoryStream.ToArray();

                        //update passport

                        dependant.EnrolleePassport.Imgraw = imgData;


                    }





                    if (!string.IsNullOrEmpty(provider))
                    {
                        var providerid = 0;
                        if (int.TryParse(provider, out providerid))
                        {
                            dependant.Primaryprovider = providerid;

                        }
                    }

                    _enrolleeService.UpdateEnrollee(dependant);

                }




                if (dependant == null && tempdependant != null && enrolleeprin.Guid.ToString().ToLower() == tempdependant.principalGuid.ToLower())
                {
                    //do image work
                    byte[] imgData = null;

                    if (image != null && image.ContentLength > 0)
                    {
                        Image image2 = Image.FromStream(image.InputStream);

                        //Image thumb = image2.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                        var memoryStream = new MemoryStream();
                        image2.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imgData = memoryStream.ToArray();

                        //update passport

                        tempdependant.ImgRaw = imgData;


                    }

                    if (!string.IsNullOrEmpty(provider))
                    {
                        var providerid = 0;
                        if (int.TryParse(provider, out providerid))
                        {
                            tempdependant.hospital = providerid;

                        }
                    }

                    _enrolleeService.updateTempDependant(tempdependant);
                }
                _pageMessageSvc.SetSuccessMessage("Dependant information updated sucessfully.");
                return Redirect(_uniquePageService.GetUniquePage<EnrolleePortalUserHomePage>().AbsoluteUrl + "?EnrolleeId=" + enrolleeprin.Guid.ToString());
            }

            else
            {
                _pageMessageSvc.SetErrormessage("You need to login before you can update.");
                return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();
            }




        }

        public JsonResult GetEnrolleeMedicalHistory(string enrolleeID, string Start, string End)
        {

            var id = -1;
            var startdate = CurrentRequestData.Now.AddDays(-365);
            var enddate = CurrentRequestData.Now;


            DateTime.TryParse(End, out enddate);
            if (!DateTime.TryParse(Start, out startdate) && !DateTime.TryParse(End, out enddate))
            {
                startdate = CurrentRequestData.Now.AddDays(-365);
                enddate = CurrentRequestData.Now;
            }


            if (!string.IsNullOrEmpty(enrolleeID) && int.TryParse(enrolleeID, out id))
            {
                var enrollee = _enrolleeService.GetEnrollee(id);

                if (enrollee != null)
                {
                    var rawresult = _claimService.GetClaimHistoryByPolicyNumber(enrollee.Policynumber, startdate, enddate).OrderByDescending(x => x.ENCOUNTERDATE);
                    var processedresult = new List<MedicalHistory>();
                    foreach (var item in rawresult)
                    {
                        var providerr = new Provider();
                        if (item.PROVIDERID > 0)
                        {
                            providerr = _providerSvc.GetProvider(item.PROVIDERID);

                        }
                        else
                        {
                            providerr = null;
                        }
                        var oob = new MedicalHistory();
                        oob.DateEncounter = Convert.ToDateTime(item.ENCOUNTERDATE).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                        oob.DateReceived = Convert.ToDateTime(item.DATERECEIVED).ToString(CurrentRequestData.CultureInfo.DateTimeFormat.LongDatePattern);
                        oob.EnrolleeName = item.CLIENTNAME;
                        oob.Diagnosis = item.DIAGNOSIS;
                        oob.providerName = item.PROVIDER;
                        oob.providerID = item.PROVIDERID.ToString();
                        oob.providerAddress = providerr != null ? providerr.Address.ToLower() : "";
                        oob.Tags = item.CLASS;
                        oob.InitialAmount = item.AMOUNTSUBMITTED.ToString("N");
                        oob.ProcessedAmount = item.AMOUNTPROCESSED.ToString("N");
                        processedresult.Add(oob);


                    }
                    return Json(processedresult, JsonRequestBehavior.AllowGet);
                }
            }
            var resp = new GenericReponse
            {
                Id = "-1",
                Name = "There was an Error"
            };
            return Json(resp, JsonRequestBehavior.AllowGet);
        }



        public JsonResult AuthenticateEnrolleePPortal(string providerid, string enrolleePolicynumber)
        {
            var ErrorResponse = new
            {
                rcode = "99",
                rmsg = "There was an error authenticating the enrollee,kindly contact Us."
            };

            var evstyyy = CurrentRequestData.CurrentContext.Request["evsvisittype"];
            var evsvisityrpe = evsvisittype.NewVisit;

            if (!string.IsNullOrEmpty(evstyyy))
            {
                evsvisityrpe = (evsvisittype)Enum.Parse(typeof(evsvisittype), evstyyy);

            }

            var providerID = 0;
            var veriCode = enrolleePolicynumber;
            if (!string.IsNullOrEmpty(providerid) && !string.IsNullOrEmpty(enrolleePolicynumber) && int.TryParse(providerid, out providerID))
            {
                var provider = _providerSvc.GetProvider(Convert.ToInt32(providerID));
                var enrolleemodel = new EnrolleeModel();
                var staffplann = "--";
                var canusehospitalPLAN = false;
                if (enrolleePolicynumber.Length > 8)
                {
                    //policy number
                    var enrolle = _enrolleeService.GetEnrolleeByPolicyNumber(enrolleePolicynumber.Trim());
                    var hassub = false;

                    if (enrolle != null)
                    {

                        var staff = _companyService.Getstaff(enrolle.Staffprofileid);
                        var staffplan = _companyService.GetCompanyPlan(staff.StaffPlanid);


                        if (staffplan != null)
                        {
                            staffplann = staffplan.Planfriendlyname.ToUpper();

                        }
                        var comp_sub = _companyService.checkifCompanyHasSubscription(Convert.ToInt32(staff.CompanyId));
                        var sub_sub =
                            _companyService.checkifSubsidiaryhasSubscrirption(Convert.ToInt32(staff.CompanySubsidiary));
                        //var validsub = comp_sub || sub_sub;

                        if (sub_sub || comp_sub)
                        {
                            var Ssubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid,
                                staff.CompanySubsidiary);

                            var csubscription = _companyService.GetSubscriptionByPlan(staff.StaffPlanid);

                            if (Ssubscription != null &&
                                (Convert.ToDateTime(Ssubscription.Expirationdate) > CurrentRequestData.Now))
                            {
                                hassub = true;
                            }


                            if (csubscription != null &&
                                (Convert.ToDateTime(csubscription.Expirationdate) > CurrentRequestData.Now))
                            {
                                hassub = true;
                            }
                            //model.SubscriptionExpirationDate = Ssubscription != null ? Convert.ToDateTime(Ssubscription.Expirationdate).ToShortDateString() : "NIL";
                            //model.HasSubscription = Ssubscription != null && Ssubscription.Expirationdate > CurrentRequestData.Now;

                        }




                        if (!hassub)
                        {

                            //return that the enrollee has no subscription
                            return Json(new { rcode = "99", rmsg = string.Format("There is an issue with the enrollee's subscription  ({0} - {1}) . Kindly contact Us.", enrolle.Policynumber, enrolle.Surname + " " + enrolle.Othernames) }, JsonRequestBehavior.AllowGet);

                        }

                        if (enrolle.Isexpundged)
                        {
                            //return that the enrollee has no subscription
                            return Json(new { rcode = "99", rmsg = string.Format("The enrollee with policynumber {0} requires authorization to access care in your facility . Kindly contact us.", enrolle.Policynumber) }, JsonRequestBehavior.AllowGet);
                        }

                        if (provider != null && staffplan != null && !string.IsNullOrEmpty(provider.Providerplans))
                        {

                            foreach (var item in provider.Providerplans.Split(','))
                            {
                                if (staffplan.Planid.ToString() == item)
                                {
                                    canusehospitalPLAN = true;
                                    break;
                                }
                            }



                            if (!string.IsNullOrEmpty(provider.CompanyConsession))
                            {
                                foreach (var item in provider.CompanyConsession.Split(','))
                                {
                                    if (staffplan.Companyid == Convert.ToInt32(item))
                                    {
                                        canusehospitalPLAN = true;
                                        break;
                                    }
                                }
                            }
                        }

                        //check if theres consessions


                        if (!canusehospitalPLAN)
                        {
                            //return that the enrollee has no subscription
                            return Json(new { rcode = "99", rmsg = string.Format("The enrollee with policynumber {0}  cannot access care at your facility because of the enrollee's plan. Kindly contact us.", enrolle.Policynumber) }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    if (enrolle != null || provider == null && hassub)
                    {
                        var verificationcode = _helperSvc.GenerateVerificationCode();
                        var verification2 = new EnrolleeVerificationCode();
                        verification2.EnrolleeId = enrolle.Id;
                        verification2.visittype = evsvisityrpe;
                        verification2.VerificationCode = verificationcode;
                        verification2.EncounterDate = CurrentRequestData.Now;
                        verification2.CreatedBy = 1;
                        verification2.Channel = (int)ChannelType.Web;
                        verification2.RequestPhoneNumber = "0";
                        verification2.Note = "Verification code was sent to enrollee for hospital access.";
                        verification2.Status = EnrolleeVerificationCodeStatus.Authenticated;
                        verification2.DateAuthenticated = CurrentRequestData.Now;
                        verification2.AuthChannel = (int)ChannelType.Web;
                        verification2.ProviderId = providerID;
                        verification2.Note = "Verification Code was authenticated. Generated By Provider";

                        _helperSvc.Addverification(verification2);

                        veriCode = verification2.VerificationCode;
                    }
                }
                veriCode = !string.IsNullOrEmpty(veriCode) ? veriCode : string.Empty;
                var verification = _helperSvc.GetverificationByVerificationCode(veriCode);


                if (!string.IsNullOrEmpty(veriCode) && verification != null)
                {

                    var enrollee = _enrolleeService.GetEnrollee(verification.EnrolleeId);
                    verification.Status = EnrolleeVerificationCodeStatus.Authenticated;
                    verification.DateAuthenticated = CurrentRequestData.Now;
                    verification.AuthChannel = (int)ChannelType.Web;
                    verification.ProviderId = providerID;
                    verification.Note = "The Verification code was authenticated.";


                    _helperSvc.Updateverification(verification);

                    return Json(new
                    {
                        Id = enrollee.Id.ToString(),
                        Fullname = enrollee.Surname + " " + enrollee.Othernames,
                        Gender = Enum.GetName(typeof(Sex), enrollee.Sex),
                        PolicyNumber = enrollee.Policynumber.ToUpper(),
                        Passport = Convert.ToBase64String(enrollee.EnrolleePassport.Imgraw),
                        Company = _companyService.Getsubsidiary(_companyService.Getstaff(enrollee.Staffprofileid).CompanySubsidiary)
                           .Subsidaryname.ToUpper(),
                        Code = veriCode,
                        plan = staffplann,
                        Provider = provider.Name,
                        rcode = "00",
                        rmsg = "Sucessful"
                    }, JsonRequestBehavior.AllowGet);
                }


                if (provider == null)
                {
                    return Json(new { rcode = "99", rmsg = "The Provider UPN is Invalid." }, JsonRequestBehavior.AllowGet);

                }


            }





            return Json(ErrorResponse, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BeneficiaryAddition(BeneficiaryAdditionPage page)
        {
            return View(page);
        }

        public JsonResult getAllpendingDependant()
        {
            var allpending = _enrolleeService.getalltempDependant();

            var output = new List<PendingDependantbox>();

            foreach (var item in allpending)
            {
                var newshi = new PendingDependantbox();

                var penrollee = _enrolleeService.GetEnrolleeGuid(item.principalGuid);

                if (penrollee != null)
                {
                    var staff = _companyService.Getstaff(penrollee.Staffprofileid);
                    var staffplan = staff != null ? _companyService.GetCompanyPlan(staff.StaffPlanid).Planfriendlyname.ToUpper() : "";
                    var provider = _providerSvc.GetProvider(item.hospital);


                    newshi.Id = item.Id;
                    newshi.principalpolicynumber = penrollee.Policynumber;
                    newshi.principalplan = staffplan;

                    newshi.DependantFullname = (item.lastname + " " + item.firstName).ToUpper();
                    newshi.dob = Convert.ToDateTime(item.dob).ToLongDateString();
                    newshi.Gender = Enum.GetName(typeof(Sex), item.sex);
                    newshi.noofdep = _enrolleeService.GetDependentsEnrollee(penrollee.Id).Count();
                    newshi.ImgRaw = Convert.ToBase64String(item.ImgRaw);
                    newshi.relationship = Enum.GetName(typeof(Relationship), item.relationship);
                    newshi.provider = provider != null ? provider.Name.ToUpper() : "--";
                    newshi.submitted = Convert.ToDateTime(item.CreatedOn).ToLongDateString();
                    output.Add(newshi);

                }
                else
                {
                    continue;
                }


            }
            output = output.OrderBy(x => x.principalpolicynumber).ToList<PendingDependantbox>();
            var response = Json(output, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                aaData = response.Data
            });
        }

        public ActionResult approvePendingDependant(int id)
        {
            var theshii = _enrolleeService.getTempDependant(id);

            if (theshii != null)
            {
                var enrollee = _enrolleeService.GetEnrolleeGuid(theshii.principalGuid);

                if (enrollee != null)
                {
                    var res = AddDependentAutomaticStaff(theshii.ImgRaw, theshii.relationship, "", theshii.lastname,
                  theshii.firstName, theshii.dob, theshii.sex, theshii.mobile,
                 theshii.hospital, theshii.preexisting, enrollee.Id, enrollee);

                    if (res)
                    {

                        theshii.Approved = true;
                        _enrolleeService.updateTempDependant(theshii);
                        _pageMessageSvc.SetSuccessMessage("Dependant Added Successfully.");


                        //send message to enrollee to inform the enrollee
                        if (!string.IsNullOrEmpty(enrollee.Emailaddress))
                        {
                            var emailmsg = new QueuedMessage();
                            emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                            emailmsg.ToAddress = enrollee.Emailaddress;
                            emailmsg.Subject = "Dependant Approved";
                            emailmsg.FromName = "Novo Health Africa";
                            emailmsg.Body = "Dear Enrollee,We are pleased to inform you that your dependant was approved.You can log into our portal to view.Thank you";

                            _emailSender.AddToQueue(emailmsg);
                        }


                        return _uniquePageService.RedirectTo<BeneficiaryAdditionPage>();
                    }

                }


            }
            _pageMessageSvc.SetErrormessage("There was an error approving dependant.");
            return _uniquePageService.RedirectTo<BeneficiaryAdditionPage>();
        }

        public ActionResult deletePendingDependant(int id)
        {
            var theshii = _enrolleeService.getTempDependant(id);

            if (theshii != null)
            {
                theshii.IsDeleted = true;
                _enrolleeService.updateTempDependant(theshii);
                _pageMessageSvc.SetSuccessMessage("Deleted successfully.");
                return _uniquePageService.RedirectTo<BeneficiaryAdditionPage>();
            }
            _pageMessageSvc.SetErrormessage("There was an error deleting dependant.");
            return _uniquePageService.RedirectTo<BeneficiaryAdditionPage>();

        }
        [HttpPost]
        public ActionResult RecoverPolicynumber(FormCollection form)
        {
            var emailladdress = form["emailaddresss"];
            if (!string.IsNullOrEmpty(emailladdress))
            {


                var principal = _enrolleeService.GetEnrolleesbyEmail(emailladdress).Where(x => x.Parentid == 0).SingleOrDefault();

                if (principal != null)
                {
                    var bodi = new StringBuilder();
                    bodi.AppendLine("Dear Enrollee");
                    bodi.AppendLine("You recently requested for your policy number.");
                    bodi.AppendLine(string.Format("Your policy number is {0}.", principal.Policynumber.ToUpper()));
                    bodi.AppendLine("Thank You");
                    var emailmsg = new QueuedMessage();
                    emailmsg.FromAddress = _mailSettings.SystemEmailAddress;
                    emailmsg.ToAddress = principal.Emailaddress;
                    emailmsg.Subject = "Policy Number Request";
                    emailmsg.FromName = "Novo Health Africa";
                    emailmsg.Body = bodi.ToString();

                    _emailSender.AddToQueue(emailmsg);
                    _pageMessageSvc.SetSuccessMessage("Your request was successful,");

                }
                else
                {
                    _pageMessageSvc.SetErrormessage("There is no enrollee with the email address.");

                }

            }
            else
            {
                _pageMessageSvc.SetErrormessage("There is no enrollee with the email address.");

            }

            return _uniquePageService.RedirectTo<EnrolleePortalLoginPage>();

        }

        public ActionResult Issues(IssuesPage page)
        {
            return View(page);
        }

        [HttpGet]
        public ActionResult restoreenrollee(int id)
        {
            var enrollee = _enrolleeService.GetEnrollee(id);
            if (enrollee != null)
            {
                if (enrollee.Parentid > 0)
                {
                    var parent = _enrolleeService.GetEnrollee(enrollee.Parentid);

                    if (parent != null && parent.Isexpundged == false)
                    {
                        enrollee.Isexpundged = false;
                        enrollee.Expungedby = 0;
                        enrollee.ExpungeNote = "";
                        enrollee.Dateexpunged = null;
                        _enrolleeService.UpdateEnrollee(enrollee);
                        _pageMessageSvc.SetSuccessMessage("Enrollee restored.");


                    }
                    else
                    {
                        _pageMessageSvc.SetErrormessage("There is a problem with the principal enrollee.");

                    }
                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Enrollee does not exist.");
            }

            return _uniquePageService.RedirectTo<EnrolleeListPage>();

        }

        [HttpGet]
        public ActionResult deleteenrollee(int id)
        {
            var enrollee = _enrolleeService.GetEnrollee(id);
            if (enrollee != null)
            {
                var issues = true;
                if (enrollee.Parentid == 0)
                {
                    var depen = _enrolleeService.GetDependentsEnrollee(enrollee.Id);

                    if (depen.Count() > 0)
                    {
                        _pageMessageSvc.SetErrormessage("You cannot delete a principal enrollee with dependants ,kindly delete the dependants first then try again.");
                        return _uniquePageService.RedirectTo<EnrolleeListPage>();
                    }
                    else
                    {
                        issues = false;

                    }
                }
                else
                {
                    issues = false;


                }

                if (!issues)
                {
                    enrollee.IsDeleted = true;
                    _enrolleeService.UpdateEnrollee(enrollee);
                    _pageMessageSvc.SetSuccessMessage("Enrollee deleted.");
                }
            }
            else
            {
                _pageMessageSvc.SetErrormessage("Enrollee does not exist.");
            }

            return _uniquePageService.RedirectTo<EnrolleeListPage>();

        }
    }
}