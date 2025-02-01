using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using AppointmentReminderFunctionApp.Models;

namespace AppointmentReminderFunctionApp
{
    public static class CancelScheduleReminder
    {
        [Function("CancelScheduleReminder")]
        public static async Task<HttpResponseData> HttpCancel(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext
        )
        {
            var log = executionContext.GetLogger("CancelScheduleReminder");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                // Deserialize the cancel request
                var cancelRequest = JsonConvert.DeserializeObject<CancelRequest>(requestBody);
                if (cancelRequest == null || string.IsNullOrEmpty(cancelRequest.InstanceId))
                {
                    throw new ArgumentNullException(
                        nameof(cancelRequest),
                        "Invalid cancel request provided."
                    );
                }

                // Fetch the orchestration instance directly using the instance ID
                var instance = await client.GetInstanceAsync(
                    cancelRequest.InstanceId,
                    CancellationToken.None
                );

                if (
                    instance != null
                    && instance.RuntimeStatus == OrchestrationRuntimeStatus.Running
                )
                {
                    // Terminate the orchestration
                    await client.TerminateInstanceAsync(
                        cancelRequest.InstanceId,
                        "User requested cancellation"
                    );
                    log.LogInformation(
                        $"Terminated orchestration with ID = '{cancelRequest.InstanceId}'."
                    );

                    var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await response.WriteStringAsync("Reminder cancelled successfully.");
                    return response;
                }
                else
                {
                    log.LogWarning(
                        $"No running orchestration found for instance ID = '{cancelRequest.InstanceId}'."
                    );

                    var response = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await response.WriteStringAsync(
                        "No running reminder found for the given instance ID."
                    );
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error cancelling reminder: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Failed to cancel reminder.");
                return errorResponse;
            }
        }
    }
}
