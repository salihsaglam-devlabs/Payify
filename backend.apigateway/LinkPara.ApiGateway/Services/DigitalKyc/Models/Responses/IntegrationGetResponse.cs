namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

public partial class IntegrationGetResponse
{
    public bool IsSuccessful { get; set; }
    public Guid ReferenceId { get; set; }
    public IntegrationGetResponseData[] Data { get; set; }
}

public partial class IntegrationGetResponseData
{
    public Guid UId { get; set; }
    public string Type { get; set; }
    public Guid Reference { get; set; }
    public string CallType { get; set; }
    public string SessionUId { get; set; }
    public IDRegistration IdRegistration { get; set; }
    public AddressRegistration AddressRegistration { get; set; }
    public string IdentityType { get; set; }
    public string IdentityNo { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Data { get; set; }
    public string Status { get; set; }
}