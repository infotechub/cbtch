using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using Ninject;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class SetWebpageAdminViewData : ISetWebpageAdminViewData
    {
        private readonly IKernel _kernel;

        static SetWebpageAdminViewData()
        {
            AssignViewDataTypes = new Dictionary<Type, HashSet<Type>>();

            foreach (Type type in TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Webpage>().Where(type => !type.ContainsGenericParameters))
            {
                HashSet<Type> hashSet = new HashSet<Type>();

                Type thisType = type;
                while (thisType != null && typeof(Webpage).IsAssignableFrom(thisType))
                {
                    foreach (Type assignType in TypeHelper.GetAllConcreteTypesAssignableFrom(
                        typeof(BaseAssignWebpageAdminViewData<>).MakeGenericType(thisType)))
                    {
                        hashSet.Add(assignType);
                    }
                    thisType = thisType.BaseType;

                }

                AssignViewDataTypes.Add(type, hashSet);
            }
        }

        public SetWebpageAdminViewData(IKernel kernel)
        {
            _kernel = kernel;
        }

        private static readonly Dictionary<Type, HashSet<Type>> AssignViewDataTypes;

        public void SetViewData<T>(T webpage, ViewDataDictionary viewData) where T : Webpage
        {
            if (webpage == null)
            {
                return;
            }
            Type type = webpage.GetType();
            if (AssignViewDataTypes.ContainsKey(type))
            {
                foreach (
                    object assignAdminViewData in
                        AssignViewDataTypes[type].Select(assignViewDataType => _kernel.Get(assignViewDataType))
                    )
                {
                    BaseAssignWebpageAdminViewData adminViewData = assignAdminViewData as BaseAssignWebpageAdminViewData;
                    if (adminViewData != null) adminViewData.AssignViewDataBase(webpage, viewData);
                }
            }
        }
    }
}