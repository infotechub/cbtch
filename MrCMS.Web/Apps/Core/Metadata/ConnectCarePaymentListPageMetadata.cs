using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class ConnectCarePaymentListPageMetadata : DocumentMetadataMap<ConnectCarePaymentListPage>
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
                return "PaymentList";
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