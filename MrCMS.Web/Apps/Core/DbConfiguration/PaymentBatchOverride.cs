using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.DbConfiguration.Types;
using MrCMS.Web.Apps.Core.Entities;
namespace MrCMS.Web.Apps.Core.DbConfiguration
{

    public class PaymentBatchOverride : IAutoMappingOverride<PaymentBatch>
    {
        public void Override(AutoMapping<PaymentBatch> mapping)
        {
            mapping.HasMany(document => document.ClaimBatchList).KeyColumn("PaymentBatchId").Cascade.None();
            //mapping.HasOne()
        }





    }


}