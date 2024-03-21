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
        [HttpPost("receive")]
        public IActionResult ReceiveNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest("Notification is null.");
            }

            try
            {
                // Validate notification
                if (!IsValid(notification))
                {
                    return BadRequest("Invalid notification.");
                }

                // Process the valid notification
                ProcessNotification(notification);

                // Return success response
                return Ok("Notification processed successfully.");
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "An error occurred while processing the notification.");
            }
        }

        private bool IsValid(Notification notification)
        {
            return !string.IsNullOrEmpty(notification.Id) && !string.IsNullOrEmpty(notification.Type);
        }
        private void ProcessNotification(Notification notification)
        {
            // Example processing: log message or perform action based on type
            Console.WriteLine($"Processing notification: {notification.Message}");   
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
