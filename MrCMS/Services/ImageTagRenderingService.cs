using System.Drawing;
using System.Web.Mvc;
using MrCMS.DbConfiguration;
using MrCMS.Helpers;
using MrCMS.Services.Caching;
using MrCMS.Settings;
using NHibernate;

namespace MrCMS.Services
{
    public class ImageTagRenderingService : IImageTagRenderingService
    {
        private readonly ISession _session;
        private readonly IImageProcessor _imageProcessor;
        private readonly IFileService _fileService;
        private readonly MediaSettings _mediaSettings;

        public ImageTagRenderingService(ISession session, IImageProcessor imageProcessor, IFileService fileService, MediaSettings mediaSettings)
        {
            _session = session;
            _imageProcessor = imageProcessor;
            _fileService = fileService;
            _mediaSettings = mediaSettings;
        }

        public MvcHtmlString RenderImage(HtmlHelper helper, string imageUrl, Size targetSize = new Size(), string alt = null,
            string title = null, object attributes = null)
        {
            Models.CachingInfo cachingInfo = _mediaSettings.GetCachingInfo(imageUrl, targetSize, alt, title, attributes);
            return helper.GetCached(cachingInfo, htmlHelper =>
               {
                   using (new SiteFilterDisabler(_session))
                   {
                       if (string.IsNullOrWhiteSpace(imageUrl))
                           return MvcHtmlString.Empty;

                       Entities.Documents.Media.MediaFile image = _imageProcessor.GetImage(imageUrl);
                       if (image == null)
                           return MvcHtmlString.Empty;

                       if (targetSize != default(Size) && ImageProcessor.RequiresResize(image.Size, targetSize))
                       {
                           string location = _fileService.GetFileLocation(image, targetSize);
                           if (!string.IsNullOrWhiteSpace(location))
                               imageUrl = location;
                       }

                       TagBuilder tagBuilder = new TagBuilder("img");
                       tagBuilder.Attributes.Add("src", imageUrl);
                       tagBuilder.Attributes.Add("alt", alt ?? image.Title);
                       tagBuilder.Attributes.Add("title", title ?? image.Description);
                       if (attributes != null)
                       {
                           System.Web.Routing.RouteValueDictionary routeValueDictionary = MrCMSHtmlHelper.AnonymousObjectToHtmlAttributes(attributes);
                           foreach (System.Collections.Generic.KeyValuePair<string, object> kvp in routeValueDictionary)
                           {
                               tagBuilder.Attributes.Add(kvp.Key, kvp.Value.ToString());
                           }
                       }
                       return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
                   }
               });
        }
    }
}