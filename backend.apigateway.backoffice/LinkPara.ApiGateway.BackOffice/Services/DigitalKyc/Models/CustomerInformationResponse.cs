using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
public class CustomerInformationResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public string SerialNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public float SimilarityRate { get; set; }
    public KycSessionState KycState { get; set; }
    public DateTime CreateDate { get; set; }
    public List<CustomerKycProcessResponse> CustomerKycProcess { get; set; }
}
