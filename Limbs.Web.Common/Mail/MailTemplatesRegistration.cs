﻿using System.Data;
using System.IO;
using Limbs.Web.Entities.Models;

namespace Limbs.Web.Common.Mail
{
    public static class MailTemplatesRegistration
    {
        public static void Initialize()
        {
            CompiledTemplateEngine.Add<string>("Mails.EmailConfirmation", GetStringTemplate("Limbs.Web.Common.Mail.Templates.EmailConfirmation.cshtml"));
            CompiledTemplateEngine.Add<string>("Mails.EmailPasswordChange", GetStringTemplate("Limbs.Web.Common.Mail.Templates.EmailPasswordChange.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderAcceptedToAmbassador", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderAcceptedToAmbassador.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderAcceptedToRequestor", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderAcceptedToRequestor.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderReadyToAdmin", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderReadyToAdmin.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderReadyToAmbassador", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderReadyToAmbassador.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderReadyToRequestor", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderReadyToRequestor.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderNewAmbassador", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderNewAmbassador.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderNewAmbassadorToOldAmbassador", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderNewAmbassadorToOldAmbassador.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderDeliveryInformation", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderDeliveryInformation.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.OrderProofOfDeliveryInfo", GetStringTemplate("Limbs.Web.Common.Mail.Templates.OrderProofOfDeliveryInfo.cshtml"));
            CompiledTemplateEngine.Add<OrderModel>("Mails.Generic", GetStringTemplate("Limbs.Web.Common.Mail.Templates.Generic.cshtml"));
        }


        private static string GetStringTemplate(string templateFilePath)
        {
            var assembly = typeof(MailTemplatesRegistration).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(templateFilePath))
            {
                if (stream == null)
                    throw new NoNullAllowedException("No se pudo encontrar el Recurso " + templateFilePath);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
