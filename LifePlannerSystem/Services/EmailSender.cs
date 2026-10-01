using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Http.Json;

namespace LifePlanner.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailSender(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["EmailSettings:ApiKey"]
                ?? throw new InvalidOperationException("EmailSettings:ApiKey is not configured.");
            var senderEmail = _configuration["EmailSettings:SenderEmail"]
                ?? throw new InvalidOperationException("EmailSettings:SenderEmail is not configured.");
            var senderName = _configuration["EmailSettings:SenderName"] ?? "LifePlanner System";

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = JsonContent.Create(new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email } },
                subject,
                htmlContent = GetBeautifulEmailTemplate(subject, htmlMessage)
            });

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        private string GetBeautifulEmailTemplate(string title, string messageBody)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }}
                    .header {{ background-color: #0d6efd; padding: 20px; text-align: center; color: white; }}
                    .content {{ padding: 30px; color: #333; line-height: 1.6; }}
                    .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; }}
                    a {{ color: #0d6efd; text-decoration: none; font-weight: bold; }}
                    .button {{ display: inline-block; padding: 10px 20px; background-color: #0d6efd; color: white !important; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1 style='margin:0;'>LifePlanner System</h1>
                    </div>
                    <div class='content'>
                        <h2 style='color: #0d6efd;'>{title}</h2>
                        <p>{messageBody}</p>
                        <p>If you did not request this email, simply ignore it.</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} LifePlanner System. All rights reserved.<br>
                        This is an automated message, please do not reply.
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}