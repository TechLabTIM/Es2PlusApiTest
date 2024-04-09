using Es2PlusApiTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Es2PlusApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly string certificatePath = @"C:\projetos\tim\timcert_new.pfx";
        private readonly string certificatePassword = "Claryca236566@?@";
        private readonly HttpClientHandler handler;
        public NotificationsController()
        {
            handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(new X509Certificate2(certificatePath, certificatePassword));
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
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

        [HttpPost("receive")]
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


        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Object content)
        {
            string url = "https://valides2plus.validereachdpplus.com:8445/gsma/rsp2/es2plus/downloadOrder";
            string certificatePath = @"C:\projetos\tim\timcert_new.pfx";
            string certificatePassword = "Claryca236566@?@";
            //string certificatePassword = "password";

            var contentJson = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

            try
{
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ClientCertificates.Add(new X509Certificate2(certificatePath, certificatePassword));

                    // This is a security risk in production. It's bypassing certificate validation. Use only for testing.
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        // Replicate headers from Postman if needed
                        var requestContent = new StringContent(
                            JsonConvert.SerializeObject(new
                            {
                                header = new
                                {
                                    functionRequesterIdentifier = "2",
                                    functionCallIdentifier = "TX-567"
                                },
                                iccid = "89550399000185000335"
                            }),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await client.PostAsync(url, requestContent);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            return Ok(responseContent);
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                            Console.WriteLine($"Content: {responseContent}");
                            return StatusCode((int)response.StatusCode, responseContent);
                        }
                    }
                }


            }
            catch (Exception ex)
{
                Console.WriteLine(ex.ToString()); // Or use your preferred logging mechanism
                return StatusCode(500, "An error occurred while sending the notification.");
            }

        }



    }
}
