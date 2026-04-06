namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests
{
    public class DigitalKycEndRequest
    {
        public string Reference { get; set; }
        public string SessionUId { get; set; }
        public string Reason { get; set; }
        public string ReasonDetail { get; set; }
    }
}
