using MrCMS.Entities.Multisite;
using MrCMS.Settings;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-100)]
    public class CopySettings : ICloneSiteParts
    {
        private readonly ILegacySettingsProvider _legacySettingsProvider;

        public CopySettings(ILegacySettingsProvider legacySettingsProvider)
        {
            _legacySettingsProvider = legacySettingsProvider;
        }

        public void Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            ConfigurationProvider fromProvider = new ConfigurationProvider(@from, _legacySettingsProvider);
            ConfigurationProvider toProvider = new ConfigurationProvider(@to, _legacySettingsProvider);
            System.Collections.Generic.List<SiteSettingsBase> siteSettingsBases = fromProvider.GetAllSiteSettings();
            siteSettingsBases.ForEach(toProvider.SaveSettings);
        }
    }
}