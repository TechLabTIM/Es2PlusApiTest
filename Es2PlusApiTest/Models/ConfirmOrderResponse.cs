namespace Es2PlusApiTest.Models
{
    public class ConfirmOrderResponse
    {
        public HeaderExecutionStatus Header { get; set; }
        public string MatchingId { get; set; }
        public string SmdpAddress { get; set; }
    }

}
