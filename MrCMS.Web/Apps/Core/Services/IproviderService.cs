using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Web.Apps.Core.Models.Provider;
using MrCMS.Web.Apps.Core.Utility;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IProviderService
    {
        IList<Provider> GetallProvider();
        IList<ProviderVm> GetallProviderforJson();

        int GetenrolleeusingproviderCount(int providerId);
        IList<Provider> GetallProviderByPlan(int planType);
        IList<Provider> GetallProviderByService(int serviceType);
        IList<ProviderVm> QueryallPendingProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate, DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype);
        IList<ProviderVm> QueryallProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate, DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype, int Zone, int ServiceType, int BoundByType, int category, bool delistshow);
        bool AddnewProvider(Provider provider);
        bool DeleteProvider(Provider provider);
        bool UpdateProvider(Provider provider);
        Provider GetProvider(int id);
        Provider GetProviderByName(string name);
        ProviderVm GetProviderVm(int id);
        long ProviderCount();
        IList<GenericReponse2> GetProviderNameList();
        IList<ProviderReponse> GetProviderNameWithAddressList();

        bool addproviderFeedBack(ProviderRating rating);
        IList<ProviderRating> GetProviderfeedbackList(int providerid);
        ProviderRating GetProviderfeedback(int feedbackid);
        bool SetAltProvider(int provider, int altprovider);

        IList<ProviderVm> QueryallDelistedProviderforJson(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, string sortOrder, string srcProviderName, string scrAddress, int state, int zone, bool useDate, DateTime scrFromDate, DateTime scrToDate, string scrUsers, int otherFilters, int plantype);

    }
}