using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class ProviderPortalLoginPageMetadata : DocumentMetadataMap<ProviderPortalLoginPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "ProviderPortalLogin"; }
        }
        public override string WebPostAction
        {
            get { return "ProviderPortalLogin"; }
        }
        public override string WebGetController
        {
            get { return "ProviderPage"; }
        }
        public override string WebPostController
        {
            get { return "ProviderPage"; }
        }
    }
}