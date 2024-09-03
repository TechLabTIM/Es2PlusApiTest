namespace Es2PlusApiTest.Models
{
    public class CancelOrderRequest
    {
        public Header Header { get; set; }
        public string Iccid { get; set; }
        public string FinalProfileStatusIndicator { get; set; }
    }

}
