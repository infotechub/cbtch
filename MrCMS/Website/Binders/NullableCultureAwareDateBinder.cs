using System;
using System.Web.Mvc;

namespace MrCMS.Website.Binders
{
    public class NullableCultureAwareDateBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext", "controllerContext is null.");
            if (bindingContext == null)
                throw new ArgumentNullException("bindingContext", "bindingContext is null.");

            ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null) return null;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            try
            {
                object date = value.ConvertTo(typeof(DateTime), CurrentRequestData.CultureInfo);

                return date;
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                return null;
            }
        }
    }
}