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
    public interface ITariffService
    {
        IList<Tariff> GetallTariff();
        bool AddnewTariff(Tariff tariff);
        bool DeleteTariff(Tariff tariff);
        bool UpdateTarrif(Tariff tariff);
        Tariff GetTariff(int id);

        bool AddnewCategory(TariffCategory category);
        TariffCategory GetCategory(int id);
        bool DeleteCategory(TariffCategory category);
        IList<TariffCategory> GetallTariffCategory();

        bool AddnewDrugTariff(DrugTariff drug);
        DrugTariff GetDrug(int id);
        bool DeleteDrug(DrugTariff drug);
        IList<DrugTariff> Getalldrugtariff();


        IList<DrugTariff> GetalldrugtariffByCategory(int catId);
        IList<ServiceTariff> GetallservicetariffByCategory(int catId);
        IList<DrugTariff> QueryAllDrugTariff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, int groupid, string sortOrder, string scrProvider, string scrUsers, int otherFilters);
        bool UpdateDrug(DrugTariff drugtariff);
        bool ClearallDrugTariff(int TariffID);


        bool AddnewServiceTariff(ServiceTariff serviceTariff);
        ServiceTariff GetServiceTariff(int id);
        bool DeleteServiceTariff(ServiceTariff serviceTariff);
        IList<ServiceTariff> GetallServicetariff();
        IList<ServiceTariff> QueryAllServiceTariff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, int groupid, string sortOrder, string scrProvider, string scrUsers, int otherFilters);
        bool UpdateServiceTariff(ServiceTariff serviceTariff);
        bool ClearallServiceTariff(int TariffID);
    }
}