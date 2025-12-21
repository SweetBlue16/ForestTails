using ForestTails.Server.Data.Enums;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Logic.Utils
{
    public interface INotificationService
    {
        Task SendVerificationCodeAsync(string email, string code, CodeType type);
        Task SendWelcomeAsync(string email, string username);
    }

    public class NotificationService : INotificationService
    {
        private readonly IEmailService emailService;
        private readonly ILogger<NotificationService> logger;

        private const string HeaderColor = "#2E7D32";
        private const string BodyColor = "#F1F8E9";
        private const string Font = "font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;";

        public NotificationService(IEmailService emailService, ILogger<NotificationService> logger)
        {
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task SendVerificationCodeAsync(string email, string code, CodeType type)
        {
            string subject = type == CodeType.Registration
                ? "Verify your account - Forest Tails"
                : "Password Recovery - Forest Tails";

            string actionText = type == CodeType.Registration
                ? "to complete your registration"
                : "to reset your password";

            string body = $@"
            <div style=""{Font} background-color: {BodyColor}; padding: 20px; border-radius: 10px;"">
                <div style=""background-color: {HeaderColor}; padding: 15px; border-radius: 10px 10px 0 0; text-align: center;"">
                    <h1 style=""color: white; margin: 0;"">Forest Tails 🌲</h1>
                </div>
                <div style=""padding: 20px; background-color: white; border: 1px solid #ddd;"">
                    <h2>Verification Code</h2>
                    <p>Hi, traveler!,</p>
                    <p>Use the following code {actionText}:</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <span style=""font-size: 32px; letter-spacing: 5px; font-weight: bold; color: {HeaderColor}; border: 2px dashed {HeaderColor}; padding: 10px 20px;"">
                            {code}
                        </span>
                    </div>
                    <p>This code will expire in 15 minutes.</p>
                    <p style=""font-size: 12px; color: #888;"">If you did not request this code, please ignore this message.</p>
                </div>
            </div>";
            await emailService.SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeAsync(string email, string username)
        {
            string subject = "Welcome to Forest Tails! 🦊";
            string body = $@"
            <div style=""{Font} background-color: {BodyColor}; padding: 20px; border-radius: 10px;"">
                <div style=""background-color: {HeaderColor}; padding: 15px; border-radius: 10px 10px 0 0; text-align: center;"">
                    <h1 style=""color: white; margin: 0;"">Adventure Started!</h1>
                </div>
                <div style=""padding: 20px; background-color: white;"">
                    <h3>Hi, {username}!</h3>
                    <p>Your account has been successfully created. You can now enter the world of Forest Tails.</p>
                    <p>We've added a starter pack to your inventory:</p>
                    <ul>
                        <li>5x Health Potions</li>
                        <li>100 MichiCoins</li>
                    </ul>
                    <p>See you in the forest!</p>
                </div>
            </div>";
            await emailService.SendEmailAsync(email, subject, body);
        }
    }
}
