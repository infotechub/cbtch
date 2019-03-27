using MrCMS.Entities.Documents.Web;
using MrCMS.Website;

namespace MrCMS.Events.Documents
{
    public class MarkWebpageAsUnpublished : IOnUpdating<Webpage>
    {
        public void Execute(OnUpdatingArgs<Webpage> args)
        {
            System.DateTime now = CurrentRequestData.Now;
            Webpage webpage = args.Item;
            if (webpage.Published && (webpage.PublishOn == null || webpage.PublishOn > now))
            {
                webpage.Published = false;
            }
        }
    }
}