using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using MrCMS.Entities.People;
using MrCMS.Web.Apps.Core.Models.RegisterAndLogin;
using MrCMS.Web.Apps.Core.Models.Services;
using MrCMS.Web.Apps.Core.Utility;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Faqs.Models;
using System.Collections;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IClaimService
    {
        IList<IncomingClaims> QueryAllIncomingClaims(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int addedBy, string scrDeliveredBy, int month, int year, int transferedTo, bool useDate, DateTime scrFromDate, DateTime scrToDate, int otherFilters, int trackingid);
        bool ReceiveNewIncomingClaim(IncomingClaims claim);
        bool UpdateIncomingClaim(IncomingClaims Claim);
        bool DeleteIncomingClaim(IncomingClaims Claim);
        IncomingClaims GetIncomingClaim(int Id);
        IncomingClaims getincomingClaimByMonthandYear(int providerid, int month, int year);

        IList<TariffGenericReponse> GetServiceTariff(int Id, string phrase);
        IList<TariffGenericReponse> GetDrugTariff(int Id, string phrase);

        bool AddClaim(Claim claim);
        bool UpdateClaim(Claim claim);
        bool DeleteClaim(Claim claim);
        Claim GetClaim(int Id);
        bool CheckClaimByClientID(string Id);
        IList<IncomingClaims> QueryAllClaims(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int Provider, int enrolleeID, string enrolleePolicynumber, int Batch, int Year, int Month, string Diagnosis, int capturedBy, int vettedBy, int ClaimsTag, string otherfilter, int claimsstatus);

        bool AddClaimBatch(Entities.ClaimBatch batch);
        bool UpdateClaimBatch(Entities.ClaimBatch batch);
        bool DeleteClaimBatch(Entities.ClaimBatch batch);



        bool CleanClaim(Claim Claim);
        Entities.ClaimBatch GetClaimBatch(int Id);
        Entities.ClaimBatch GetBatchForProvider(int ProviderID, DateTime DateReceived);
        IList<Entities.ClaimBatch> QueryAllClaimBatch(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int month, int year, Utility.ClaimBatch Batch, string zone, int otherFilters, ClaimBatchStatus status, out int noofprovider, out decimal totalamountcaptured, out decimal totalproccessed, int remoteonly, int bacthid);



        //AuthorizationCode
        bool addAuthorization(AuthorizationCode authorizaction);
        bool updateAuthorization(AuthorizationCode authorization);
        AuthorizationCode getAuthorization(int id);
        AuthorizationCode getAuthorizationByCode(string Authorization);
        bool deleteAuthorization(AuthorizationCode authorization);
        IList<AuthorizationCode> QueryAuthorization(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, int scrProvider, int addedBy, int authorizedby, string Policynumber, int companyid, bool useDate, DateTime scrFromDate, DateTime scrToDate, int otherFilters, int opmode);

        IList<AuthorizationCode> getAuthorizationByPolicyNumber(string policynumber, DateTime startdate, DateTime enddate);
        IList<AuthorizationCode> getBirthAuthorization();


        long GetNumberofAdmission(DateTime start, DateTime End, ref Dictionary<int, int> theplan);
        IDictionary<string, int> GetutilizationReport(DateTime start, DateTime End);

        IDictionary<string, int> GetProviderutilizationReport(DateTime start, DateTime End);
        //vetting protocol

        bool addVettingPRotocol(VettingProtocol obj);
        IList<VettingProtocol> GetallVettingProtocol();
        VettingProtocol getVettingPRotocol(int id);
        bool deleteVettingProtocol(VettingProtocol vetprotocol);
        bool UpdateVettingProtocol(VettingProtocol vetprotocol);


        //the claimshistory

        bool addClaimHistory(ClaimHistory history);
        bool deleteClaimHistory(int id);
        ClaimHistory GetClaimHistory(int Id);
        IList<ClaimHistory> GetClaimHistoryByPolicyNumber(string policynumber, DateTime Start, DateTime end);
        int MaxClaimHistory();

        //claim payment batch
        bool addPaymentBatch(PaymentBatch paymentbatch);
        bool deletepaymentbatch(PaymentBatch paymentbatch);
        PaymentBatch getpaymentbatch(int id);
        IList<PaymentBatch> Queryallpaymentbatch(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string Title, bool usestatus, PaymentStatus paymentstatus, bool useDate, DateTime scrFromDate, DateTime scrToDate);

        IList<PaymentBatch> getrecentpaymentbatch();

        bool addAuthRequest(AuthorizationRequest request);
        bool deleteAuthRequest(AuthorizationRequest request);
        AuthorizationRequest GetAuthRequest(int Id);
        IList<AuthorizationRequest> QueryallAuthRequest(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string Search, bool useDate, DateTime scrFromDate, DateTime scrToDate);


        List<string> getAllDeletedClaimsForProvider(int providerid);
        IList<Entities.ClaimBatch> QueryAllClaimBatch(out int toltareccount, out int totalinresult, string empty, bool v1, bool v2, string sortColumnnumber, string sortOrder, int v3, int v4, int v5, Utility.ClaimBatch claimBatch, string zone, int v6, ClaimBatchStatus vetting, out int providercount, out decimal totalamount, out decimal totalprocessed, int channelint, int clambatchiddd);
    }
}