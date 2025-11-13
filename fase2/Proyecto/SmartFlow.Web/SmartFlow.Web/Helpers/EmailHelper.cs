using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace SmartFlow.Web.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _config;

        public EmailHelper(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarCorreo(string destinatario, string asunto, string mensaje)
        {
            var settings = _config.GetSection("EmailSettings");
            string from = settings["From"];
            string password = settings["Password"];
            string smtp = settings["SmtpServer"];
            int port = int.Parse(settings["Port"]);

            using (var client = new SmtpClient(smtp, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from, password);

                var mail = new MailMessage
                {
                    From = new MailAddress(from, "SmartFlow - Notificaciones Automáticas"), // 👈 nombre visible
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = true
                };

                // Agregamos destinatario real
                mail.To.Add(destinatario);

                // 🔹 Opcional: agregar dirección “no-reply” para que no puedan responder
                mail.ReplyToList.Add(new MailAddress("no-reply@smartflow.com"));


                client.Send(mail);
            }
        }
    }
}
