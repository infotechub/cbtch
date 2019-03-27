using MrCMS.Apps;
using Ninject;

namespace MrCMS.Web.Apps.Faqs
{
    public class FaqsApp : MrCMSApp
    {
        public override string AppName
        {
            get { return "FAQs"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        protected override void RegisterApp(MrCMSAppRegistrationContext context)
        {

        }

        protected override void RegisterServices(IKernel kernel)
        {

        }
    }
}