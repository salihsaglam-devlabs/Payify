using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;

public interface IAccountHttpClient
{
    Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request);
    Task UpdateEmailAsync(UpdateEmailRequest request);
    Task<GetPhoneNumberTokenResponse> GetPhoneNumberUpdateTokenAsync(GetPhoneNumberUpdateTokenRequest request);
    Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request);
    Task<CreateIndividualCustomerResponse> CreateIndividualCustomerAsync(CreateIndividualCustomerRequestWithUsername request);
}
