using System;

namespace MrCMS.Helpers
{
    public static class SitemapDateHelper
    {
        public static string SitemapDateString(this DateTime dateTime)
        {
            string date = dateTime.ToString("O");

            if (!date.EndsWith(":00"))
                date += "+00:00";
            return date;
        }
    }
}