namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetRequestBase
    {
        public string MerchantId { get; set; }
        public string TerminalId { get; set; } 
        public int TranDateRequired { get; set; } 
        public string PosnetId { get; set; } 
        public string CorrelationID { get; set; }
        public string AuthCode { get; set; }
        public string OrderId { get; set; }
        public string OrderDate { get; set; }
    }
}
