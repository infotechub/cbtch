using System.Web.Mvc;
using MrCMS.Apps;
using Ninject;

namespace MrCMS.Web.Apps.Core
{
    public class CoreApp : MrCMSApp
    {
        public override string AppName
        {
            get { return "Core"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        protected override void RegisterServices(IKernel kernel)
        {
        }

        protected override void RegisterApp(MrCMSAppRegistrationContext context)
        {

            //map the hub controller
            context.MapRoute("CoreHub - get", "CoreHub/Get",
                new { controller = "UserNotification", action = "Get" });

            context.MapRoute("CoreHub - mark all", "CoreHub/MarkAllAsRead",
               new { controller = "UserNotification", action = "MarkAllAsRead" });

            context.MapRoute("CoreHub - create", "CoreHub/Create",
               new { controller = "UserNotification", action = "Create" });

            context.MapRoute("CoreHub - Count", "CoreHub/GetCount",
          new { controller = "UserNotification", action = "GetCount" });

            //routing Service
            context.MapRoute("ServiceControllerr - Edit", "ServiceService/Edit",
                new { controller = "ServicesPage", action = "Edit" });
            context.MapRoute("ServiceControllerr - Add", "ServiceService/Add",
               new { controller = "ServicesPage", action = "Add" });
            context.MapRoute("ServicesControllerr - Delete", "ServiceService/Delete",
            new { controller = "ServicesPage", action = "Delete" });
            context.MapRoute("ServicesControllerr -   GetJson", "ServiceService/GetJson",
            new { controller = "ServicesPage", action = "GetJson" });



            //routing the Plan pages
            context.MapRoute("PlanControllerr - Edit", "plan/Edit",
           new { controller = "PlanPage", action = "Edit" });
            context.MapRoute("PlanControllerr - Add", "Plan/Add",
               new { controller = "planPage", action = "Add" });

            context.MapRoute("PlanControllerr - Delete", "Plan/Delete",
            new { controller = "planPage", action = "Delete" });

            //

            //routing the Provider pages
            context.MapRoute("ProviderController - Edit", "Provider/Edit",
           new { controller = "ProviderPage", action = "Edit" });

            context.MapRoute("ProviderController - Add", "Provider/Add",
               new { controller = "ProviderPage", action = "Add" });

            context.MapRoute("ProviderController - Delete", "Provider/Delete",
            new { controller = "ProviderPage", action = "Delete" });

            context.MapRoute("ProviderController - getlgaJson", "Provider/GetLga/{id}",
            new { controller = "ProviderPage", action = "GetLga", id = UrlParameter.Optional });

            context.MapRoute("ProviderController - checkiffirsttime", "Provider/checkiffirsttime",
           new { controller = "ProviderPage", action = "checkiffirsttime" });




            context.MapRoute("ProviderController - ExportProvider", "Provider/ExportProvider",
                new { controller = "ProviderPage", action = "ExportProvider", id = UrlParameter.Optional });


            context.MapRoute("ProviderController - ExportProvider2", "Provider/ExportProvider2",
                new { controller = "ProviderPage", action = "ExportProvider2", id = UrlParameter.Optional });

            context.MapRoute("ProviderController -  GetJson", "Provider/GetJson",
            new { controller = "ProviderPage", action = "GetJson" });

            context.MapRoute("UserAccountController -  GetAlLusers", "Users/GetAlLusers",
                new { controller = "UserAccount", action = "GetAlLusers" });
            context.MapRoute("ProviderController - GetJsonDelisted", "Provider/GetJsonDelisted",
          new { controller = "ProviderPage", action = "GetJsonDelisted" });

            context.MapRoute("ProviderController  -GetProviderNamesJson", "Provider/GetProviderNamesJson",
            new { controller = "ProviderPage", action = "GetProviderNamesJson" });


            context.MapRoute("ClaimsController  -GetrecentPaymentbatchJson", "claim/GetrecentPaymentbatchJson",
            new { controller = "ClaimsPage", action = "GetrecentPaymentbatchJson" });
            context.MapRoute("ClaimsController  -GetClaimbatchdetails", "claim/GetClaimbatchdetails",
          new { controller = "ClaimsPage", action = "GetClaimbatchdetails" });



            context.MapRoute("ClaimsController  -ExportPaymentAnalysis", "claim/ExportPaymentAnalysis",
           new { controller = "ClaimsPage", action = "ExportPaymentAnalysis" });

            context.MapRoute("ClaimsController  -ExportMemoPaymentBatch", "claim/ExportMemoPaymentBatch",
         new { controller = "ClaimsPage", action = "ExportMemoPaymentBatch" });





            context.MapRoute("ClaimsController  -ExportPaymentBatchCSV", "claim/ExportPaymentBatchCSV",
        new { controller = "ClaimsPage", action = "ExportPaymentBatchCSV" });

            context.MapRoute("ClaimsController  -ImportPaymentMadeCSV", "claim/ImportPaymentMadeCSV",
       new { controller = "ClaimsPage", action = "ImportPaymentMadeCSV" });



            //   context.MapRoute("ClaimsController  -ExpandPaymentBatch", "claims/ExpandPaymentBatch",
            //new { controller = "ClaimsPage", action = "ExpandPaymentBatch" });




            context.MapRoute("ClaimsController  -QueryAuthRequestJson", "claim/QueryAuthRequestJson",
           new { controller = "ClaimsPage", action = "QueryAuthRequestJson" });


            context.MapRoute("ClaimsController  -removeItemPaymentBatch", "claim/removeItemPaymentBatch",
           new { controller = "ClaimsPage", action = "removeItemPaymentBatch" });





            context.MapRoute("ClaimsController  -QueryPaymentBatchJson", "claim/QueryPaymentBatchJson",
           new { controller = "ClaimsPage", action = "QueryPaymentBatchJson" });


            context.MapRoute("ProviderController  -updateProviderPassword", "providerportal/changepassword",
         new { controller = "ProviderPage", action = "updateProviderPassword" });

            context.MapRoute("ProviderController  -updateProviderProfile", "providerportal/updateprofile",
         new { controller = "ProviderPage", action = "updateProviderProfile" });



            context.MapRoute("ProviderController  -Storeforbk", "providerportal/Storeforbk",
         new { controller = "ProviderPage", action = "Storeforbk" });







            context.MapRoute("ProviderController -GetJsoAdvance", "Provider/GetJsoAdvance",
          new { controller = "ProviderPage", action = "GetJsoAdvance" });
            context.MapRoute("ProviderController - GetJsonPendingAdvance", "Provider/GetJsonPendingAdvance",
         new { controller = "ProviderPage", action = "GetJsonPendingAdvance" });
            context.MapRoute("ProviderController - GenerateVerificationCodeFromProvider", "Enrollee/GenerateVerificationCodeFromProvider",
     new { controller = "EnrolleePage", action = "GenerateVerificationCodeFromProvider" });
            context.MapRoute("EnrolleeController - UpdateDependantPortal", "eportal/UpdateDependantPortal",
     new { controller = "EnrolleePage", action = "UpdateDependantPortal" });

            context.MapRoute("EnrolleeController - GetEVSCodeDetails", "enrollee/GetEVSCodeDetails",
     new { controller = "EnrolleePage", action = "GetEVSCodeDetails" });


            context.MapRoute("EnrolleeController - restoreenrollee", "enrollee/restoreenrollee",
   new { controller = "EnrolleePage", action = "restoreenrollee" });


            context.MapRoute("EnrolleeController - deleteenrollee", "enrollee/deleteenrollee",
 new { controller = "EnrolleePage", action = "deleteenrollee" });




            context.MapRoute("EnrolleeController - QueryVerificationCode", "enrollee/QueryVerificationCode",
  new { controller = "EnrolleePage", action = "QueryVerificationCode" });


            context.MapRoute("ClaimsController - GetAllVettingProtocol", "Claims/GetAllVettingProtocol",
new { controller = "ClaimsPage", action = "GetAllVettingProtocol" });

            context.MapRoute("ClaimsController - providerGetClaimBatch", "Claims/providerGetClaimBatch",
          new { controller = "ClaimsPage", action = "providerGetClaimBatch" });


            context.MapRoute("ClaimsController - AddPaymentBatch", "Claims/AddPaymentBatch",
         new { controller = "ClaimsPage", action = "AddPaymentBatch" });

            context.MapRoute("ClaimsController - EditPaymentBatch", "Claims/EditPaymentBatch",
              new { controller = "ClaimsPage", action = "EditPaymentBatch" });


            context.MapRoute("ClaimsController - DeletePaymentBatch", "Claims/DeletePaymentBatch",
              new { controller = "ClaimsPage", action = "DeletePaymentBatch" });
            context.MapRoute("ClaimsController - MarkPaymentBatch", "Claims/MarkPaymentBatch",
             new { controller = "ClaimsPage", action = "MarkPaymentBatch" });


            context.MapRoute("ClaimsController - CloseClaimBatch", "Claims/CloseClaimBatch",
          new { controller = "ClaimsPage", action = "CloseClaimBatch" });
            context.MapRoute("ProviderController - Getmybackupfromserver", "Provider/Getmybackupfromserver",
         new { controller = "ProviderPage", action = "Getmybackupfromserver" });


            context.MapRoute("ProviderController - DeleteBk", "Provider/DeleteBk",
                   new { controller = "ProviderPage", action = "DeleteBk" });
            context.MapRoute("ProviderController - getalldeletedclaim", "claim/getalldeletedclaim",
        new { controller = "ClaimsPage", action = "getalldeletedclaim" });


            context.MapRoute("ProviderController - checkifkeyexist", "Provider/checkifkeyexist",
                   new { controller = "ProviderPage", action = "checkifkeyexist" });



            context.MapRoute("ClaimsController - Requestforauthcode", "Claims/Requestforauthcode",
          new { controller = "ClaimsPage", action = "Requestforauthcode" });


            context.MapRoute("ClaimsController - GetAllClaimBatchJson", "Claims/GetAllClaimBatchJson",
new { controller = "ClaimsPage", action = "GetAllClaimBatchJson" });


            context.MapRoute("ClaimsController - ExportBatchAnalysis", "Claims/ExportBatchAnalysis",
          new { controller = "ClaimsPage", action = "ExportBatchAnalysis" });



            context.MapRoute("ClaimsController - GetAllVettingProtocolforlist", "Claims/GetAllVettingProtocolforlist",
    new { controller = "ClaimsPage", action = "GetAllVettingProtocolforlist" });




            context.MapRoute("ClaimsController - UploadVetPRotocol", "Claims/UploadVetPRotocol",
new { controller = "ClaimsPage", action = "UploadVetPRotocol" });
            context.MapRoute("ClaimsController - EditvetProtocol", "Claims/EditvetProtocol",
            new { controller = "ClaimsPage", action = "EditvetProtocol" });

            context.MapRoute("ClaimsController - DeletevetProtocol", "Claims/DeletevetProtocol",
         new { controller = "ClaimsPage", action = "DeletevetProtocol" });


            context.MapRoute("ClaimsController - AddvetProtocol", "Claims/AddvetProtocol",
        new { controller = "ClaimsPage", action = "AddvetProtocol" });



            context.MapRoute("ProviderController - AddProviderService", "Provider/AddProviderService",
        new { controller = "ProviderPage", action = "AddProviderService" });


            context.MapRoute("EnrolleeController - SaveBeneficiary", "connectcare/SaveBeneficiary",
    new { controller = "EnrolleePage", action = "SaveBeneficiary" });

            context.MapRoute("EnrolleeController - updateBeneficiary", "connectcare/UpdateBeneficiary",
new { controller = "ConnectCarePage", action = "UpdateBeneficiary" });

            context.MapRoute("ConnectCareController - QueryConnectCareSponsors", "connectcare/QueryConnectCareSponsors",
           new { controller = "ConnectCarePage", action = "QueryConnectCareSponsors" });


            context.MapRoute("ConnectCareController - ReceivePayment", "connectcare/ReceivePayment",
           new { controller = "ConnectCarePage", action = "ReceivePayment" });



            context.MapRoute("ConnectCareController - QueryAllPayment", "connectcare/QueryAllPayment",
           new { controller = "ConnectCarePage", action = "QueryAllPayment" });


            context.MapRoute("ConnectCareController - GetAllPendingBeneficiary", "connectcare/GetAllPendingBeneficiary",
                     new { controller = "ConnectCarePage", action = "GetAllPendingBeneficiary" });



            context.MapRoute("TariffController - GetProviderServiceTariffJson", "Tariff/GetProviderServiceTariffJson",
new { controller = "TariffPage", action = "GetProviderServiceTariffJson" });


            context.MapRoute("TariffController - ApproveTarrif", "Tariff/ApproveTarrif",
new { controller = "TariffPage", action = "ApproveTarrif" });


            context.MapRoute("EnrolleeController - AddPendingDependant", "ePortal/AddPendingDependant",
new { controller = "EnrolleePage", action = "AddPendingDependant" });

            context.MapRoute("EnrolleeController - EnrolleePortalLogout", "ePortal/EnrolleePortalLogout",
new { controller = "EnrolleePage", action = "EnrolleePortalLogout" });

            context.MapRoute("EnrolleeController - RecoverPolicynumber", "ePortal/RecoverPolicynumber",
new { controller = "EnrolleePage", action = "RecoverPolicynumber" });



            context.MapRoute("EnrolleeController - UpdateEnrolleeFromPortal", "ePortal/UpdateEnrolleeFromPortal",
new { controller = "EnrolleePage", action = "UpdateEnrolleeFromPortal" });

            context.MapRoute("TariffController - GetProviderDrugTariffJson", "Tariff/GetProviderDrugTariffJson",
new { controller = "TariffPage", action = "GetProviderDrugTariffJson" });

            context.MapRoute("EnrolleeController - approvePendingDependant", "enrollee/approvePendingDependant",
new { controller = "EnrolleePage", action = "approvePendingDependant" });


            context.MapRoute("EnrolleeController - GetEnrolleeDetailsbyStaffId", "Portal/GetStaffDetailsbyStaffId",
new { controller = "EnrolleePage", action = "GetEnrolleeDetailsbyStaffId" });


            context.MapRoute("EnrolleeController - AuthenticateEnrolleePPortal", "Enrollee/AuthenticateEnrolleePPortal",
new { controller = "EnrolleePage", action = "AuthenticateEnrolleePPortal" });



            context.MapRoute("EnrolleeController - EnrolleePortalLogin", "ePortal/EnrolleePortalLogin",
new { controller = "EnrolleePage", action = "EnrolleePortalLogin" });
            context.MapRoute("EnrolleeController - GetEnrolleeMedicalHistory", "enrollee/GetEnrolleeMedicalHistory",
new { controller = "EnrolleePage", action = "GetEnrolleeMedicalHistory" });


            context.MapRoute("ProviderController -  GetJsonPending", "Provider/GetJsonPending",
         new { controller = "ProviderPage", action = "GetJsonPending" });
            context.MapRoute("ProviderController -  Details", "Provider/Details",
            new { controller = "ProviderPage", action = "Details" });
            context.MapRoute("ProviderController - Approve", "Provider/Approve",
           new { controller = "ProviderPage", action = "Approve" });
            context.MapRoute("ProviderController - dispprove", "Provider/Disapprove",
          new { controller = "ProviderPage", action = "Disapprove" });
            context.MapRoute("ProviderController - DoBulkApproval", "Provider/DoBulkApproval",
         new { controller = "ProviderPage", action = "DoBulkApproval" });

            context.MapRoute("ProviderController - delistProvider", "Provider/delistProvider",
       new { controller = "ProviderPage", action = "delistProvider" });



            //Tariff 

            context.MapRoute("TariffPageControllerr - Edit", "Tariff/Edit",
                new { controller = "TariffPage", action = "Edit" });
            context.MapRoute("TariffPageController - Add", "Tariff/Add",
               new { controller = "TariffPage", action = "Add" });
            context.MapRoute("TariffPageController - Delete", "Tariff/Delete",
            new { controller = "TariffPage", action = "Delete" });
            context.MapRoute("TariffPageController -   GetJson", "Tariff/GetJson",
            new { controller = "TariffPage", action = "GetJson" });
            context.MapRoute("TariffPageController -   EditContent", "Tariff/Content/{id}",
        new { controller = "TariffPage", action = "Content", id = UrlParameter.Optional });

            context.MapRoute("TariffPageController - addcategory", "Tariff/addcategory",
            new { controller = "TariffPage", action = "AddCategory" });

            //Drug Tariff
            context.MapRoute("TariffPageController -  GetCategoryJson", "Tariff/GetCategoryJson",
            new { controller = "TariffPage", action = "GetCategoryJson" });

            context.MapRoute("TariffPageController - GetDrugTariffJson", "Tariff/GetDrugTariffJson",
      new { controller = "TariffPage", action = "GetDrugTariffJson" });

            context.MapRoute("TariffPageController - AddDrug", "Tariff/AddDrug",
   new { controller = "TariffPage", action = "AddDrug" });

            context.MapRoute("TariffPageController - EditDrug", "Tariff/EditDrug",
new { controller = "TariffPage", action = "EditDrug" });
            context.MapRoute("TariffPageController - DeleteDrug", "Tariff/DeleteDrug",
new { controller = "TariffPage", action = "DeleteDrug" });

            //Service Tariff

            context.MapRoute("TariffPageController - GetServiceTariffJson", "Tariff/GetServiceTariffJson",
      new { controller = "TariffPage", action = "GetServiceTariffJson" });

            context.MapRoute("TariffPageController - AddService", "Tariff/AddService",
   new { controller = "TariffPage", action = "AddService" });

            context.MapRoute("TariffPageController - EditService", "Tariff/EditService",
new { controller = "TariffPage", action = "EditService" });


            context.MapRoute("ConnectCarePageController - TestConnect", "ConnectCare/TestService",
new { controller = "ConnectCarePage", action = "TestService" });


            context.MapRoute("TariffPageController - DeleteService", "Tariff/DeleteService",
new { controller = "TariffPage", action = "DeleteService" });


            context.MapRoute("TariffPageControllerr -UploadService", "Tariff/UploadService",
              new { controller = "TariffPage", action = "UploadService" });

            context.MapRoute("TariffPageControllerr -UploadDrug", "Tariff/UploadDrug",
           new { controller = "TariffPage", action = "UploadDrug" });
            //Company Route
            context.MapRoute("CompanyPageController - AddCompany", "company-list/Add",
new { controller = "CompanyPage", action = "Add" });


            context.MapRoute("CompanyPageController - importPlanBenefits", "company/importPlanBenefits",
new { controller = "CompanyPage", action = "importPlanBenefits" });


            context.MapRoute("CompanyPageController - BulkLoadStaff", "company/BulkLoadStaff/{id}",
new { controller = "CompanyPage", action = "BulkLoadStaff", id = UrlParameter.Optional });


            context.MapRoute("CompanyPageController - updateRenewal", "company/updateRenewal/{id}",
new { controller = "CompanyPage", action = "updateRenewal", id = UrlParameter.Optional });







            context.MapRoute("CompanyPageController - BulkExpungeStaff", "company/BulkExpungeStaff/{id}",
           new { controller = "CompanyPage", action = "BulkExpungeStaff", id = UrlParameter.Optional });


            context.MapRoute("CompanyPageController - DoExpungeUpload", "company/DoExpungeUpload",
new { controller = "CompanyPage", action = "DoExpungeUpload" });


            context.MapRoute("CompanyPageController - DoBulkUpload", "company/DoBulkUpload",
new { controller = "CompanyPage", action = "DoBulkUpload" });


            context.MapRoute("CompanyPageController -SaveCompanyRegBranch", "company/SaveCompanyRegBranch",
new { controller = "CompanyPage", action = "SaveCompanyRegBranch" });


            context.MapRoute("CompanyPageController -GetCompanyRegBranchList", "company/GetCompanyRegBranchList",
new { controller = "CompanyPage", action = "GetCompanyRegBranchList" });


            context.MapRoute("CompanyPageController -GetCompanyRegBranch", "company/GetCompanyRegBranch",
new { controller = "CompanyPage", action = "GetCompanyRegBranch" });


            context.MapRoute("TariffPageControllerr -ClearTariff", "Tariff/ClearTariff",
           new { controller = "TariffPage", action = "ClearTariff" });


            context.MapRoute("CompanyPageController - getJson", "company/GetJson",
new { controller = "CompanyPage", action = "GetJson" });

            context.MapRoute("CompanyPageController - AddCategory", "Company/AddCategory",
new { controller = "CompanyPage", action = "AddCategory" });


            context.MapRoute("CompanyPageController - GetCategoryJson", "Company/GetCategoryJson",
new { controller = "CompanyPage", action = "GetCategoryJson" });


            context.MapRoute("CompanyPageController - AddBenefit", "Company/AddBenefit",
new { controller = "CompanyPage", action = "AddBenefit" });


            context.MapRoute("CompanyPageController - GetBenefitJson", "Company/GetBenefitJson",
new { controller = "CompanyPage", action = "GetBenefitJson" });

            context.MapRoute("CompanyPageController -EditBenefit", "Company/EditBenefit",
new { controller = "CompanyPage", action = "EditBenefit" });

            context.MapRoute("CompanyPageController -DeleteBenefit", "Company/DeleteBenefit",
new { controller = "CompanyPage", action = "DeleteBenefit" });

            context.MapRoute("CompanyPageController -RestoreStaff", "Company/RestoreStaff",
new { controller = "CompanyPage", action = "RestoreStaff" });


            context.MapRoute("EnrolleePageController -GetEnrolleebyMobile", "Enrollee/GetEnrolleebyMobile",
new { controller = "EnrolleePage", action = "GetEnrolleebyMobile" });


            context.MapRoute("CompanyPageController -DeleteCategory", "Company/DeleteCategory",
new { controller = "CompanyPage", action = "DeleteCategory" });

            context.MapRoute("CompanyPageController -CompanyPlan", "Company/CompanyPlan",
new { controller = "CompanyPage", action = "CompanyPlan" });

            context.MapRoute("CompanyPageController -AddCompanyPlan", "Company-list/AddCompanyPlan",
new { controller = "CompanyPage", action = "AddCompanyPlan" });

            context.MapRoute("CompanyPageController - GetCompanyPlanJson", "Company/GetCompanyPlanJson",
new { controller = "CompanyPage", action = "GetCompanyPlanJson" });

            context.MapRoute("CompanyPageController -  GetCompanyBenefitforPlanJson", "Company/GetCompanyBenefitforPlanJson",
new { controller = "CompanyPage", action = "GetCompanyBenefitforPlanJson" });

            context.MapRoute("CompanyPageController -AddBenefitToPlan", "Company/AddBenefitToPlan",
new { controller = "CompanyPage", action = "AddBenefitToPlan" });

            context.MapRoute("CompanyPageController -RemoveBenefitToPlan", "Company/RemoveBenefitToPlan",
new { controller = "CompanyPage", action = "RemoveBenefitToPlan" });


            context.MapRoute("CompanyPageController -EditCompanyPlan", "Company/EditCompanyPlan",
new { controller = "CompanyPage", action = "EditCompanyPlan" });
            //Remove when it goes live
            context.MapRoute("CompanyPageController -RunCompanyplanGeneration", "Company/RunCompanyplanGeneration",
new { controller = "CompanyPage", action = "RunCompanyplanGeneration" });


            context.MapRoute("CompanyPageController -EditBenefitLimit", "Company/EditBenefitLimit",
new { controller = "CompanyPage", action = "EditBenefitLimit" });

            context.MapRoute("CompanyPageController -GetCompanyFreeBenefitforPlanJson", "Company/GetCompanyFreeBenefitforPlanJson",
new { controller = "CompanyPage", action = "GetCompanyFreeBenefitforPlanJson" });

            context.MapRoute("CompanyPageController -DeleteCompanyPlan", "Company/DeleteCompanyPlan",
new { controller = "CompanyPage", action = "DeleteCompanyPlan" });

            context.MapRoute("CompanyPageController -GetFreeBenefitforPlanJson", "Company/GetFreeBenefitforPlanJson",
new { controller = "CompanyPage", action = "GetFreeBenefitforPlanJson" });


            context.MapRoute("CompanyPageController - GetBenefitforPlanJson", "Company/GetBenefitforPlanJson",
new { controller = "CompanyPage", action = "GetBenefitforPlanJson" });

            context.MapRoute("CompanyPageController -AddDefaultBenefitToPlan", "Company/AddDefaultBenefitToPlan",
new { controller = "CompanyPage", action = "AddDefaultBenefitToPlan" });

            context.MapRoute("CompanyPageController -RemoveBenefitTfromPlan", "Company/RemoveBenefitTfromPlan",
new { controller = "CompanyPage", action = "RemoveBenefitTfromPlan" });

            context.MapRoute("CompanyPageController -EditPlanBenefitLimit", "Company/EditPlanBenefitLimit",
new { controller = "CompanyPage", action = "EditPlanBenefitLimit" });

            context.MapRoute("CompanyPageController -AddStaff", "Company/AddStaff",
new { controller = "CompanyPage", action = "AddStaff" });
            context.MapRoute("CompanyPageController -EditStaff", "Company/EditStaff",
new { controller = "CompanyPage", action = "EditStaff" });
            context.MapRoute("CompanyPageController -deleteStaff", "Company/DeleteStaff",
new { controller = "CompanyPage", action = "DeleteStaff" });

            context.MapRoute("CompanyPageController -ExpungeStaff", "Company/ExpungeStaff",
new { controller = "CompanyPage", action = "ExpungeStaff" });

            context.MapRoute("CompanyPageController -GetStaffListJson", "Company/GetStaffListJson",
new { controller = "CompanyPage", action = "GetStaffListJson" });

            context.MapRoute("CompanyPageController -GetNewStaffListJson", "Company/GetNewStaffListJson",
new { controller = "CompanyPage", action = "GetNewStaffListJson" });

            context.MapRoute("CompanyPageController - GetPlans", "Company/GetPlans/{id}",
            new { controller = "CompanyPage", action = "GetPlans", id = UrlParameter.Optional });



            context.MapRoute("EnrolleePageController - EnrolleeReg", "Portal/{Companyid}",
            new { controller = "EnrolleePage", action = "Portal", Companyid = UrlParameter.Optional });



            context.MapRoute("CompanyPageController -  GetPlanswithoutsubscription", "Company/GetPlanswithoutsubscription/{id}",
            new { controller = "CompanyPage", action = "GetPlanswithoutsubscription", id = UrlParameter.Optional });


            context.MapRoute("CompanyPageController -AddSubscription", "Company/AddSubscription",
new { controller = "CompanyPage", action = "AddSubscription" });

            context.MapRoute("CompanyPageController -GetSubscriptionsJson", "Company/GetSubscriptionsJson",
new { controller = "CompanyPage", action = "GetSubscriptionsJson" });

            context.MapRoute("CompanyPageController -GetSubscriptionssmmaryJson", "Company/GetSubscriptionssmmaryJson",
new { controller = "CompanyPage", action = "GetSubscriptionssmmaryJson" });
            context.MapRoute("CompanyPageController -GetSubscriptionsTop5ExpiringJson", "Company/GetSubscriptionsTop5ExpiringJson",
new { controller = "CompanyPage", action = "GetSubscriptionsTop5ExpiringJson" });
            context.MapRoute("CompanyPageController -EditSubscription", "Company/EditSubscription",
new { controller = "CompanyPage", action = "EditSubscription" });

            context.MapRoute("CompanyPageController - DoMassExpunge", "Company/DoMassExpunge",
new { controller = "CompanyPage", action = "DoMassExpunge" });


            context.MapRoute("CompanyPageController -DoRemoveExpunge", "Company/DoRemoveExpunge",
new { controller = "CompanyPage", action = "DoRemoveExpunge" });
            context.MapRoute("CompanyPageController -DeleteSubscription", "Company/DeleteSubscription",
new { controller = "CompanyPage", action = "DeleteSubscription" });

            context.MapRoute("CompanyPageController -GetAutomaticExpungeStaffListJson", "Company/GetAutomaticExpungeStaffListJson",
new { controller = "CompanyPage", action = "GetAutomaticExpungeStaffListJson" });

            context.MapRoute("CompanyPageController - ExecuteBulkStaffTask", "Company/ExecuteBulkStaffTask",
new { controller = "CompanyPage", action = "ExecuteBulkStaffTask" });

            context.MapRoute("CompanyPageController - GetBulkJobsJson", "Company/GetBulkJobsJson",
new { controller = "CompanyPage", action = "GetBulkJobsJson" });


            context.MapRoute("CompanyPageController -TerminateSubscription", "Company/TerminateSubscription",
new { controller = "CompanyPage", action = "TerminateSubscription" });

            context.MapRoute("CompanyPageController - QueryCompanyPlanJson", "Company/QueryCompanyPlanJson",
new { controller = "CompanyPage", action = "QueryCompanyPlanJson" });


            context.MapRoute("CompanyPageController - SubscriptionDetails", "Company/SubscriptionDetails",
new { controller = "CompanyPage", action = "SubscriptionDetails" });

            context.MapRoute("CompanyPageController - GetpendingSubscriptionsJson", "Company/GetpendingSubscriptionsJson",
new { controller = "CompanyPage", action = "GetpendingSubscriptionsJson" });
            context.MapRoute("CompanyPageController -ApproveSubscription", "Company/ApproveSubscription",
new { controller = "CompanyPage", action = "ApproveSubscription" });

            context.MapRoute("CompanyPageController -DisapproveSubscription", "Company/DisapproveSubscription",
new { controller = "CompanyPage", action = "DisapproveSubscription" });
            context.MapRoute("CompanyPageController -EditCompany", "Company/Edit",
      new { controller = "CompanyPage", action = "Edit" });
            context.MapRoute("CompanyPageController -DeleteCompany", "Company/Delete",
new { controller = "CompanyPage", action = "Delete" });

            context.MapRoute("EnrolleeController -GeneratePolicyNumberJson", "Enrollee/GeneratePolicyNumberJson/{count}",
new { controller = "EnrolleePage", action = "GeneratePolicyNumberJson", count = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -GetState", "Enrollee/GetStateInZone",
                new { controller = "EnrolleePage", action = "GetStatebyZone", id = UrlParameter.Optional });



            context.MapRoute("EnrolleeController -Add", "Enrollee/Add",
new { controller = "EnrolleePage", action = "Add" });

            context.MapRoute("EnrolleeController   SubmitPortalRegistration", "Enrollee/SubmitPortalRegistration",
new { controller = "EnrolleePage", action = "SubmitPortalRegistration" });


            context.MapRoute("EnrolleeController -AddDependent", "Enrollee/AddDependent",
new { controller = "EnrolleePage", action = "AddDependent" });

            context.MapRoute("EnrolleeController -UpdateEnrollee", "Enrollee/UpdateEnrollee",
new { controller = "EnrolleePage", action = "UpdateEnrollee" });

            context.MapRoute("EnrolleeController -GetDependentsJson", "Enrollee/GetDependentsJson/{id}",
                             new { controller = "EnrolleePage", action = "GetDependentsJson", id = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -EditDependent", "Enrollee/EditDependent",
new { controller = "EnrolleePage", action = "EditDependent" });



            context.MapRoute("EnrolleeController -gettempEnrollee", "Enrollee/gettempEnrollee",
new { controller = "EnrolleePage", action = "gettempEnrollee" });

            context.MapRoute("EnrolleeController -deletePendingDependant", "Enrollee/deletePendingDependant",
new { controller = "EnrolleePage", action = "deletePendingDependant" });



            context.MapRoute("EnrolleeController -AutomaticStaffProfile", "Enrollee/AutomaticStaffProfile",
new { controller = "EnrolleePage", action = "AutomaticStaffProfile" });



            context.MapRoute("EnrolleeController - getstafffbyCompanystaffid", "Enrollee/getstafffbyCompanystaffid",
new { controller = "EnrolleePage", action = "getstafffbyCompanystaffid" });


            context.MapRoute("EnrolleeController -ExpungeDependent", "Enrollee/ExpungeDependent",
new { controller = "EnrolleePage", action = "ExpungeDependent" });
            context.MapRoute("CompanyPageController - GetJsonSubsidiary", "Company/GetJsonSubsidiary/{id}",
         new { controller = "CompanyPage", action = "GetJsonSubsidiary", id = UrlParameter.Optional });


            context.MapRoute("EnrolleeController -GetShortMessageJson", "Enrollee/GetShortMessageJson",
new { controller = "EnrolleePage", action = "GetShortMessageJson" });


            context.MapRoute("EnrolleeController - RunUploadEnrolleesCapitation", "Enrollee/RunUploadEnrolleesCapitation",
new { controller = "EnrolleePage", action = "RunUploadEnrolleesCapitation" });


            context.MapRoute("CompanyPageController -AddSubsidiary", "Company/AddSubsidiary",
         new { controller = "CompanyPage", action = "AddSubsidiary" });

            context.MapRoute("CompanyPageController -AddStaffBulk", "Company/AddStaffBulk",
         new { controller = "CompanyPage", action = "AddStaffBulk" });



            context.MapRoute("CompanyPageController -EditSubsidiary", "Company/EditSubsidiary",
         new { controller = "CompanyPage", action = "EditSubsidiary" });


            context.MapRoute("CompanyPageController -GetSubsidiary", "Company/GetSubsidiary/{id}",
         new { controller = "CompanyPage", action = "GetSubsidiary", id = UrlParameter.Optional });

            context.MapRoute("CompanyPageController -DeleteSubsidiary", "Company/DeleteSubsidiary",
   new { controller = "CompanyPage", action = "DeleteSubsidiary" });


            context.MapRoute("CompanyPageController -GetallStaffinCompanyJson", "Company/GetallStaffinCompanyJson",
   new { controller = "CompanyPage", action = "GetallStaffinCompanyJson" });






            context.MapRoute("EnrolleeController - GetEnrolleesJson", "Enrollee/GetEnrolleesJson",
   new { controller = "EnrolleePage", action = "GetEnrolleesJson" });


            context.MapRoute("EnrolleeController -   AuthenticateVerificationCodeNew", "Enrollee/AuthenticateVerificationCodeNew",
   new { controller = "EnrolleePage", action = "AuthenticateVerificationCodeNew" });


            context.MapRoute("EnrolleeController - PostProviderAuthenticate", "Enrollee/PostProviderAuthenticate",
   new { controller = "EnrolleePage", action = "PostProviderAuthenticate" });

            context.MapRoute("EnrolleeController -   GetProviderDetails", "Enrollee/GetProviderDetails",
     new { controller = "EnrolleePage", action = "GetProviderDetails" });

            context.MapRoute("EnrolleeController -EnrolleeRegHome", "Enrollee/EnrolleeRegHome",
           new { controller = "EnrolleePage", action = "EnrolleeRegHome" });

            context.MapRoute("CompanyController -GetallStaffinCompanyLiteJson", "Company/GetallStaffinCompanyLiteJson",
  new { controller = "CompanyPage", action = "GetallStaffinCompanyLiteJson" });





            context.MapRoute("EnrolleeController - GenerateVerificationCode", "Enrollee/GenerateVerificationCode",
        new { controller = "EnrolleePage", action = "GenerateVerificationCode" });

            context.MapRoute("EnrolleeController -     GetVerificationsJson", "Enrollee/GetVerificationsJson",
        new { controller = "EnrolleePage", action = "GetVerificationsJson" });

            context.MapRoute("EnrolleeController -     ExportTable", "Enrollee/ExportTable",
        new { controller = "EnrolleePage", action = "ExportTable" });


            context.MapRoute("CompanyController -     DoStaffLinking", "Company/DoStaffLinking",
  new { controller = "CompanyPage", action = "DoStaffLinking" });


            context.MapRoute("EnrolleeController -returnEnrolleeNumbers", "Enrollee/returnEnrolleeNumbers",
        new { controller = "EnrolleePage", action = "returnEnrolleeNumbers" });


            context.MapRoute("EnrolleeController - DeleteBulkEnrollee", "Enrollee/DeleteBulkEnrollee",
          new { controller = "EnrolleePage", action = "DeleteBulkEnrollee" });

            context.MapRoute("EnrolleeController - GenerateTempVerificationCode", "Enrollee/GenerateTempVerificationCode",
          new { controller = "EnrolleePage", action = "GenerateTempVerificationCode" });

            context.MapRoute("EnrolleeController -     ", "Enrollee/GetSmsJson/{id}",
                  new { controller = "EnrolleePage", action = "GetSmsJson", id = UrlParameter.Optional });

            context.MapRoute("EnrolleeController - SMSCONFIG", "Enrollee/SmSConfig",
                  new { controller = "EnrolleePage", action = "SmSConfig" });

            context.MapRoute("EnrolleeController - GetEnrolleePolicyNumberName", "Enrollee/GetEnrolleePolicyNumberName",
                 new { controller = "EnrolleePage", action = "GetEnrolleePolicyNumberName" });

            context.MapRoute("EnrolleeController -QuickSMS", "Enrollee/QuickSms",
                       new { controller = "EnrolleePage", action = "QuickSms" });
            context.MapRoute("EnrolleeController -RunUploadEnrollees", "Enrollee/RunUploadEnrollees",
                            new { controller = "EnrolleePage", action = "RunUploadEnrollees" });

            context.MapRoute("EnrolleeController - enrolleePoliceNum", "Enrollee/GenerateVerificationCodeJson/{enrolleePoliceNum}/{purpose}",
                   new { controller = "EnrolleePage", action = "GenerateVerificationCodeJson", enrolleePoliceNum = UrlParameter.Optional, purpose = UrlParameter.Optional });
            context.MapRoute("EnrolleeController -AuthenticateVerificationCode",
                             "Enrollee/AuthenticateVerificationCode/{verificationCode}/{providerId}/",
                             new
                             {
                                 controller = "EnrolleePage",
                                 action = "AuthenticateVerificationCode",
                                 verificationCode = UrlParameter.Optional,
                                 providerId = UrlParameter.Optional
                             });



            context.MapRoute("EnrolleeController -SetupMobileApp", "Enrollee/SetupMobileApp/{policyNumber}/{mobileNumber}/{email}",
                         new { controller = "EnrolleePage", action = "SetupMobileApp", policyNumber = UrlParameter.Optional, mobileNumber = UrlParameter.Optional, email = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -GetEnrolleeDetailsClaim", "Enrollee/GetEnrolleeDetailsClaim/{policyNumber}",
             new { controller = "EnrolleePage", action = "GetEnrolleeDetailsClaim", policyNumber = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -MobileGetProfile", "Enrollee/MobileGetProfile/{policyNumber}",
                          new { controller = "EnrolleePage", action = "MobileGetProfile", policyNumber = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -GetEnrolleesTiedToPhone", "Enrollee/GetOtherPolicyNumbersTiedToNumber/{mobileNumber}",
              new { controller = "EnrolleePage", action = "GetOtherPolicyNumbersTiedToNumber", mobileNumber = UrlParameter.Optional });
            context.MapRoute("EnrolleeController -GetPurposeOfVisit", "Enrollee/GetPurposeOfVisit",
              new { controller = "EnrolleePage", action = "GetPurposeOfVisit" });
            context.MapRoute("EnrolleeController -DoExportIDCARD", "Enrollee/DoExportIdcard",
           new { controller = "EnrolleePage", action = "DoExportIdcard" });
            context.MapRoute("EnrolleeController -PostSms", "Enrollee/PostSms/{number}/{message}",
               new { controller = "EnrolleePage", action = "PostSms", number = UrlParameter.Optional, message = UrlParameter.Optional });

            context.MapRoute("EnrolleeController -PostSms2", "Enrollee/PostSms2/{number}/{message}",
               new { controller = "EnrolleePage", action = "PostSms2", number = UrlParameter.Optional, message = UrlParameter.Optional });


            context.MapRoute("EnrolleeController -AttendToVerification", "Enrollee/AttendToVerification/{id}",
  new { controller = "EnrolleePage", action = "AttendToVerification", id = UrlParameter.Optional });
            context.MapRoute("EnrolleeController - RunUploadCustomEnrollees", "Enrollee/RunUploadCustomEnrollees",
new { controller = "EnrolleePage", action = "RunUploadCustomEnrollees" });


            context.MapRoute("EnrolleeController -VerificationDetails", "Enrollee/VerificationDetails/{id}",
  new { controller = "EnrolleePage", action = "VerificationDetails", id = UrlParameter.Optional });
            //MrCMS Files 
            context.MapRoute("External Login", "external-login", new { controller = "ExternalLogin", action = "Login" });
            context.MapRoute("External Login Callback", "external-login/callback",
                new { controller = "ExternalLogin", action = "Callback" });

            context.MapRoute("User Registration", "Registration/RegistrationDetails",
                new { controller = "Registration", action = "RegistrationDetails" });
            context.MapRoute("User Registration - check email", "Registration/CheckEmailIsNotRegistered",
                new { controller = "Registration", action = "CheckEmailIsNotRegistered" });

            context.MapRoute("UserAccountController - account details", "UserAccount/UserAccountDetails",
                new { controller = "UserAccount", action = "UserAccountDetails" });
            context.MapRoute("UserAccountController - check email isn't already registered", "UserAccount/IsUniqueEmail",
                new { controller = "UserAccount", action = "IsUniqueEmail" });

            context.MapRoute("UserAccountController - change password", "UserAccount/ChangePassword",
                new { controller = "UserAccount", action = "ChangePassword" });

            context.MapRoute("ClaimsController - ReceiveNewClaim", "ClaimsPage/IncomingClaims",
            new { controller = "ClaimsPage", action = "IncomingClaims" });
            context.MapRoute("ClaimsController - RejectToCapture", "Claim/RejectToCapture",
          new { controller = "ClaimsPage", action = "RejectToCapture" });


            context.MapRoute("ClaimsController - ExportPaymentAdvice", "ClaimsPage/ExportPaymentAdvice",
            new { controller = "ClaimsPage", action = "ExportPaymentAdvice" });


            context.MapRoute("ClaimsController - SubmitClaimsForm", "ClaimsPage/SubmitClaimsForm",
            new { controller = "ClaimsPage", action = "SubmitClaimsForm" });

            context.MapRoute("ClaimsController - SubmitClaimsForm2", "ClaimsPage/SubmitClaimsForm2",
      new { controller = "ClaimsPage", action = "SubmitClaimsForm2" });

            context.MapRoute("ClaimsController -GetIncomingClaimsJson", "ClaimsPage/GetIncomingClaimsJson",
            new { controller = "ClaimsPage", action = "GetIncomingClaimsJson" });
            context.MapRoute("ClaimsController -GetClaimBatchAwaitingPaymentJson", "ClaimsPage/GetClaimBatchAwaitingPaymentJson",
                       new { controller = "ClaimsPage", action = "GetClaimBatchAwaitingPaymentJson" });


            context.MapRoute("ClaimsController -VerifyAuthorizationCode", "Claims/VerifyAuthorizationCode",
                   new { controller = "ClaimsPage", action = "VerifyAuthorizationCode" });


            context.MapRoute("ProviderController -SubmitFeedBack", "Provider/SubmitFeedBack",
         new { controller = "ProviderPage", action = "SubmitFeedBack" });
            context.MapRoute("ProviderController -LogoutfromproviderPortal", "Provider/LogoutfromproviderPortal",
        new { controller = "ProviderPage", action = "LogoutfromproviderPortal" });




            context.MapRoute("ClaimsController -DeleteIncomingClaim", "ClaimsPage/DeleteIncomingClaim",
         new { controller = "ClaimsPage", action = "DeleteIncomingClaim" });


            context.MapRoute("ClaimsController -GetServiceTariffJson", "ClaimsPage/GetServiceTariffJson",
         new { controller = "ClaimsPage", action = "GetServiceTariffJson" });

            context.MapRoute("ClaimsController -GetDrugTariffJson", "ClaimsPage/GetDrugTariffJson",
      new { controller = "ClaimsPage", action = "GetDrugTariffJson" });

            context.MapRoute("ClaimsController - GetClaimBatchJson", "ClaimsPage/GetClaimBatchJson",
    new { controller = "ClaimsPage", action = "GetClaimBatchJson" });

            context.MapRoute("ClaimsController - GetClaimBatchVetJson", "ClaimsPage/GetClaimBatchVetJson",
new { controller = "ClaimsPage", action = "GetClaimBatchVetJson" });


            context.MapRoute("ClaimsController - GetClaimBatchReviewJson", "ClaimsPage/GetClaimBatchReviewJson",
new { controller = "ClaimsPage", action = "GetClaimBatchReviewJson" });

            context.MapRoute("ClaimsController - GetAuthorizationCodes", "ClaimsPage/GetAuthorizationCodes",
new { controller = "ClaimsPage", action = "GetAuthorizationCodes" });


            context.MapRoute("ClaimsController -ExpandClaim", "ClaimsPage/ExpandClaim",
new { controller = "ClaimsPage", action = "ExpandClaim", id = UrlParameter.Optional });


            context.MapRoute("ClaimsController -DeleteClaim", "ClaimsPage/DeleteClaim/{id}",
    new { controller = "ClaimsPage", action = "DeleteClaim", id = UrlParameter.Optional });

            context.MapRoute("ClaimsController -SubmitClaimVet", "ClaimsPage/SubmitClaimVet/{id}",
    new { controller = "ClaimsPage", action = "SubmitClaimVet", id = UrlParameter.Optional });

            context.MapRoute("ClaimsController -SubmitClaimReview", "ClaimsPage/SubmitClaimReview/{id}",
new { controller = "ClaimsPage", action = "SubmitClaimReview", id = UrlParameter.Optional });



            context.MapRoute("ClaimsController -EditClaim", "ClaimsPage/EditClaimForm/{id}",
 new { controller = "ClaimsPage", action = "EditClaimForm", id = UrlParameter.Optional });

            context.MapRoute("ClaimsController -VetSingleClaim", "ClaimsPage/VetSingleClaim",
new { controller = "ClaimsPage", action = "VetSingleClaim", id = UrlParameter.Optional });


            context.MapRoute("ClaimsController -DoApproveReview", "ClaimsPage/DoApproveReview",
new { controller = "ClaimsPage", action = "DoApproveReview", id = UrlParameter.Optional });


            context.MapRoute("ClaimsController - RejectToRevet", "ClaimsPage/RejectToRevet",
new { controller = "ClaimsPage", action = "RejectToRevet", id = UrlParameter.Optional });


            context.MapRoute("ClaimsController - GetClaimBatchReviewedJson", "ClaimsPage/GetClaimBatchReviewedJson",
new { controller = "ClaimsPage", action = "GetClaimBatchReviewedJson" });

            context.MapRoute("TaskController -ExecuteTask", "Task/RunTasks",
                new { controller = "Task", action = "RunTask" });

            context.MapRoute("ClaimsController -ExportMemo", "ClaimsPage/ExportMemo",
new { controller = "ClaimsPage", action = "ExportMemo" });


            context.MapRoute("ClaimsController -returntoreview", "claims/returntoreview",
new { controller = "ClaimsPage", action = "returntoreview" });

            context.MapRoute("ClaimsController - DoApproveForPayment", "ClaimsPage/DoApproveForPayment",
new { controller = "ClaimsPage", action = "DoApproveForPayment" });

            context.MapRoute("ClaimsController - GetincomingClaimforEdit", "ClaimsPage/GetincomingClaimforEdit",
new { controller = "ClaimsPage", action = "GetincomingClaimforEdit" });

            context.MapRoute("ClaimsController - UpdateAuthorizationCode", "ClaimsPage/UpdateAuthorizationCode",
            new { controller = "ClaimsPage", action = "UpdateAuthorizationCode" });


            context.MapRoute("downloadController -GetDownloadFilesJson", "DownloadPage/GetDownloadFilesJson",
new { controller = "DownloadFilePage", action = "GetDownloadFilesJson" });


            context.MapRoute("ClaimsController -GenerateAuthorizationCode", "ClaimsPage/GenerateAuthorizationCode",
new { controller = "ClaimsPage", action = "GenerateAuthorizationCode", id = UrlParameter.Optional });
            context.MapRoute("ClaimsController -GenerateAuthorizationCode2", "ClaimsPage/GenerateAuthorizationCode2",
                new { controller = "ClaimsPage", action = "GenerateAuthorizationCode2", id = UrlParameter.Optional });

            context.MapRoute("ClaimsController -EditAuthorizationCode", "ClaimsPage/EditAuthorizationCode",
new { controller = "ClaimsPage", action = "EditAuthorizationCode", id = UrlParameter.Optional });
            context.MapRoute("ClaimsController -DeleteAuthorizationCode", "ClaimsPage/DeleteAuthorizationCode",
new { controller = "ClaimsPage", action = "DeleteAuthorizationCode", id = UrlParameter.Optional });

            context.MapRoute("ClaimsController -UploadClaimHistory", "ClaimsPage/UploadClaimHistory",
new { controller = "ClaimsPage", action = "UploadClaimHistory" });

            context.MapRoute("CompanyController -ApproveUpload", "Company/ApproveUpload",
new { controller = "CompanyPage", action = "ApproveUpload" });

            context.MapRoute("EnrolleeController - ProviderPortalLogin", "provider/ProviderPortalLogin",
new { controller = "EnrolleePage", action = "ProviderPortalLogin" });

            context.MapRoute("EnrolleeController -getAllpendingDependant", "Enrollee/getAllpendingDependant",
new { controller = "EnrolleePage", action = "getAllpendingDependant" });

        }
    }
}