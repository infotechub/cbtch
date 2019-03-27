using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using MrCMS.DbConfiguration.Configuration;
using MrCMS.DbConfiguration.Types;

namespace MrCMS.DbConfiguration.Conventions
{
    public class StringLengthConvention : IPropertyConvention
    {
        public void Apply(IPropertyInstance instance)
        {
            if (instance.Property.PropertyType != typeof(string))
                return;
            MemberInfo memberInfo = instance.Property.MemberInfo;
            StringLengthAttribute stringLengthAttribute = memberInfo.GetCustomAttribute<StringLengthAttribute>();
            IsDBLengthAttribute isDbLengthAttribute = memberInfo.GetCustomAttribute<IsDBLengthAttribute>();
            if (stringLengthAttribute != null && isDbLengthAttribute != null)
            {
                instance.Length(stringLengthAttribute.MaximumLength);
            }
            else
            {
                instance.CustomType<VarcharMax>();
                instance.Length(4001);
            }
        }
    }
}