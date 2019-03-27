using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public class CustomFormRenderer : ICustomFormRenderer
    {
        private readonly IElementRendererManager _elementRendererManager;
        private readonly ILabelRenderer _labelRenderer;
        private readonly IValidationMessaageRenderer _validationMessaageRenderer;
        private readonly ISubmittedMessageRenderer _submittedMessageRenderer;
        private readonly SiteSettings _siteSettings;

        public CustomFormRenderer(IElementRendererManager elementRendererManager, ILabelRenderer labelRenderer,
                                  IValidationMessaageRenderer validationMessaageRenderer,
                                  ISubmittedMessageRenderer submittedMessageRenderer, SiteSettings siteSettings)
        {
            _elementRendererManager = elementRendererManager;
            _labelRenderer = labelRenderer;
            _validationMessaageRenderer = validationMessaageRenderer;
            _submittedMessageRenderer = submittedMessageRenderer;
            _siteSettings = siteSettings;
        }

        public string GetForm(Webpage webpage, FormSubmittedStatus submittedStatus)
        {
            if (webpage == null)
                return string.Empty;

            IList<FormProperty> formProperties = webpage.FormProperties;
            if (!formProperties.Any())
                return string.Empty;

            TagBuilder form = GetForm(webpage);

            string formDesign = webpage.FormDesign;
            formDesign = Regex.Replace(formDesign, "{label:([^}]+)}", AddLabel(formProperties));
            formDesign = Regex.Replace(formDesign, "{input:([^}]+)}", AddElement(formProperties, submittedStatus));
            formDesign = Regex.Replace(formDesign, "{validation:([^}]+)}", AddValidation(formProperties));
            formDesign = Regex.Replace(formDesign, "{submitted-message}", AddSubmittedMessage(webpage, submittedStatus));
            form.InnerHtml = formDesign;

            if (_siteSettings.HasHoneyPot)
                form.InnerHtml += _siteSettings.GetHoneypot();

            return form.ToString();
        }

        private MatchEvaluator AddValidation(IList<FormProperty> formProperties)
        {
            return match =>
            {

                FormProperty formProperty =
                    formProperties.FirstOrDefault(
                        property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                return formProperty == null
                           ? string.Empty
                           : _validationMessaageRenderer.AppendRequiredMessage(formProperty).ToString();
            };
        }

        private string AddSubmittedMessage(Webpage webpage, FormSubmittedStatus submittedStatus)
        {
            if (!submittedStatus.Submitted) return string.Empty;

            return _submittedMessageRenderer.AppendSubmittedMessage(webpage, submittedStatus).ToString();
        }

        private MatchEvaluator AddLabel(IList<FormProperty> formProperties)
        {
            return match =>
                       {
                           FormProperty formProperty =
                               formProperties.FirstOrDefault(
                                   property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                           return formProperty == null
                                      ? string.Empty
                                      : _labelRenderer.AppendLabel(formProperty).ToString();
                       };
        }
        private MatchEvaluator AddElement(IList<FormProperty> formProperties, FormSubmittedStatus submittedStatus)
        {
            return match =>
                       {

                           FormProperty formProperty =
                               formProperties.FirstOrDefault(
                                   property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                           if (formProperty == null)
                               return string.Empty;
                           string existingValue = submittedStatus.Data[formProperty.Name];

                           IFormElementRenderer renderer = _elementRendererManager.GetElementRenderer(formProperty);

                           return
                               renderer.AppendElement(formProperty, existingValue, _siteSettings.FormRendererType)
                                       .ToString(renderer.IsSelfClosing
                                                     ? TagRenderMode.SelfClosing
                                                     : TagRenderMode.Normal);
                       };
        }

        public TagBuilder GetForm(Webpage webpage)
        {
            TagBuilder tagBuilder = new TagBuilder("form");
            tagBuilder.Attributes["method"] = "POST";
            tagBuilder.Attributes["enctype"] = "multipart/form-data";
            tagBuilder.Attributes["action"] = string.Format("/save-form/{0}", webpage.Id);

            return tagBuilder;
        }
    }
}