using FluentEmail.Core;
using Communications.Application.Common.Interfaces;
using Communications.Application.Notifications.Email;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Communications.Infrastructure.Notifications.Email
{
    public class EmailNotificationService : IEmailNotification
    {
        private const string TemplatePath = "Communications.Infrastructure.Notifications.Email.Templates.{0}.cshtml";
        private readonly IFluentEmail _email;

        public EmailNotificationService(IFluentEmail email)
        {
            _email = email;
        }

        public async Task SendEmailAsync(EmailMessage message, EmailTemplates template)
        {
            await _email
                .To(emailAddress: message.To)
                .Subject(subject: message.Subject)
                .UsingTemplateFromEmbedded(string.Format(TemplatePath, template), ToExpando(message.Model), GetType().Assembly)
                .SendAsync();
        }

        private static ExpandoObject ToExpando(object model)
        {
            if (model is ExpandoObject exp)
            {
                return exp;
            }

            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var propertyDescriptor in model.GetType().GetTypeInfo().GetProperties())
            {
                var obj = propertyDescriptor.GetValue(model);

                if (obj != null && IsAnonymousType(obj.GetType()))
                {
                    obj = ToExpando(obj);
                }

                expando.Add(propertyDescriptor.Name, obj);
            }

            return (ExpandoObject)expando;
        }

        private static bool IsAnonymousType(Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetTypeInfo()
                .GetCustomAttributes(typeof(CompilerGeneratedAttribute), false)
                .Any();

            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}