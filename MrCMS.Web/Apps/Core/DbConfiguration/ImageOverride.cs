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

    public class ImageOverride : IAutoMappingOverride<EnrolleePassport>
    {
        public void Override(AutoMapping<EnrolleePassport> mapping)
        {
            mapping.Map(passport => passport.Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            //mapping.Map(passport => passport.Imgraw).CustomType<BinaryData<>>().Length(9999);


        }



    }

    public class TempImageOverride : IAutoMappingOverride<TempEnrollee>
    {
        public void Override(AutoMapping<TempEnrollee> mapping)
        {
            mapping.Map(passport => passport.Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            mapping.Map(passport => passport.S_Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            mapping.Map(passport => passport.child1_Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            mapping.Map(passport => passport.child2_Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            mapping.Map(passport => passport.child3_Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);
            mapping.Map(passport => passport.child4_Imgraw).CustomSqlType("varbinary(MAX)").Length(9999);

            //mapping.Map(passport => passport.Imgraw).CustomType<BinaryData<>>().Length(9999);


        }



    }

    public class PendDependantOverride : IAutoMappingOverride<PendingDependant>
    {
        public void Override(AutoMapping<PendingDependant> mapping)
        {
            mapping.Map(passport => passport.ImgRaw).CustomSqlType("varbinary(MAX)").Length(9999);
            //mapping.Map(passport => passport.Imgraw).CustomType<BinaryData<>>().Length(9999);


        }


    }
}