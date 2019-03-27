using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class EnrolleePortalLoginPageMetadata : DocumentMetadataMap<EnrolleePortalLoginPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "EnrolleePortalLogin"; }
        }
        public override string WebPostAction
        {
            get { return "EnrolleePortalLogin"; }
        }
        public override string WebGetController
        {
            get { return "EnrolleePage"; }
        }
    }
}