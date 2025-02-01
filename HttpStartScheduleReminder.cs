using AppointmentReminderFunctionApp.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace AppointmentReminderFunctionApp
{
    public static class HttpStartScheduleReminder
    {
        [Function("HttpStartScheduleReminder")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext,
            [DurableClient] DurableTaskClient starter)
        {
            var log = executionContext.GetLogger("HttpStartScheduleReminder");
            log.LogInformation("HTTP trigger function processed a request.");

            // Parse the request body to get the reminder info
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var reminderInfo = JsonConvert.DeserializeObject<ReminderInfo>(requestBody);

            // Start the orchestration
            string instanceId = await starter.ScheduleNewOrchestrationInstanceAsync("OrchestratorScheduleReminder", reminderInfo);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Started orchestration with ID = '{instanceId}'.");

            return response;
        }
    }
}
