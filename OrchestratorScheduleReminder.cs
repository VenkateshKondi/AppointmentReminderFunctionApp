using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentReminderFunctionApp
{
	public static class OrchestratorScheduleReminder
	{
		[Function("OrchestratorScheduleReminder")]
		public static async Task Run([OrchestrationTrigger] TaskOrchestrationContext context, FunctionContext functionContext)
		{
			var log = functionContext.GetLogger("OrchestratorScheduleReminder");
			try
			{
				var reminderInfo = context.GetInput<ReminderInfo>();

				// Calculate the reminder time
				var reminderTime = reminderInfo.AppointmentDate.AddMinutes(-30);

				// Create a durable timer that expires 30 minutes before the appointment
				await context.CreateTimer(reminderTime, CancellationToken.None);
				log.LogInformation("About to send reminder email.");

				// Call the SendEmailFunction activity to send the reminder email
				await context.CallActivityAsync("SendEmailFunction", reminderInfo);

				log.LogInformation("Reminder email sent successfully.");
			}
			catch (Exception ex)
			{
				log.LogError($"Error in OrchestratorScheduleReminder: {ex.Message}");
				throw;
			}
		}
	}
}
