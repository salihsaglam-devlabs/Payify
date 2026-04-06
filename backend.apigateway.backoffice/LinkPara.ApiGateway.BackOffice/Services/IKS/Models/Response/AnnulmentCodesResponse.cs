namespace LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response
{
    public class AnnulmentCodesResponse
    {
        public Annulmentcode[] annulmentCodes { get; set; }
        public int totalCount { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
    }

    public class Annulmentcode
    {
        public string code { get; set; }
        public string description { get; set; }
        public string isInformCode { get; set; }
        public string isCancelCode { get; set; }
        public string isFictitiousCode { get; set; }
        public string isActive { get; set; }
    }
}
