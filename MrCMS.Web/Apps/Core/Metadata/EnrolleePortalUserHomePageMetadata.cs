﻿using MrCMS.Entities.Documents.Metadata;
using MrCMS.Web.Apps.Core.Pages;

namespace MrCMS.Web.Apps.Core.Metadata
{
    public class EnrolleePortalUserHomePageMetadata : DocumentMetadataMap<EnrolleePortalUserHomePage>
    {
        public override string IconClass
        {
            get { return "glyphicon glyphicon-user"; }
        }

        public override string WebGetAction
        {
            get { return "EnrolleePortalUserHome"; }
        }
        public override string WebPostAction
        {
            get { return "EnrolleePortalUserHome"; }
        }
        public override string WebGetController
        {
            get { return "EnrolleePage"; }
        }
    }
}