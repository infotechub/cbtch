using System;
using System.Collections.Generic;
using System.Reflection;
using MrCMS.Helpers;
using MrCMS.Settings;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;

namespace MrCMS.IoC
{
    public class NinjectSystemSettingsBinder : IBindingGenerator
    {
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {
            List<IBindingWhenInNamedWithOrOnSyntax<object>> list = new List<IBindingWhenInNamedWithOrOnSyntax<object>>();
            if (type.IsInterface || type.IsAbstract)
            {
                return list;
            }

            bindingRoot.Bind(type).ToMethod(context => GetValue(type, context));

            return list;
        }

        private static object GetValue(Type type, IContext context)
        {
            ISystemConfigurationProvider configProvider =
                context.Kernel.Get<ISystemConfigurationProvider>();
            MethodInfo method =
                typeof (ISystemConfigurationProvider).GetMethodExt("GetSystemSettings");

            return method != null
                ? method.MakeGenericMethod(type)
                    .Invoke(configProvider,
                        new object[] {})
                : null;
        }
    }
}