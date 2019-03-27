using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services.ImportExport.DTOs;
using MrCMS.Services.ImportExport.Rules;
using MrCMS.Website;
using OfficeOpenXml;

namespace MrCMS.Services.ImportExport
{
    public class ImportDocumentsValidationService : IImportDocumentsValidationService
    {
        private readonly IWebpageUrlService _webpageUrlService;

        public ImportDocumentsValidationService(IWebpageUrlService webpageUrlService)
        {
            _webpageUrlService = webpageUrlService;
        }

        /// <summary>
        /// Validate Business Logic
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> ValidateBusinessLogic(IEnumerable<DocumentImportDTO> items)
        {
            Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
            IEnumerable<IDocumentImportValidationRule> itemRules = MrCMSApplication.GetAll<IDocumentImportValidationRule>();

            IList<DocumentImportDTO> documentImportDataTransferObjects = items as IList<DocumentImportDTO> ?? items.ToList();
            foreach (DocumentImportDTO item in documentImportDataTransferObjects)
            {
                List<string> validationErrors = itemRules.SelectMany(rule => rule.GetErrors(item, documentImportDataTransferObjects)).ToList();
                if (validationErrors.Any())
                    errors.Add(item.UrlSegment, validationErrors);
            }

            return errors;
        }

        /// <summary>
        /// Parse and Import to DTOs
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        public List<DocumentImportDTO> ValidateAndImportDocuments(ExcelPackage spreadsheet, ref Dictionary<string, List<string>> parseErrors)
        {
            List<DocumentImportDTO> items = new List<DocumentImportDTO>();

            if (spreadsheet != null && spreadsheet.Workbook != null)
            {
                ExcelWorksheet worksheet = spreadsheet.Workbook.Worksheets.SingleOrDefault(x => x.Name == "Items");
                if (worksheet == null)
                {
                    return items;
                }
                int totalRows = worksheet.Dimension.End.Row;
                for (int rowId = 2; rowId <= totalRows; rowId++)
                {
                    //Prepare handle name for storing and grouping errors
                    string urlSegment = worksheet.GetValue<string>(rowId, 1);
                    string name = worksheet.GetValue<string>(rowId, 4);
                    string handle = urlSegment.HasValue() ? urlSegment : name;

                    if (string.IsNullOrWhiteSpace(handle) || items.Any(x => x.UrlSegment == urlSegment))
                        continue;

                    List<string> errors = parseErrors.ContainsKey(handle)
                        ? parseErrors[handle]
                        : new List<string>();

                    DocumentImportDTO item = GetDocumentImportDataTransferObject(worksheet, rowId, name, ref errors);
                    parseErrors[handle] = errors;

                    items.Add(item);
                }

                //Remove duplicate errors
                parseErrors = parseErrors.GroupBy(x => x.Value)
                    .Select(x => x.First())
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            //Remove handles with no errors
            parseErrors = parseErrors.Where(x => x.Value.Any()).ToDictionary(pair => pair.Key, pair => pair.Value);

            return items;
        }

        private DocumentImportDTO GetDocumentImportDataTransferObject(ExcelWorksheet worksheet, int rowId,
                                                                                     string name, ref List<string> parseErrors)
        {
            DocumentImportDTO item = new DocumentImportDTO();
            item.ParentUrl = worksheet.GetValue<string>(rowId, 2);
            if (worksheet.GetValue<string>(rowId, 3).HasValue())
            {
                item.DocumentType = worksheet.GetValue<string>(rowId, 3);
                item.UrlSegment = worksheet.GetValue<string>(rowId, 1).HasValue()
                    ? worksheet.GetValue<string>(rowId, 1)
                    : _webpageUrlService.Suggest(null,
                        new SuggestParams { PageName = name, DocumentType = item.DocumentType });
            }
            else
                parseErrors.Add("Document Type is required.");
            if (worksheet.GetValue<string>(rowId, 4).HasValue())
                item.Name = worksheet.GetValue<string>(rowId, 4);
            else
                parseErrors.Add("Document Name is required.");
            item.BodyContent = worksheet.GetValue<string>(rowId, 5);
            item.MetaTitle = worksheet.GetValue<string>(rowId, 6);
            item.MetaDescription = worksheet.GetValue<string>(rowId, 7);
            item.MetaKeywords = worksheet.GetValue<string>(rowId, 8);
            item.Tags = GetTags(worksheet, rowId, parseErrors);
            if (worksheet.GetValue<string>(rowId, 10).HasValue())
            {
                if (!worksheet.GetValue<string>(rowId, 10).IsValidInput<bool>())
                    parseErrors.Add("Reveal in Navigation is not a valid boolean value.");
                else
                    item.RevealInNavigation = worksheet.GetValue<bool>(rowId, 10);
            }
            else
                item.RevealInNavigation = false;

            if (worksheet.GetValue<string>(rowId, 11).HasValue())
            {
                if (!worksheet.GetValue<string>(rowId, 11).IsValidInput<int>())
                    parseErrors.Add("Display Order is not a valid number.");
                else
                    item.DisplayOrder = worksheet.GetValue<int>(rowId, 11);
            }
            else
                item.DisplayOrder = 0;

            if (worksheet.GetValue<string>(rowId, 12).HasValue())
            {
                if (!worksheet.GetValue<string>(rowId, 12).IsValidInput<bool>())
                    parseErrors.Add("Require SSL is not a valid boolean value.");
                else
                    item.RequireSSL = worksheet.GetValue<bool>(rowId, 12);
            }
            else
                item.RequireSSL = false;


            if (worksheet.GetValue<string>(rowId, 13).HasValue())
            {
                if (!worksheet.GetValue<string>(rowId, 13).IsValidInputDateTime())
                    parseErrors.Add("Publish Date is not a valid date.");
                else
                    item.PublishDate = worksheet.GetValue<DateTime>(rowId, 13);
            }

            item.UrlHistory = GetUrlHistory(worksheet, rowId, parseErrors);
            return item;
        }

        private static List<string> GetUrlHistory(ExcelWorksheet worksheet, int rowId, List<string> parseErrors)
        {
            List<string> list = new List<string>();
            try
            {
                string value = worksheet.GetValue<string>(rowId, 14);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string[] urls = value.Split(',');
                    foreach (string url in urls.Where(url => !string.IsNullOrWhiteSpace(url)))
                    {
                        list.Add(url);
                    }
                }
            }
            catch (Exception)
            {
                parseErrors.Add("Url History field value contains illegal characters / not in correct format.");
            }
            return list;
        }

