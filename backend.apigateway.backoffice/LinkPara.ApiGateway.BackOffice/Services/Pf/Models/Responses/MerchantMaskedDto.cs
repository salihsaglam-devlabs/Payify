namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantMaskedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string AuthorizedPersonName { get; set; }
    public string AuthorizedPersonSurname { get; set; }
    public string AuthorizedPersonMaskedPhoneNumber { get; set; }
}
