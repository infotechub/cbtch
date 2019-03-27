using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class EnrolleeStaffListPageMetadata : DocumentMetadataMap<EnrolleeStaffListPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "EnrolleeStaffList"; }
        }

        public override string WebGetController
        {
            get { return "CompanyPage"; }
        }
    }
}