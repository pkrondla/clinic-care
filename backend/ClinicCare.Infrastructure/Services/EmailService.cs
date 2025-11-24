using ClinicCare.Application.Common.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ClinicCare.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(to, subject, body, null, null, null, isHtml, cancellationToken);
    }

    public async Task<EmailSendResult> SendEmailAsync(
        string to,
        string subject,
        string body,
        List<string>? cc,
        List<string>? bcc,
        List<EmailAttachment>? attachments,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@cliniccare.com";
            var fromName = _configuration["Email:FromName"] ?? "ClinicCare";

            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogWarning("Email service not configured. SMTP settings missing.");
                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = "Email service not configured"
                };
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));

            if (cc != null && cc.Any())
            {
                foreach (var ccAddress in cc)
                {
                    message.Cc.Add(MailboxAddress.Parse(ccAddress));
                }
            }

            if (bcc != null && bcc.Any())
            {
                foreach (var bccAddress in bcc)
                {
                    message.Bcc.Add(MailboxAddress.Parse(bccAddress));
                }
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            if (attachments != null && attachments.Any())
            {
                foreach (var attachment in attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            
            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            }

            var response = await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}, Subject: {Subject}", to, subject);

            return new EmailSendResult
            {
                Success = true,
                MessageId = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<EmailSendResult> SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> templateData, CancellationToken cancellationToken = default)
    {
        // TODO: Implement template engine (Razor, Handlebars, etc.)
        // For now, return error
        _logger.LogWarning("Templated email not implemented. Template: {TemplateName}", templateName);
        
        return new EmailSendResult
        {
            Success = false,
            ErrorMessage = "Templated email feature not yet implemented"
        };
    }
}

