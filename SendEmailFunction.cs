using AppointmentReminderFunctionApp.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace AppointmentReminderFunctionApp
{
    public static class SendEmailFunction
    {
        [Function("SendEmailFunction")]
        public static async Task Run([ActivityTrigger] ReminderInfo reminderInfo, FunctionContext context)
        {
            var log = context.GetLogger("SendEmailFunction");

            try
            {
                var client = new SendGridClient(Environment.GetEnvironmentVariable("SendGridApiKey"));
                var from = new EmailAddress("no-reply@yourbusiness.com", "Your Business");
                var subject = "Upcoming Appointment Reminder";
                var to = new EmailAddress(reminderInfo.Email, reminderInfo.Name);
                var plainTextContent = $"Hi {reminderInfo.Name},\n\nThis is a reminder for your appointment scheduled at {reminderInfo.AppointmentDate}.";
                var htmlContent = $"<strong>Hi {reminderInfo.Name},</strong><br><br>This is a reminder for your appointment scheduled at {reminderInfo.AppointmentDate}.";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                await client.SendEmailAsync(msg);

                log.LogInformation($"Reminder email sent to {reminderInfo.Email}.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error in SendEmailFunction: {ex.Message}");
                throw;
            }
        }
    }
}
