using LinkPara.ApiGateway.Services.KPS.Models.Request;
using LinkPara.ApiGateway.Services.KPS.Models.Response;

namespace LinkPara.ApiGateway.Services.KPS.HttpClients
{
    public interface IKPSHttpClient
    {
        Task<ValidateIdentityResponse> ValidateIdentityAsync(ValidateIdentityRequest request);
        Task<AddressInformationResponse> GetAddressAsync(AddressInformationRequest request);
        Task<ValidateCustodyInformationResponse> ValidateCustodyInformationAsync(ValidateCustodyInformationRequest request);
    }
}
