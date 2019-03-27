using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Logging;
using MrCMS.Web.Apps.Core.Entities;
using Elmah;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IHelperService
    {
        IEnumerable<Zone> GetallZones();
        IEnumerable<State> GetallStates();
        IEnumerable<Bank> Getallbanks();
        Bank Getbank(int id);
        IEnumerable<State> GetStatesinZone(int zoneid);
        State GetState(int id);
        long usersCount();
        IEnumerable<Lga> GetLgainstate(int stateId);
        Lga GetLgabyId(int id);
        Zone GetzonebyId(int id);
        string GenerateProvidersubCode(Lga lga);
        string GenerateCompanysubCode();
        string GenerateFourDigitCode();
        string GeneratesubscriptionCode();
        bool Log(LogEntryType type, Error error, string message, string detail);

        string GetDescription(Enum value);
        bool PushUserNotification(string destinationuser, string message, NotificationTarget target, string roles,
                                                NotificationType notificationType, string clickaction);

        bool AddNotificationTable(int notificationcode, string description, string roles, bool active);
        NotificationTable GetNotificationTable(int id);
        IEnumerable<NotificationTable> GetallNotificationTable();

        IList<string> GeneratePolicyNumber(int count, bool validate);
        string GenerateCCPolicyNumber();
        string GenerateVerificationCode();

        IList<EnrolleeVerificationCode> Getallverifications();

        EnrolleeVerificationCode Getverification(int id);
        EnrolleeVerificationCode GetverificationByVerificationCode(string verification);
        bool Addverification(EnrolleeVerificationCode verificationcode);
        bool Updateverification(EnrolleeVerificationCode verificationcode);


        int GetTotalNoofverification();
        int GetAllAuthenticatedVerification();
        int GetwithoutAuthentication();
        int GetMobileCount();
        int GetSmsCount();
        int GetUniqueProvidersAuntenticated();
        int GetUniquesEnrolleeGenerated();

        int GetTotalNoofverificationByDate(DateTime startdate, DateTime endDate);
        int GetAllAuthenticatedVerificationByDate(DateTime startdate, DateTime endDate);

        IList<ShortCodeMsg> QueryShortCodeMsgs(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string scrMobilenumber, string scrMessage, bool useDate, DateTime scrFromDate, DateTime scrToDate);

        bool AddDownloadFile(DownloadFile file);
        bool updateDownloadFile(DownloadFile file);
        bool DeleteDownloadedFile(DownloadFile fileid);
        DownloadFile getDownloadedFile(int fileid);
        IList<DownloadFile> QueryDownloadFiles(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string searchText);
        string GenerateAuthorizactionCode();




        bool addTask(TaskShit Task);
        bool deleteTask(TaskShit Task);
        TaskShit getTask(int Id);
        bool updateTask(TaskShit Task);
        IList<TaskShit> GetAllTask();


        bool addSponsor(ConnectCareSponsor sponsor);
        bool UpdateSponsor(ConnectCareSponsor sponsor);
        bool addBeneficiary(ConnectCareBeneficiary beneficiary);
        bool ValidateConenctCarePolicynumber(string policynumber);
        IList<ConnectCareSponsor> QueryConnectCareSponsor(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string fullname, string Policynumber, int senttomatontine, bool useDate, DateTime scrFromDate, DateTime scrToDate);
        IList<ConnectCareBeneficiary> GetBeneficiariesBySponsorID(int sponsorId);
        ConnectCareBeneficiary getBeneficiary(int Id);

        ConnectCareSponsor GetSponsor(int Id);


        bool addpaymentdetails(ConnectcarePaymentDetails payment);
        ConnectcarePaymentDetails getpaymentbyId(int Id);
        IList<ConnectcarePaymentDetails> getAllPaymentDetails();
        IList<ConnectcarePaymentDetails> getAllPaymentDetailsBySponsorID(int Id);
        bool checkpaymentuniqueID(string Id);

        IList<ConnectCareBeneficiary> getAllUnverifiedBeneficiary();


        bool addEmailDest(EmailDEST email);
        EmailDEST getEmailDestByCode(string code);

    }
}