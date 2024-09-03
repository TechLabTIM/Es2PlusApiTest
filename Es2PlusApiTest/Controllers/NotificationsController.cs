using Es2PlusApiTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Es2PlusApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly HttpClientHandler handler = new HttpClientHandler();
        private readonly IConfiguration _configuration;

        public NotificationsController(IConfiguration configuration)
        {
            _configuration = configuration;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            string certificatePath = _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = _configuration["CertificateSettings:CertificatePassword"];
            handler.ClientCertificates.Add(new X509Certificate2(certificatePath, certificatePassword));
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        public class Notification
        {
            public Header Header { get; set; }
            public string Eid { get; set; }
            public string Iccid { get; set; }
            public string ProfileType { get; set; }
            public DateTime Timestamp { get; set; }
            public int NotificationPointId { get; set; }
            public NotificationPointStatus NotificationPointStatus { get; set; }
            // Include additional fields if necessary
        }


        public class NotificationPointStatus
        {
            public string Status { get; set; }
            public StatusCodeData StatusCodeData { get; set; }
            // Include additional fields if necessary
        }

        public class StatusCodeData
        {
            // Define properties according to the schema if they are present
        }

        [HttpPost("gsma/rsp2/es2plus/handleDownloadProgressInfo")]
        public IActionResult ReceiveNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest("Notification is null.");
            }

            try
            {
                // Validate notification (adjust this method to check for required fields based on the provided schema)
                if (!IsValid(notification))
                {
                    return BadRequest("Invalid notification.");
                }

                // Process the valid notification
                ProcessNotification(notification);

                // Since the expected response is HTTP 204 No Content, we return this status code
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception here as needed
                return StatusCode(500, "An error occurred while processing the notification: " + ex.Message);
            }
        }

        private bool IsValid(Notification notification)
        {
            // Expand validation logic based on your requirements
            // Here's an example of checking if some properties are not null or empty
            return notification.Header != null
                   && !string.IsNullOrEmpty(notification.Eid)
                   && !string.IsNullOrEmpty(notification.Iccid)
                   && !string.IsNullOrEmpty(notification.ProfileType)
                   && notification.NotificationPointId > 0
                   && notification.NotificationPointStatus != null;
        }

        private void ProcessNotification(Notification notification)
        {
            // Process based on notification.NotificationPointId
            switch (notification.NotificationPointId)
            {
                case 3:
                    Console.WriteLine("Download Confirmed!", notification.NotificationPointStatus);


                    break;
                case 4:
                    Console.WriteLine("Instalation Confirmed", notification.NotificationPointStatus);

                    break;

                case 100:
                    Console.WriteLine("ESIM Enabled", notification.NotificationPointStatus);
                    break;

                case 101:
                    Console.WriteLine("ESIM Disabled", notification); 
                    
                    break;

                case 102:
                    Console.WriteLine("ESIM Deleted", notification, notification.NotificationPointStatus);
                    break;
                // Add more cases for different NotificationPointIds
                default:
                    // Handle unknown NotificationPointId
                    break;
            }

            // Log the notification
            Console.WriteLine($"Received notification with ID: {notification.NotificationPointId}");
        }


        [HttpPost("downloadOrder")]
        public async Task<IActionResult> DownloadOrder([FromBody] dynamic request)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/downloadOrder";
            string certificatePath = _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = _configuration["CertificateSettings:CertificatePassword"];

            var response = await SendEs2PlusRequestAsync(url, request, certificatePath, certificatePassword);

            var downloadOrderResponse = JsonConvert.DeserializeObject<DownloadOrderResponse>(response);

            return Ok(downloadOrderResponse);
        }


        [HttpPost("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder([FromBody] dynamic request)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/confirmOrder";
            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];

            var response = await SendEs2PlusRequestAsync(url, request, certificatePath, certificatePassword);

            var confirmOrderResponse = JsonConvert.DeserializeObject<ConfirmOrderResponse>(response);

            return Ok(confirmOrderResponse);
        }


        [HttpPost("releaseProfile")]
        public async Task<IActionResult> ReleaseProfile([FromBody] dynamic request)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/releaseProfile";
            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];

            var response = await SendEs2PlusRequestAsync(url, request, certificatePath, certificatePassword);

            var releaseProfileResponse = JsonConvert.DeserializeObject<ReleaseProfileResponse>(response);

            return Ok(releaseProfileResponse);
        }


        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder([FromBody] dynamic request)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/cancelOrder";
            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];

            var response = await SendEs2PlusRequestAsync(url, request, certificatePath, certificatePassword);

            var cancelOrderResponse = JsonConvert.DeserializeObject<CancelOrderResponse>(response);

            return Ok(cancelOrderResponse);
        }




        private async Task<string> SendEs2PlusRequestAsync(string url, object payload, string certificatePath, string certificatePassword)
        {
            StringContent contentJson;

            try
            {
                string jsonString;
                if (payload is JsonElement jsonElement)
                {
                    jsonString = jsonElement.GetRawText();
                }
                else
                {
                    jsonString = JsonConvert.SerializeObject(payload);
                }

                Console.WriteLine($"Serialized JSON Payload: {jsonString}");  // Log para diagnóstico
                contentJson = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("X-Admin-Protocol", "gsma/rsp/v2.2.0");
                    client.DefaultRequestHeaders.Add("User-Agent", "gsma-rsp-Ipad");
                    client.DefaultRequestHeaders.Add("functionRequesterIdentifier", "2");
                    client.DefaultRequestHeaders.Add("functionCallIdentifier", "TX-567");

                    var response = await client.PostAsync(url, contentJson);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        return responseContent;
                    }
                    else
                    {
                        //return the error that caused not to be successed
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during serialization or HTTP request: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return ("An error occurred while sending the request: " + ex.Message);
            }
        }

    }
}
