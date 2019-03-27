using System;
using System.Linq;
using MrCMS.Entities.Messaging;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Messages;
using MrCMS.Services;
using NHibernate;
using Ninject;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class MessageTemplatePreviewService : IMessageTemplatePreviewService
    {
        private readonly IMessageTemplateProvider _messageTemplateProvider;
        private readonly Site _site;
        private readonly IKernel _kernel;
        private readonly ISession _session;

        public MessageTemplatePreviewService(IMessageTemplateProvider messageTemplateProvider, Site site, IKernel kernel, ISession session)
        {
            _messageTemplateProvider = messageTemplateProvider;
            _site = site;
            _kernel = kernel;
            _session = session;
        }

        public MessageTemplate GetTemplate(string type)
        {
            return _messageTemplateProvider.GetAllMessageTemplates(_site).FirstOrDefault(x => x.GetType().FullName == type);
        }

        public QueuedMessage GetPreview(string type, int id)
        {
            Type templateType = TypeHelper.GetTypeByName(type);
            MessageTemplate messageTemplateBase = GetTemplate(type);
            if (messageTemplateBase == null)
                return null;
            Type modelType = messageTemplateBase.ModelType;
            if (modelType != null)
            {
                object o = _session.Get(modelType, id);
                if (o == null)
                    return null;
                Type parserType = typeof(IMessageParser<,>).MakeGenericType(templateType, modelType);
                System.Reflection.MethodInfo method = parserType.GetMethod("GetMessage");
                object parser = _kernel.Get(parserType);
                return method.Invoke(parser, new object[]
                {
                    o,null,null,null,null,null,null
                }) as QueuedMessage;
            }
            return null;
        }
    }
}