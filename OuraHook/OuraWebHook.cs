using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OuraHook
{
    public class EventData
    {
        [JsonPropertyName("event_type")]
        public string EventType { get; set; }

        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("object_id")]
        public string ObjectId { get; set; }

        [JsonPropertyName("event_time")]
        public DateTime EventTime { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }

    public static class OuraWebHook
    {
        private static readonly string OuraClientSecret = Environment.GetEnvironmentVariable("oura_client_secret");

        [FunctionName("OAuth")]
        public static IActionResult OAuth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // oura will send a request here when the user has authorized the app starting from this url:
            // https://cloud.ouraring.com/oauth/authorize?response_type=token&client_id=< client id >&redirect_uri=http%3A%2F%2F< host[:port] >%2Fapi%2FOAuth&scope=email+personal+daily+heartrate+workout+tag+session+spo2&state=somestate
            // with this kind of url parameters:
            // http://host/api/OAuth#access_token=<token here>&token_type=bearer&expires_in=2592000&scope=email%20personal%20daily%20heartrate%20workout%20tag%20session%20spo2&state=somestate
            // after that the host can be subscribed for events, user auth is not needed
            // see https://cloud.ouraring.com/docs/authentication

            log.LogInformation($"OAuth request received: {req.Path}");

            return new OkResult();
        }

        [FunctionName("WebHook")]
        public static async Task<IActionResult> WebHook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // uncomment to get a beep for local debugging
            // log.LogInformation($"{(char)7}");

            if (string.Compare(req.Method, "get", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                log.LogInformation("Verification request received");
                string verification_token = req.Query["verification_token"];
                string challenge = req.Query["challenge"];
                log.LogInformation($"Verification verification_token: '{verification_token}', challenge: '{challenge}'");

                return new OkObjectResult(new { challenge });
            }
            else
            {
                log.LogInformation($"Event request received");
                return await Event(req, log);
            }
        }

        static async Task<IActionResult> Event(HttpRequest req, ILogger log)
        {
            if (!req.Headers.TryGetValue("x-oura-signature", out var signature))
            {
                log.LogError("x-oura-signature header not found");
                return new OkResult();
            }

            if (!req.Headers.TryGetValue("x-oura-timestamp", out var timestamp))
            {
                log.LogError("x-oura-timestamp header not found");
                return new OkResult();
            }

            if (string.IsNullOrEmpty(OuraClientSecret))
            {
                log.LogError("oura_client_secret not configured");
                return new OkResult();
            }

            string body = await new StreamReader(req.Body).ReadToEndAsync();

            string mySignature = ComputeSignature($"{timestamp}{body}");

            if (string.Compare(mySignature, signature, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                log.LogError($"Invalid signature:\ntimestamp:{timestamp}\nbody:\n{body}\n{mySignature} !=\n{signature}");
                return new OkResult();
            }

            EventData eventData = JsonSerializer.Deserialize<EventData>(body);

            log.LogInformation($"{eventData.EventTime}: {eventData.DataType} {eventData.EventType}\n");

            return new OkResult();
        }

        static string ComputeSignature(string stringToSign)
        {
            byte[] key = Encoding.UTF8.GetBytes(OuraClientSecret);
            byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            using var hmacsha256 = new HMACSHA256(key);
            var hashedBytes = hmacsha256.ComputeHash(bytesToSign);

            string result = BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
            return result;
        }
    }
}
