using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class PaymentBatchListPageMetadata : DocumentMetadataMap<PaymentBatchListPage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "PaymentBatchList"; }
        }

        public override string WebGetController
        {
            get { return "ClaimsPage"; }
        }
    }
}