namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveMccRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxIndividualInstallmentCount { get; set; }
    public int MaxCorporateInstallmentCount { get; set; }
    public string Description { get; set; }
}