        private static List<string> GetTags(ExcelWorksheet worksheet, int rowId, List<string> parseErrors)
        {
            List<string> tagList = new List<string>();
            try
            {
                string value = worksheet.GetValue<string>(rowId, 9);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string[] tags = value.Split(',');
                    foreach (string tag in tags.Where(tag => !string.IsNullOrWhiteSpace(tag)))
                    {
                        tagList.Add(tag);
                    }
                }
            }
            catch (Exception)
            {
                parseErrors.Add(
                    "Url History field value contains illegal characters / not in correct format.");
            }
            return tagList;
        }

        /// <summary>
        /// Validate Import File
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> ValidateImportFile(ExcelPackage spreadsheet)
        {
            Dictionary<string, List<string>> parseErrors = new Dictionary<string, List<string>> { { "file", new List<string>() } };

            if (spreadsheet == null)
                parseErrors["file"].Add("No import file");
            else
            {
                if (spreadsheet.Workbook == null)
                    parseErrors["file"].Add("Error reading Workbook from import file.");
                else
                {
                    if (spreadsheet.Workbook.Worksheets.Count == 0)
                        parseErrors["file"].Add("No worksheets in import file.");
                    else
                    {
                        if (spreadsheet.Workbook.Worksheets.Count < 2 ||
                            !spreadsheet.Workbook.Worksheets.Any(x => x.Name == "Info") ||
                             !spreadsheet.Workbook.Worksheets.Any(x => x.Name == "Items"))
                            parseErrors["file"].Add(
                                "One or both of the required worksheets (Info and Items) are not present in import file.");
                    }
                }
            }

            return parseErrors.Where(x => x.Value.Any()).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}