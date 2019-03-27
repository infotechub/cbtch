using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class BulkStaffJobPageMetadata : DocumentMetadataMap<BulkStaffJobPage>
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
                return "BulkStaffJob";
            }

        }

        public override string WebGetController
        {
            get
            {
                return "CompanyPage";
            }

        }
    }
}