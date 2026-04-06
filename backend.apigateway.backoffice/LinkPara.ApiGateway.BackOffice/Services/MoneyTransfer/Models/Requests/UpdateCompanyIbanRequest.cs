namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class UpdateCompanyIbanRequest
{
    public Guid Id { get; set; }
    public string Iban { get; set; }
    public string Description { get; set; }
}
