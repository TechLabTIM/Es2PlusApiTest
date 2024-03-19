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
            string certificatePath = @"C:\Projetos\ES2\tim\tim.crt";
            string certificatePfx = @"C:\Projetos\ES2\certs\new_pass.pfx";
            string certificatePassword = "Claryca236566@?@";
            //string certificatePassword = "password";

            var contentJson = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

            try
{
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;

                    // Load the .pfx file with the password
                    var certificate = new X509Certificate2(certificatePfx, certificatePassword);
                    handler.ClientCertificates.Add(certificate);

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        return errors == SslPolicyErrors.None;
                    };

                    using (var client = new HttpClient(handler))
                    {
                        var response = await client.PostAsync(url, contentJson);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return Ok(response);
                        }
                        else
                        {
                            return StatusCode(500, "An error occurred while sending the notification.");
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
