﻿using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;

namespace MrCMS.DbConfiguration.Types
{
    [Serializable]
    public class LocalNullableDateTimeData : BaseImmutableUserType<DateTime?>
    {
        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            object nullSafeGet = NHibernateUtil.DateTime.NullSafeGet(rs, names[0]);
            if (nullSafeGet == null)
                return null;
            DateTime dateTime = DateTime.SpecifyKind((DateTime)nullSafeGet, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, TimeZoneInfo.Local);
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value != null)
            {
                DateTime dateTime = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Unspecified);
                dateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, TimeZoneInfo.Utc);
                NHibernateUtil.DateTime.NullSafeSet(cmd, dateTime, index);
            }
            else
                NHibernateUtil.DateTime.NullSafeSet(cmd, value, index);
        }

        public override SqlType[] SqlTypes
        {
            get { return new[] { NHibernateUtil.DateTime.SqlType }; }
        }
    }
}