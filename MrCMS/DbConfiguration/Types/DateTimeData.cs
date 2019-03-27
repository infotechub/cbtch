﻿using System;
using System.Data;
using MrCMS.Website;
using NHibernate;
using NHibernate.SqlTypes;

namespace MrCMS.DbConfiguration.Types
{
    [Serializable]
    public class DateTimeData : BaseImmutableUserType<DateTime>
    {
        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            DateTime dateTime = DateTime.SpecifyKind((DateTime)NHibernateUtil.DateTime.NullSafeGet(rs, names[0]),
                                                DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, CurrentRequestData.TimeZoneInfo);
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            DateTime dateTime = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Unspecified);
            dateTime = TimeZoneInfo.ConvertTime(dateTime, CurrentRequestData.TimeZoneInfo, TimeZoneInfo.Utc);
            NHibernateUtil.DateTime.NullSafeSet(cmd, dateTime, index);
        }

        public override SqlType[] SqlTypes
        {
            get { return new[] { NHibernateUtil.DateTime.SqlType }; }
        }
    }
}