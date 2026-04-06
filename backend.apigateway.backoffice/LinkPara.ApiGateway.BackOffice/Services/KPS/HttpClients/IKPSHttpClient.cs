using LinkPara.ApiGateway.BackOffice.Services.KPS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.KPS.Models.Response;

namespace LinkPara.ApiGateway.BackOffice.Services.KPS.HttpClients
{
    public interface IKPSHttpClient
    {
        Task<ValidateIdentityResponse> ValidateIdentityAsync(ValidateIdentityRequest request);

        Task<AddressInformationResponse> GetAddressAsync(AddressInformationRequest request);
    }
}
