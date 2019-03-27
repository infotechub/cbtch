﻿using MrCMS.Services;
using MrCMS.Website;
using Owin;

namespace MrCMS.Helpers
{
    public static class AuthConfigurationServiceExtensions
    {
        public static void ConfigureAuth(this IAppBuilder app)
        {
            IStandardAuthConfigurationService standardAuthConfigurationService = MrCMSApplication.Get<IStandardAuthConfigurationService>();
            standardAuthConfigurationService.ConfigureAuth(app);
            if (CurrentRequestData.DatabaseIsInstalled)
            {
                IAuthConfigurationService authConfigurationService = MrCMSApplication.Get<IAuthConfigurationService>();
                authConfigurationService.ConfigureAuth(app);
            }
        }
    }
}