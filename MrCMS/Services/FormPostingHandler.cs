using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Entities.Messaging;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Settings;
using MrCMS.Shortcodes.Forms;
using NHibernate;

namespace MrCMS.Services
{
    public class FormPostingHandler : IFormPostingHandler
    {
        private readonly IDocumentService _documentService;
        private readonly IFileService _fileService;
        private readonly MailSettings _mailSettings;
        private readonly ISession _session;

        public FormPostingHandler(IDocumentService documentService, IFileService fileService, MailSettings mailSettings, ISession session)
        {
            _documentService = documentService;
            _session = session;
            _fileService = fileService;
            _mailSettings = mailSettings;
        }

        public List<string> SaveFormData(Webpage webpage, HttpRequestBase request)
        {
            IList<FormProperty> formProperties = webpage.FormProperties;

            FormPosting formPosting = new FormPosting { Webpage = webpage };
            _session.Transact(session =>
            {
                webpage.FormPostings.Add(formPosting);
                session.SaveOrUpdate(formPosting);
            });
            List<string> errors = new List<string>();
            _session.Transact(session =>
            {
                foreach (FormProperty formProperty in formProperties)
                {
                    try
                    {
                        if (formProperty is FileUpload)
                        {
                            HttpPostedFileBase file = request.Files[formProperty.Name];

                            if (file == null && formProperty.Required)
                                throw new RequiredFieldException("No file was attached to the " +
                                                                 formProperty.Name + " field");

                            if (file != null && !string.IsNullOrWhiteSpace(file.FileName))
                            {
                                string value = SaveFile(webpage, formPosting, file);

                                formPosting.FormValues.Add(new FormValue
                                {
                                    Key = formProperty.Name,
                                    Value = value,
                                    IsFile = true,
                                    FormPosting = formPosting
                                });
                            }
                        }
                        else
                        {
                            string value = SanitizeValue(formProperty, request.Form[formProperty.Name]);

                            if (string.IsNullOrWhiteSpace(value) && formProperty.Required)
                                throw new RequiredFieldException("No value was posted for the " +
                                                                 formProperty.Name + " field");

                            formPosting.FormValues.Add(new FormValue
                            {
                                Key = formProperty.Name,
                                Value = value,
                                FormPosting = formPosting
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }

                if (errors.Any())
                {
                    session.Delete(formPosting);
                }
                else
                {
                    formPosting.FormValues.ForEach(value => session.Save(value));

                    SendFormMessages(webpage, formPosting);
                }
            });
            return errors;
        }
        private MediaCategory CreateFileUploadMediaCategory()
        {
            MediaCategory mediaCategory = new MediaCategory { UrlSegment = "file-uploads", Name = "File Uploads" };
            _documentService.AddDocument(mediaCategory);
            return mediaCategory;
        }
        private string SanitizeValue(FormProperty formProperty, string value)
        {
            if (formProperty is CheckboxList)
            {
                if (value != null)
                {
                    List<string> list = value.Split(',').ToList();
                    list.Remove(CheckBoxListRenderer.CbHiddenValue);
                    return !list.Any() ? null : string.Join(",", list);
                }
                return value;
            }
            return value;
        }

        private string SaveFile(Webpage webpage, FormPosting formPosting, HttpPostedFileBase file)
        {
            MediaCategory mediaCategory = _documentService.GetDocumentByUrl<MediaCategory>("file-uploads") ??
                                CreateFileUploadMediaCategory();

            MediaFile result = _fileService.AddFile(file.InputStream, webpage.Id + "-" + formPosting.Id + "-" + file.FileName, file.ContentType, file.ContentLength, mediaCategory);

            return result.FileUrl;
        }
        private void SendFormMessages(Webpage webpage, FormPosting formPosting)
        {
            if (webpage.SendFormTo == null) return;

            string[] sendTo = webpage.SendFormTo.Split(',');
            if (sendTo.Any())
            {
                _session.Transact(session =>
                {
                    foreach (string email in sendTo)
                    {
                        string formMessage = ParseFormMessage(webpage.FormMessage, webpage,
                            formPosting);
                        string formTitle = ParseFormMessage(webpage.FormEmailTitle, webpage,
                            formPosting);

                        session.SaveOrUpdate(new QueuedMessage
                        {
                            Subject = formTitle,
                            Body = formMessage,
                            FromAddress = _mailSettings.SystemEmailAddress,
                            ToAddress = email,
                            IsHtml = true
                        });
                    }
                });
            }
        }

        private static string ParseFormMessage(string formMessage, Webpage webpage, FormPosting formPosting)
        {

            Regex formRegex = new Regex(@"\[form\]");
            Regex pageRegex = new Regex(@"{{page.(.*)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex messageRegex = new Regex(@"{{(.*)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            formMessage = formRegex.Replace(formMessage, match =>
            {
                TagBuilder list = new TagBuilder("ul");

                foreach (FormValue formValue in formPosting.FormValues)
                {
                    TagBuilder listItem = new TagBuilder("li");

                    TagBuilder title = new TagBuilder("b");
                    title.InnerHtml += formValue.Key + ":";
                    listItem.InnerHtml += title.ToString() + " " +
                                          formValue.GetMessageValue();

                    list.InnerHtml += listItem.ToString();
                }

                return list.ToString();
            });

            formMessage = pageRegex.Replace(formMessage, match =>
            {
                System.Reflection.PropertyInfo propertyInfo =
                    typeof(Webpage).GetProperties().FirstOrDefault(
                        info =>
                            info.Name.Equals(match.Value.Replace("{", "").Replace("}", "").Replace("page.", ""),
                                StringComparison.OrdinalIgnoreCase));

                return propertyInfo == null
                    ? string.Empty
                    : propertyInfo.GetValue(webpage,
                        null).
                        ToString();
            });
            return messageRegex.Replace(formMessage, match =>
            {
                FormValue formValue =
                    formPosting.FormValues.FirstOrDefault(
                        value =>
                            value.Key.Equals(
                                match.Value.Replace("{", "").Replace("}", ""),
                                StringComparison.
                                    OrdinalIgnoreCase));
                return formValue == null
                    ? string.Empty
                    : formValue.GetMessageValue();
            });
        }
    }
}