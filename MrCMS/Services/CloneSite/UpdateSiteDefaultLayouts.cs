using System.Linq;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Multisite;
using MrCMS.Settings;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-65)]
    public class UpdateSiteDefaultLayouts : ICloneSiteParts
    {
        private readonly ILegacySettingsProvider _legacySettingsProvider;

        public UpdateSiteDefaultLayouts(ILegacySettingsProvider legacySettingsProvider)
        {
            _legacySettingsProvider = legacySettingsProvider;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            ConfigurationProvider toProvider = new ConfigurationProvider(@to, _legacySettingsProvider);
            PageDefaultsSettings pageDefaultsSettings = toProvider.GetSiteSettings<PageDefaultsSettings>();

            System.Collections.Generic.List<string> keys = pageDefaultsSettings.Layouts.Keys.ToList();
            foreach (string key in keys.Where(key => pageDefaultsSettings.Layouts[key].HasValue))
            {
                Layout layout = siteCloneContext.FindNew<Layout>(pageDefaultsSettings.Layouts[key].Value);
                if (layout != null)
                    pageDefaultsSettings.Layouts[key] = layout.Id;
            }

            toProvider.SaveSettings(pageDefaultsSettings);
        }
    }
}