using LinkPara.ApiGateway.Services.KPS.HttpClients;
using LinkPara.ApiGateway.Services.KPS.Models.Request;
using LinkPara.ApiGateway.Services.KPS.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.KPS
{
    public class KPSController : ApiControllerBase
    {
        private readonly IKPSHttpClient _kpsHttpClient;
        public KPSController(IKPSHttpClient kpsHttpClient)
        {
            _kpsHttpClient = kpsHttpClient;
        }

        [HttpGet("validate-identity")]
        [Authorize(Policy = "Kps:Read")]
        public async Task<ActionResult<ValidateIdentityResponse>> ValidateIdentityAsync([FromQuery] ValidateIdentityRequest request)
        {
            return await _kpsHttpClient.ValidateIdentityAsync(request);
        }

        [HttpGet("address-information")]
        [Authorize(Policy = "Kps:Read")]
        public async Task<ActionResult<AddressInformationResponse>> GetAddressAsync([FromQuery] AddressInformationRequest request)
        {
            return await _kpsHttpClient.GetAddressAsync(request);
        }

        [HttpGet("validate-custody-information")]
        [Authorize(Policy = "Kps:Read")]
        public async Task<ActionResult<ValidateCustodyInformationResponse>> ValidateCustodyInformationAsync([FromQuery] ValidateCustodyInformationRequest request)
        {
            return await _kpsHttpClient.ValidateCustodyInformationAsync(request);
        }
    }
}
