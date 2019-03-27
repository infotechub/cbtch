using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class EnrolleePageRegMetadata : DocumentMetadataMap<EnrolleeRegPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "EnrolleeReg"; }
        }

        public override string WebGetController
        {
            get { return "EnrolleePage"; }
        }
    }
}