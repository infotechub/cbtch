using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class CompanyDefaultPlanPagePageMetadata : DocumentMetadataMap<CustomizeDefaultPlanPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "CustomizeDefaultPlan"; }
        }

        public override string WebGetController
        {
            get { return "CompanyPage"; }
        }
    }
}