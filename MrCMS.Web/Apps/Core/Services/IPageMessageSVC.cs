namespace MrCMS.Web.Apps.Core.Services
{
    public interface IPageMessageSvc
    {

        bool SetErrormessage(string msg);
        bool SetSuccessMessage(string msg);
        bool ClearErrormessage();
        bool ClearSuccessmessage();

    }
}