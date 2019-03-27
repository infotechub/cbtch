using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class AdmissionMonitorPageMetadata : DocumentMetadataMap<AdmissionMonitorPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "Admission"; }
        }

        public override string WebGetController
        {
            get { return "ClaimsPage"; }
        }
    }
}