using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.DbConfiguration.Types;
using MrCMS.Web.Apps.Faqs.Entities;
using NHibernate.Mapping;

namespace MrCMS.Web.Apps.Faqs.DbConfiguration
{
    public class FaqItemOverride : IAutoMappingOverride<Faq>
    {
        public void Override(AutoMapping<Faq> mapping)
        {
            mapping.Map(faq => faq.Answer).CustomType<VarcharMax>().Length(4001);


        }
    }
}