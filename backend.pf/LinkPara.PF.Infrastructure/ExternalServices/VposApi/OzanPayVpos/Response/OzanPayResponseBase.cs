namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Response
{
    public class OzanPayResponseBase
    {
        public string TransactionId { get; set; }
        public string ReferenceNo { get; set; }
        public long Code { get; set; }
        public string Status { get; set; }
        public string Operation { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public long Date { get; set; }
    }
}
