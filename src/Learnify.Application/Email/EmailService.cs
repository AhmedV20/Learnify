using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Learnify.Application.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        // Brand colors matching Learnify frontend
        private const string PrimaryColor = "#16a34a";
        private const string DangerColor = "#dc2626";
        private const string WarningColor = "#f59e0b";
        private const string TextColor = "#0B100F";
        private const string MutedTextColor = "#808080";

        // Base email template - clean Windsurf-inspired design
        private string GetBaseTemplate(string title, string content)
        {
            var baseUrl = _configuration["App:BaseUrl"] ?? "https://learnify.com";
            var logoUrl = $"https://res.cloudinary.com/dvfhan4lw/image/upload/v1766931549/Logo_ahzjcq.png";
            
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{title}</title>
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;800&display=swap"" rel=""stylesheet"">
    <style>
        body {{
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            background-color: #ffffff;
            color: {TextColor};
            margin: 0;
            padding: 0;
            line-height: 1.6;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 32px auto;
            padding: 40px 24px;
            text-align: left;
            background-color: #ffffff;
        }}
        .logo {{
            font-size: 28px;
            font-weight: 700;
            color: {PrimaryColor};
            margin-bottom: 16px;
        }}
        .divider {{
            height: 1px;
            background-color: #e5e5e5;
            margin: 24px 0;
        }}
        h2 {{
            font-weight: 600;
            font-size: 28px;
            margin-bottom: 16px;
            color: {TextColor};
            letter-spacing: -0.5px;
        }}
        p {{
            font-size: 16px;
            color: {TextColor};
            margin: 12px 0;
            line-height: 150%;
        }}
        .code {{
            font-size: 32px;
            font-weight: 800;
            color: {PrimaryColor};
            letter-spacing: 4px;
            margin: 24px 0;
            text-align: center;
        }}
        .button {{
            display: inline-block;
            background-color: {PrimaryColor};
            color: #ffffff !important;
            padding: 14px 28px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            font-size: 16px;
            margin: 16px 0;
        }}
        .amount {{
            font-size: 36px;
            font-weight: 800;
            color: {PrimaryColor};
            text-align: center;
            margin: 16px 0;
        }}
        .status-approved {{
            display: inline-block;
            background-color: {PrimaryColor};
            color: #ffffff;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: 600;
            font-size: 14px;
        }}
        .status-pending {{
            display: inline-block;
            background-color: {WarningColor};
            color: #ffffff;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: 600;
            font-size: 14px;
        }}
        .status-rejected {{
            display: inline-block;
            background-color: {DangerColor};
            color: #ffffff;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: 600;
            font-size: 14px;
        }}
        .course-item {{
            padding: 12px 0;
            border-bottom: 1px solid #f0f0f0;
        }}
        .subtext {{
            font-size: 14px;
            color: {MutedTextColor};
            margin-top: 12px;
        }}
        .caption {{
            font-size: 12px;
            color: {MutedTextColor};
        }}
        .footer {{
            margin-top: 40px;
        }}
        .info-box {{
            background-color: #f9fafb;
            border-radius: 8px;
            padding: 16px;
            margin: 16px 0;
        }}
        .logo-img {{
            width: 180px;
            max-width: 100%;
            height: auto;
            object-fit: contain;
            margin-bottom: 16px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <img src=""{logoUrl}"" alt=""Learnify"" class=""logo-img"">
        <div class=""divider""></div>
        {content}
        <div class=""divider""></div>
        <div class=""footer"">
            <p class=""caption"">2024 Learnify. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUser = _configuration["Smtp:Username"] ?? throw new NullReferenceException ("SMTP username is not configured.");
            var smtpPass = _configuration["Smtp:Password"] ?? throw new NullReferenceException ("SMTP password is not configured.");
            var fromEmail = _configuration["Smtp:FromEmail"] ?? smtpUser;
            var fromName = _configuration["Smtp:FromName"] ?? "Learnify";

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendOtpEmailAsync(string to, string userName, string otp, string purpose)
        {
            var subject = $"{purpose} - Learnify";
            
            var content = $@"
        <h2>Verify your email address</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>To complete your {purpose.ToLower()}, please enter the verification code below:</p>
        <div class=""code"">{otp}</div>
        <p class=""subtext"">This code expires in 10 minutes.</p>
        <p class=""subtext"">If you didn't request this code, you can safely ignore this email.</p>";

            var body = GetBaseTemplate($"{purpose} - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendVerificationEmailAsync(string to, string userName, string confirmationLink)
        {
            var subject = "Verify Your Email - Learnify";
            
            var content = $@"
        <h2>Welcome to Learnify!</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>Thank you for registering. To complete your registration and start learning, please verify your email address.</p>
        <p style=""text-align: center;"">
            <a href=""{confirmationLink}"" class=""button"">Verify Email Address</a>
        </p>
        <p class=""subtext"">This link will expire in 1 hour.</p>
        <p class=""subtext"">If you didn't create an account, you can safely ignore this email.</p>";

            var body = GetBaseTemplate("Verify Your Email - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        #region Payment Emails

        public async Task SendPaymentSubmittedEmailAsync(string to, string userName, decimal amount, string paymentMethod, List<string> courseNames)
        {
            var subject = "Payment Received - Under Review | Learnify";
            var courseList = string.Join("", courseNames.Select(c => $@"<div class=""course-item"">{c}</div>"));
            
            var content = $@"
        <h2>Payment Received</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>We've received your payment and it's currently under review.</p>
        
        <p style=""text-align: center;""><span class=""status-pending"">Pending Review</span></p>
        
        <div class=""amount"">${amount:F2}</div>
        <p style=""text-align: center;"" class=""subtext"">via {paymentMethod}</p>
        
        <div class=""info-box"">
            <p style=""margin: 0 0 8px 0;""><strong>Courses in your order:</strong></p>
            {courseList}
        </div>
        
        <p class=""subtext"">Our team will verify your payment within 24-48 hours. You'll receive an email once approved and be automatically enrolled in your courses.</p>";

            var body = GetBaseTemplate("Payment Received - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPaymentApprovedEmailAsync(string to, string userName, decimal amount, List<string> courseNames, string? adminNote)
        {
            var subject = "Payment Approved - Start Learning! | Learnify";
            var courseList = string.Join("", courseNames.Select(c => $@"<div class=""course-item"">{c}</div>"));
            var adminNoteHtml = !string.IsNullOrEmpty(adminNote) 
                ? $@"<div class=""info-box""><p style=""margin: 0;""><strong>Note:</strong> {adminNote}</p></div>" 
                : "";

            var content = $@"
        <h2>Payment Approved!</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>Great news! Your payment has been verified and approved.</p>
        
        <p style=""text-align: center;""><span class=""status-approved"">Approved</span></p>
        
        <div class=""amount"">${amount:F2}</div>
        
        <div class=""info-box"">
            <p style=""margin: 0 0 8px 0;""><strong>You're now enrolled in:</strong></p>
            {courseList}
        </div>
        
        {adminNoteHtml}
        
        <p style=""text-align: center;"">
            <a href=""#"" class=""button"">Start Learning</a>
        </p>
        
        <p class=""subtext"" style=""text-align: center;"">Log in to your account and begin your learning journey!</p>";

            var body = GetBaseTemplate("Payment Approved - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPaymentRejectedEmailAsync(string to, string userName, decimal amount, string? adminNote)
        {
            var subject = "Payment Review Update | Learnify";
            var reasonHtml = !string.IsNullOrEmpty(adminNote) 
                ? $@"<div class=""info-box""><p style=""margin: 0;""><strong>Reason:</strong> {adminNote}</p></div>" 
                : "";

            var content = $@"
        <h2>Payment Review Update</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>We've reviewed your payment submission.</p>
        
        <p style=""text-align: center;""><span class=""status-rejected"">Payment Not Verified</span></p>
        
        <div class=""amount"" style=""color: {MutedTextColor};"">${amount:F2}</div>
        
        <p>Unfortunately, we were unable to verify your payment at this time.</p>
        
        {reasonHtml}
        
        <div class=""info-box"">
            <p style=""margin: 0 0 8px 0;""><strong>What you can do:</strong></p>
            <p style=""margin: 4px 0;"">- Double-check your payment details</p>
            <p style=""margin: 4px 0;"">- Ensure the screenshot clearly shows the transaction</p>
            <p style=""margin: 4px 0;"">- Make sure the amount matches your order total</p>
            <p style=""margin: 4px 0;"">- Try submitting again with clearer proof</p>
        </div>
        
        <p class=""subtext"">If you believe this is an error or need assistance, please contact our support team.</p>";

            var body = GetBaseTemplate("Payment Review Update - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        #endregion

        #region Welcome Email

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to Learnify! 🎓";
            
            var content = $@"
        <h2>Welcome to Learnify!</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>Congratulations! Your email has been verified and your account is now active.</p>
        
        <div class=""info-box"">
            <p style=""margin: 0 0 8px 0;""><strong>What you can do now:</strong></p>
            <p style=""margin: 4px 0;"">- Browse our extensive course catalog</p>
            <p style=""margin: 4px 0;"">- Enroll in courses that interest you</p>
            <p style=""margin: 4px 0;"">- Track your learning progress</p>
            <p style=""margin: 4px 0;"">- Earn certificates upon completion</p>
        </div>
        
        <p style=""text-align: center;"">
            <a href=""#"" class=""button"">Start Learning</a>
        </p>
        
        <p class=""subtext"" style=""text-align: center;"">We're excited to have you on this learning journey!</p>";

            var body = GetBaseTemplate("Welcome to Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        #endregion

        #region Two-Factor Authentication Emails

        public async Task SendTwoFactorEnabledEmailAsync(string to, string userName, string method)
        {
            var subject = "Two-Factor Authentication Enabled 🔐 - Learnify";
            
            // Method-specific icon and description
            var (icon, methodName, description) = method.ToLower() switch
            {
                "authenticator" => (
                    "🔑", 
                    "Authenticator App",
                    "You'll need to enter a code from your authenticator app (like Google Authenticator or Authy) each time you sign in."
                ),
                _ => (
                    "📧", 
                    "Email",
                    "You'll receive a verification code via email each time you sign in."
                )
            };
            
            var content = $@"
        <h2>Two-Factor Authentication Enabled</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>Great news! Two-factor authentication has been successfully enabled on your Learnify account.</p>
        
        <div style=""text-align: center; margin: 32px 0;"">
            <div style=""display: inline-block; width: 100px; height: 100px; background: linear-gradient(135deg, {PrimaryColor}, #22c55e); border-radius: 50%; line-height: 100px; font-size: 48px;"">
                {icon}
            </div>
        </div>
        
        <div class=""info-box"">
            <p style=""margin: 0 0 8px 0; font-size: 18px;""><strong>Method: {methodName}</strong></p>
            <p style=""margin: 0; color: {MutedTextColor};"">{description}</p>
        </div>
        
        <div class=""info-box"" style=""background-color: #fef3c7; border-color: #f59e0b;"">
            <p style=""margin: 0 0 8px 0;""><strong>🔒 Security Tips:</strong></p>
            <p style=""margin: 4px 0; color: {MutedTextColor};"">• Generate backup codes in your security settings</p>
            <p style=""margin: 4px 0; color: {MutedTextColor};"">• Keep your backup codes in a safe place</p>
            <p style=""margin: 4px 0; color: {MutedTextColor};"">• Never share your verification codes with anyone</p>
        </div>
        
        <p class=""subtext"">If you didn't make this change, please secure your account immediately by changing your password.</p>";

            var body = GetBaseTemplate("Two-Factor Authentication Enabled - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendTwoFactorDisabledEmailAsync(string to, string userName)
        {
            var subject = "Two-Factor Authentication Disabled ⚠️ - Learnify";
            
            var content = $@"
        <h2>Two-Factor Authentication Disabled</h2>
        <p>Hello <strong>{userName}</strong>,</p>
        <p>Two-factor authentication has been disabled on your Learnify account.</p>
        
        <div style=""text-align: center; margin: 32px 0;"">
            <div style=""display: inline-block; width: 100px; height: 100px; background: linear-gradient(135deg, {WarningColor}, #fbbf24); border-radius: 50%; line-height: 100px; font-size: 48px;"">
                ⚠️
            </div>
        </div>
        
        <div class=""info-box"" style=""background-color: #fef2f2; border-color: {DangerColor};"">
            <p style=""margin: 0 0 8px 0;""><strong>⚡ Important Security Notice</strong></p>
            <p style=""margin: 0; color: {MutedTextColor};"">Your account is now protected only by your password. We strongly recommend enabling two-factor authentication to keep your account secure.</p>
        </div>
        
        <p style=""text-align: center;"">
            <a href=""#"" class=""button"">Re-enable 2FA</a>
        </p>
        
        <p class=""subtext"">If you didn't make this change, please secure your account immediately by changing your password and re-enabling 2FA.</p>";

            var body = GetBaseTemplate("Two-Factor Authentication Disabled - Learnify", content);
            await SendEmailAsync(to, subject, body);
        }

        #endregion
    }
}
