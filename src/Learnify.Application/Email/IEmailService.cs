using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string to, string userName, string confirmationLink);
        Task SendOtpEmailAsync(string to, string userName, string otp, string purpose);
        
        // Payment-specific emails
        Task SendPaymentSubmittedEmailAsync(string to, string userName, decimal amount, string paymentMethod, List<string> courseNames);
        Task SendPaymentApprovedEmailAsync(string to, string userName, decimal amount, List<string> courseNames, string? adminNote);
        Task SendPaymentRejectedEmailAsync(string to, string userName, decimal amount, string? adminNote);
        
        // Welcome email after verification
        Task SendWelcomeEmailAsync(string to, string userName);
        
        // Two-Factor Authentication emails
        Task SendTwoFactorEnabledEmailAsync(string to, string userName, string method);
        Task SendTwoFactorDisabledEmailAsync(string to, string userName);
    }
}
