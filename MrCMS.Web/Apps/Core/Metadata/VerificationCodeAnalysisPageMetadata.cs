using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class VerificationcodeAnalysisPageMetadata : DocumentMetadataMap<VerificationCodeAnalysisPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "ShowVerificationAnalysis"; }
        }

        public override string WebGetController
        {
            get { return "EnrolleePage"; }
        }
    }
}