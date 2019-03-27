using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class ConnectCareNewSponsorPageMetadata : DocumentMetadataMap<ConnectCareNewSponsorPage>
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
                return "AddSponsor";
            }

        }

        public override string WebGetController
        {
            get
            {
                return "ConnectCarePage";
            }

        }
    }
}