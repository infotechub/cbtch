using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MrCMS.Helpers;

namespace MrCMS.Indexing.Utils
{
    public static class LuceneQueryStringHelper
    {
        public static Query SafeGetSearchQuery(this string term, MultiFieldQueryParser q, Analyzer analyser)
        {
            Query query;
            try
            {
                query = q.Parse(term.MakeFuzzy());
            }
            catch
            {
                string searchTerm = term.Sanitize(analyser);
                query = q.Parse(searchTerm);
            }
            return query;
        }
        public static string MakeFuzzy(this string keywords)
        {
            string makeFuzzy = Regex.Replace(keywords, "[A-Za-z0-9_\\^\\?\\@\\-\\*\\:\\.\\\"\\(\\)\\{\\}\\[\\]\\~]+",
                match =>
                    !match.Value.Equals("and", StringComparison.OrdinalIgnoreCase) &&
                    !match.Value.Equals("or", StringComparison.OrdinalIgnoreCase) &&
                    !match.Value.Contains("~", StringComparison.OrdinalIgnoreCase)
                        ? match.Value.TrimEnd('~') + "~"
                        : match.Value);
            return makeFuzzy;
        }

        public static string Sanitize(this string keywords, Analyzer analyser)
        {
            MatchCollection matchCollection = Regex.Matches(keywords, "\"(.*?)\"");
            List<string> quotes = (from Match match in matchCollection select match.Value).ToList();
            keywords = matchCollection.OfType<Match>()
                .OrderByDescending(match => match.Index)
                .Aggregate(keywords,
                    (current, match) =>
                        current.Substring(0, match.Index) + current.Substring(match.Index + match.Length));
            List<string> returnedWords =
                quotes.Select(quote => GetTokens(quote, analyser))
                    .Select(quoteList => "\"" + string.Join(" ", quoteList) + "\"~")
                    .ToList();

            List<string> list = GetTokens(keywords, analyser);
            returnedWords.Add(string.Join(" ", list.Select(s => s + "~")));
            return string.Join(" ", returnedWords);
        }

        private static List<string> GetTokens(string keywords, Analyzer analyser)
        {
            TokenStream tokenStream = analyser.TokenStream(null, new StringReader(keywords));
            ITermAttribute termAttribute = tokenStream.GetAttribute<ITermAttribute>();
            tokenStream.Reset();
            List<string> list = new List<string>();
            while (tokenStream.IncrementToken())
            {
                string term = termAttribute.Term;
                list.Add(term);
            }
            return list;
        }
    }
}