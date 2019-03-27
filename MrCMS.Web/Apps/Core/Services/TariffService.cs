using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MrCMS.Logging;
using MrCMS.Web.Apps.Core.Entities;
using MrCMS.Website;
using NHibernate;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Core.Services;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Core.Services
{
    public class TariffService : ITariffService
    {
        private readonly ISession _session;
        private readonly IHelperService _helper;
        public TariffService(ISession session, IHelperService helper)
        {
            _session = session;
            _helper = helper;
        }
        public IList<Tariff> GetallTariff()
        {
            return _session.QueryOver<Tariff>().Where(x => x.IsDeleted == false).List<Tariff>();
        }

        public bool AddnewTariff(Tariff tariff)
        {
            if (tariff != null)
            {
                _session.Transact(session => session.Save(tariff));
                _helper.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "New Tarrif has been added to the system Tariff name {0} , Tarriff id {1}, by {2}",
                                   tariff.Name, tariff.Id, CurrentRequestData.CurrentUser.Id), "Tariff Added.");
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool DeleteTariff(Tariff tariff)
        {
            if (tariff != null)
            {

                _session.Transact(session => session.Delete(tariff));
                return true;
            }
            return false;
        }

        public bool UpdateTarrif(Tariff tariff)
        {
            if (tariff != null)
            {

                _session.Transact(session => session.Update(tariff));
                return true;
            }
            return false;
        }

        public Tariff GetTariff(int id)
        {
            Tariff tariff = _session.QueryOver<Tariff>().Where(x => x.Id == id).SingleOrDefault();
            return tariff;
        }

        public bool AddnewCategory(TariffCategory category)
        {
            if (category != null)
            {
                category.Name = category.Name.ToUpper();
                _session.Transact(session => session.Save(category));
                _helper.Log(LogEntryType.Audit, null,
                               string.Format(
                                   "New Tarrif category has been added to the system category name {0} , Tarriff id {1}, by {2}",
                                   category.Name, category.Id, CurrentRequestData.CurrentUser.Id), "category Added.");
                return true;
            }
            return false;
        }

        public TariffCategory GetCategory(int id)
        {
            TariffCategory tariffcat = _session.QueryOver<TariffCategory>().Where(x => x.Id == id).SingleOrDefault();
            return tariffcat;
        }

        public bool DeleteCategory(TariffCategory category)
        {
            if (category != null)
            {
                _session.Transact(session => session.Delete(category));
                return true;
            }
            return false;
        }

        public IList<TariffCategory> GetallTariffCategory()
        {
            return _session.QueryOver<TariffCategory>().Where(x => x.IsDeleted == false).List<TariffCategory>();
        }

        public bool AddnewDrugTariff(DrugTariff drug)
        {
            if (drug != null)
            {
                _session.Transact(session => session.Save(drug));
                return true;
            }
            else
            {
                return false;
            }
        }

        public DrugTariff GetDrug(int id)
        {
            DrugTariff drugtariff = _session.QueryOver<DrugTariff>().Where(x => x.Id == id).SingleOrDefault();
            return drugtariff;
        }

        public bool DeleteDrug(DrugTariff drug)
        {
            if (drug != null)
            {

                _session.Transact(session => session.Delete(drug));
                return true;
            }
            return false;
        }

        public IList<DrugTariff> Getalldrugtariff()
        {
            return _session.QueryOver<DrugTariff>().Where(x => x.IsDeleted == false).List<DrugTariff>();
        }

        public bool UpdateDrug(DrugTariff drugtariff)
        {
            if (drugtariff != null)
            {

                _session.Transact(session => session.Update(drugtariff));
                return true;
            }
            return false;
        }

        public bool AddnewServiceTariff(ServiceTariff serviceTariff)
        {
            //add new  service tariff.
            if (serviceTariff != null)
            {
                _session.Transact(session => session.Save(serviceTariff));
                return true;
            }
            else
            {
                return false;
            }
        }

        public ServiceTariff GetServiceTariff(int id)
        {
            ServiceTariff servicetariff = _session.QueryOver<ServiceTariff>().Where(x => x.Id == id).SingleOrDefault();
            return servicetariff;
        }

        public bool DeleteServiceTariff(ServiceTariff serviceTariff)
        {
            if (serviceTariff != null)
            {

                _session.Transact(session => session.Delete(serviceTariff));
                return true;
            }
            return false;
        }

        public IList<ServiceTariff> GetallServicetariff()
        {
            return _session.QueryOver<ServiceTariff>().Where(x => x.IsDeleted == false).List<ServiceTariff>();
        }

        public bool UpdateServiceTariff(ServiceTariff serviceTariff)
        {
            if (serviceTariff != null)
            {

                _session.Transact(session => session.Update(serviceTariff));
                return true;
            }
            return false;
        }

        public bool ClearallDrugTariff(int TariffID)
        {
            IList<TariffCategory> lst = _session.QueryOver<TariffCategory>().Where(x => x.Type == 0 && x.TariffId == TariffID).List<TariffCategory>();

            foreach (TariffCategory item in lst)
            {
                DeleteCategory(item);
                ITransaction tx = _session.BeginTransaction();
                string hqlVersionedUpdate = "update DrugTariff set IsDeleted=1 where GroupId = :newValue";
                int updatedEntities = _session.CreateQuery(hqlVersionedUpdate)

                        .SetInt32("newValue", item.Id)
                        .ExecuteUpdate();
                tx.Commit();

            }
            return true;
        }

        public bool ClearallServiceTariff(int TariffID)
        {

            IList<TariffCategory> lst = _session.QueryOver<TariffCategory>().Where(x => x.Type == 1 && x.TariffId == TariffID).List<TariffCategory>();

            foreach (TariffCategory item in lst)
            {
                DeleteCategory(item);
                ITransaction tx = _session.BeginTransaction();
                string hqlVersionedUpdate = "update ServiceTariff set IsDeleted=1 where GroupId = :newValue";
                int updatedEntities = _session.CreateQuery(hqlVersionedUpdate)

                        .SetInt32("newValue", item.Id)
                        .ExecuteUpdate();
                tx.Commit();
            }
            return true;

        }

        public IList<DrugTariff> QueryAllDrugTariff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, int groupid, string sortOrder, string scrProvider, string scrUsers, int otherFilters)
        {
            IQueryOver<DrugTariff, DrugTariff> query = _session.QueryOver<DrugTariff>().Where(x => x.IsDeleted == false && x.GroupId == groupid);

            if (!string.IsNullOrEmpty(search))
            {
                search = "%" + search + "%";
                query.Where(Restrictions.On<DrugTariff>(x => x.Name).IsInsensitiveLike(search));
            }

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.Skip(start).Take(lenght).List();
        }

        public IList<ServiceTariff> QueryAllServiceTariff(out int totalRecord, out int totalcountinresult, string search, int start, int lenght, string sortColumn, int groupid, string sortOrder, string scrProvider, string scrUsers, int otherFilters)
        {
            IQueryOver<ServiceTariff, ServiceTariff> query = _session.QueryOver<ServiceTariff>().Where(x => x.IsDeleted == false && x.GroupId == groupid);

            if (!string.IsNullOrEmpty(search))
            {
                search = "%" + search + "%";
                query.Where(Restrictions.On<ServiceTariff>(x => x.Name).IsInsensitiveLike(search));
            }

            //return normal list.
            totalRecord = query.RowCount();
            totalcountinresult = totalRecord;
            return query.Skip(start).Take(lenght).List();
        }

        public IList<DrugTariff> GetalldrugtariffByCategory(int catId)
        {
            IList<DrugTariff> drugtariff = _session.QueryOver<DrugTariff>().Where(x => x.GroupId == catId && x.IsDeleted == false).List();
            return drugtariff;
        }

        public IList<ServiceTariff> GetallservicetariffByCategory(int catId)
        {
            IList<ServiceTariff> servicetariff = _session.QueryOver<ServiceTariff>().Where(x => x.GroupId == catId && x.IsDeleted == false).List();
            return servicetariff;
        }
    }
}