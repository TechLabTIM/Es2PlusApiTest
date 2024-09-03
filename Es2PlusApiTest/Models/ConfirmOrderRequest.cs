using static Es2PlusApiTest.Controllers.NotificationsController;

namespace Es2PlusApiTest.Models
{
    public class ConfirmOrderRequest
    {
        public Header Header { get; set; }
        public string Iccid { get; set; }
        public bool ReleaseFlag { get; set; }
    }

}
