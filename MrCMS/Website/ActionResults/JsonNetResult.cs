using System;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace MrCMS.Website.ActionResults
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.DenyGet;
        }

        public string JsonData { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType)
                                       ? ContentType
                                       : "application/json";
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (!string.IsNullOrWhiteSpace(JsonData))
            {
                response.Write(JsonData);
                return;
            }
            if (Data == null) return;

            string serializedData = JsonConvert.SerializeObject(Data);
            response.Write(serializedData);
        }
    }
}