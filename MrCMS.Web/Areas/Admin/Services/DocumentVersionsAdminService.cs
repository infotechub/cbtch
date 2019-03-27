using MrCMS.Entities.Documents;
using MrCMS.Helpers;
using MrCMS.Paging;
using MrCMS.Web.Areas.Admin.Models;
using NHibernate;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class DocumentVersionsAdminService : IDocumentVersionsAdminService
    {
        private readonly ISession _session;

        public DocumentVersionsAdminService(ISession session)
        {
            _session = session;
        }

        public VersionsModel GetVersions(Document document, int page)
        {
            IPagedList<DocumentVersion> versions = _session.QueryOver<DocumentVersion>()
                .Where(version => version.Document.Id == document.Id)
                .OrderBy(version => version.CreatedOn).Desc
                .Paged(page);

            return new VersionsModel(versions, document.Id);
        }

        public DocumentVersion GetDocumentVersion(int id)
        {
            return _session.Get<DocumentVersion>(id);
        }

        public void RevertToVersion(DocumentVersion documentVersion)
        {
            Document currentVersion = documentVersion.Document;
            Document previousVersion = currentVersion.GetVersion(documentVersion.Id);

            System.Collections.Generic.List<System.Reflection.PropertyInfo> versionProperties = currentVersion.GetType().GetVersionProperties();
            foreach (System.Reflection.PropertyInfo versionProperty in versionProperties)
            {
                versionProperty.SetValue(currentVersion, versionProperty.GetValue(previousVersion, null), null);
            }
            _session.Transact(session => session.Update(currentVersion));
        }
    }
}