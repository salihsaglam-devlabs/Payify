
namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class SaveAnnulmentRequest
    {
        public Guid Id { get; set; }
        public string AnnulmentCode { get; set; }
        public string AnnulmentCodeDescription { get; set; }
        public string AnnulmentDescription { get; set; }
        public bool IsCancelCode { get; set; }
    }
}
