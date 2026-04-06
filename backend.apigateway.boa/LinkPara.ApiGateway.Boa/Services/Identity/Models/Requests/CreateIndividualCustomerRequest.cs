namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class CreateIndividualCustomerRequest
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ExternalPersonId { get; set; }
    public string ExternalCustomerId { get; set; }
}

public class CreateIndividualCustomerRequestWithUsername : CreateIndividualCustomerRequest
{
    public string UserName { get; set; }
}
