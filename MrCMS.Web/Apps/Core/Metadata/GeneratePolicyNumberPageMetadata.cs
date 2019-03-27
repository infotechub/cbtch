using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class GeneratePolicyNumberPageMetadata : DocumentMetadataMap<GeneratePolicyNumberPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "GeneratePolicyNumber"; }
        }

        public override string WebGetController
        {
            get { return "EnrolleePage"; }
        }
    }
}