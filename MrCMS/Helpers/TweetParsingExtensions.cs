using System;
using System.Text.RegularExpressions;
using System.Web;

namespace MrCMS.Helpers
{
    public static class TweetParsingExtensions
    {
        public static IHtmlString ParseTweet(this string tweet)
        {
            Regex link =
                new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?");
            Regex screenName = new Regex(@"@\w+");
            Regex hashTag = new Regex(@"#\w+");

            string formattedTweet = link.Replace(tweet, match =>
                                                            {
                                                                string val = match.Value;
                                                                return "<a href='" + val + "'>" + val + "</a>";
                                                            });

            formattedTweet = screenName.Replace(formattedTweet, match =>
                                                                    {
                                                                        string val = match.Value.Trim('@');
                                                                        return
                                                                            string.Format(
                                                                                "@<a href='http://twitter.com/{0}'>{1}</a>",
                                                                                val, val);
                                                                    });

            formattedTweet = hashTag.Replace(formattedTweet, match =>
                                                                 {
                                                                     string val = match.Value;
                                                                     return
                                                                         string.Format(
                                                                             "<a href='http://twitter.com/search/?q=%23{0}'>{1}</a>",
                                                                             val.Substring(1), val);
                                                                 });

            return new HtmlString(formattedTweet);
        }
    }
}