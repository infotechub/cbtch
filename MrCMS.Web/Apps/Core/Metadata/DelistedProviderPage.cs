using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class DelistedProviderPageMetadata : DocumentMetadataMap<DelistedProviderPage>
    {
        public override string IconClass
        {
            get
            {
                return "glyphicon glyphicon-user";
            }
        }

        public override string WebGetAction
        {
            get
            {
                return "DelistedProviders";
            }

        }

        public override string WebGetController
        {
            get
            {
                return "ProviderPage";
            }

        }
    }
}