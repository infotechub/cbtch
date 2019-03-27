﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MrCMS.Helpers
{
    public static class StringHelper
    {
        public static string BreakUpString(this string value)
        {
            return Regex.Replace(value,
                                 "((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))",
                                 " $1",
                                 RegexOptions.Compiled).Trim();
        }

        public static List<int> GetIntList(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                       ? new List<int>()
                       : value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt32(s.Trim()))
                             .ToList();
        }

        public static int GetIntValue(this string value, int defaultValue = 0)
        {
            int val;
            return string.IsNullOrWhiteSpace(value)
                       ? defaultValue
                       : int.TryParse(value, out val) ? val : defaultValue;
        }

        public static string ToString(this IEnumerable<int> value)
        {
            if (value == null || !value.Any())
                return string.Empty;

            return string.Join(",", value);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static string StripHtml(this string htmlString, string replaceHtmlWith = " ")
        {
            if (string.IsNullOrWhiteSpace(htmlString))
                return "";
            return Regex.Replace(htmlString, @"<(.|\n)*?>", replaceHtmlWith);
        }

        public static string TruncateString(this string text, int maxCharacters, string trailingText = "...")
        {
            if (string.IsNullOrEmpty(text) || maxCharacters <= 0)
                return text;
            string returnString = "";
            if (text.Length >= maxCharacters)
                returnString = text.Substring(0, maxCharacters) + trailingText;
            else
                returnString = text;
            
            return returnString;
        }

        public static string SafeTrim(this string s)
        {
            if (s == null)
                return s;
            return s.Trim();
        }
        public static string SafeTrim(this string s, params char[] chars)
        {
            if (s == null)
                return s;
            return s.Trim(chars);
        }
    }
}
