using System;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Helpers
{
    public static class PageTemplateExtensions
    {
        public static Type GetPageType(this PageTemplate template)
        {
            return template != null
                ? TypeHelper.GetTypeByName(template.PageType)
                : null;
        }
        public static string GetLayoutName(this PageTemplate template)
        {
            return template != null && template.Layout != null
                ? template.Layout.Name
                : string.Empty;
        }
        public static string GetPageTypeName(this PageTemplate template)
        {
            Type pageType = GetPageType(template);
            if (pageType == null) return string.Empty;

            Entities.Documents.DocumentMetadata metadata = pageType.GetMetadata();

            return metadata != null
                ? metadata.Name
                : string.Empty;
        }

        public static Type GetUrlGeneratorType(this PageTemplate template)
        {
            return template != null
                ? TypeHelper.GetTypeByName(template.UrlGeneratorType)
                : null;
        }
    }
}