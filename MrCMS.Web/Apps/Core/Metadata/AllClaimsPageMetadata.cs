using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class AllClaimsPageMetadata : DocumentMetadataMap<AllClaimsPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "AllClaimList"; }
        }

        public override string WebGetController
        {
            get { return "ClaimsPage"; }
        }
    }
}