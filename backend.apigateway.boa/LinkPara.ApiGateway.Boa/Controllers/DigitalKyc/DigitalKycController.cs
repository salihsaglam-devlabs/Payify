using LinkPara.ApiGateway.Boa.Filters.CustomerContext;
using LinkPara.ApiGateway.Boa.Services.DigitalKyc.HttpClients;
using LinkPara.ApiGateway.Boa.Services.DigitalKyc.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.DigitalKyc;

public class DigitalKycController : ApiControllerBase
{
    private readonly IDigitalKycHttpClient _httpClient;

    public DigitalKycController(IDigitalKycHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Updates the customer's KYC (Know Your Customer) information.
    /// </summary>
    /// <param name="request">The information required to update the KYC details.</param>
    [HttpPost("kyc-update")]
    [CustomerContextRequired]
    public async Task KycUpdateAsync(KycUpdateRequest request)
    {
        await _httpClient.KycUpdateAsync(request);
    }
}
