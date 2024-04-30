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

        public class Header
        {
            public string FunctionRequesterIdentifier { get; set; }
            public string FunctionCallIdentifier { get; set; }
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

        [HttpPost("gsma/rsp2/es2/handleDownloadProgressInfo")]
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
                case 3: // Handle Download Progress Info
                        // Business logic for download progress info
                    break;
                case 4: // Handle Installation
                        // Business logic for installation
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
        public async Task<IActionResult> DownloadOrder([FromBody] dynamic content)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/downloadOrder";
            string certificatePath = _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = _configuration["CertificateSettings:CertificatePassword"];

            return await SendEs2PlusRequestAsync(url, content, certificatePath, certificatePassword);
        }

        [HttpPost("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder([FromBody] dynamic content)
        {
            // Replace with the correct URL for the confirmOrder endpoint
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/confirmOrder";
            // ... use the shared SendEs2PlusRequestAsync method
            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];


            return await SendEs2PlusRequestAsync(url, content, certificatePath, certificatePassword);
        }

        [HttpPost("releaseProfile")]
        public async Task<IActionResult> ReleaseProfile([FromBody] dynamic content)
        {
            // Replace with the correct URL for the releaseProfile endpoint
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/releaseProfile";
            // ... use the shared SendEs2PlusRequestAsync method

            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];




            return await SendEs2PlusRequestAsync(url, content, certificatePath, certificatePassword);
        }

        [HttpPost("cancelOrder")]
        public async Task<IActionResult> CancelOrder([FromBody] dynamic content)
        {
            // Replace with the correct URL for the cancelOrder endpoint
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/cancelOrder";
            // ... use the shared SendEs2PlusRequestAsync method
            string certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH") ?? _configuration["CertificateSettings:CertificatePath"];
            string certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD") ?? _configuration["CertificateSettings:CertificatePassword"];


            return await SendEs2PlusRequestAsync(url, content, certificatePath, certificatePassword);
        }



        private async Task<IActionResult> SendEs2PlusRequestAsync(string url, object payload, string certificatePath, string certificatePassword)
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
                    var response = await client.PostAsync(url, contentJson);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return StatusCode((int)response.StatusCode, responseContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during serialization or HTTP request: {ex.Message}");
                return StatusCode(500, $"Error in sending request: {ex.Message}");
            }
        }






    }
}
