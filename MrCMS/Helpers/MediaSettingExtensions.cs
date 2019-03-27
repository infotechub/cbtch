using System;
using System.Drawing;
using System.Text;
using MrCMS.Models;
using MrCMS.Settings;

namespace MrCMS.Helpers
{
    public static class MediaSettingExtensions
    {
        public const string RenderImagePrefix = "RenderImage.";

        public static CachingInfo GetCachingInfo(this MediaSettings mediasettings, string imageUrl, Size targetSize = default(Size), string alt = null, string title = null, object attributes = null)
        {
            string cacheKey = GetCacheKey(imageUrl, targetSize, alt, title, attributes);
            return new CachingInfo(mediasettings.Cache, cacheKey, TimeSpan.FromSeconds(mediasettings.CacheLength), mediasettings.CacheExpiryType);
        }

        private static string GetCacheKey(string imageUrl, Size targetSize, string alt, string title, object attributes)
        {
            StringBuilder stringBuilder = new StringBuilder(RenderImagePrefix + imageUrl);
            if (targetSize != default(Size))
                stringBuilder.AppendFormat(";size:{0},{1}", targetSize.Width, targetSize.Height);
            if (!string.IsNullOrWhiteSpace(alt))
                stringBuilder.AppendFormat(";alt:{0}", alt);
            if (!string.IsNullOrWhiteSpace(title))
                stringBuilder.AppendFormat(";title:{0}", title);
            if (!string.IsNullOrWhiteSpace(title))
                stringBuilder.AppendFormat(";title:{0}", title);
            if (attributes != null)
            {
                System.Web.Routing.RouteValueDictionary routeValueDictionary = MrCMSHtmlHelper.AnonymousObjectToHtmlAttributes(attributes);
                foreach (System.Collections.Generic.KeyValuePair<string, object> kvp in routeValueDictionary)
                {
                    stringBuilder.AppendFormat(";{0}:{1}", kvp.Key, kvp.Value);
                }
            }

            return stringBuilder.ToString();
        }
    }
}