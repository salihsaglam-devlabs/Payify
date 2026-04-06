namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses
{
    public class LinkResponse
    {
        public Guid Id { get; set; }
        public bool IsSuccess { get; set; }
        public string LinkUrl { get; set; }
    }
}
