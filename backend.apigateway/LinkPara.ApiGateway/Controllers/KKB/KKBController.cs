using LinkPara.ApiGateway.Services.KKB.HttpClients;
using LinkPara.ApiGateway.Services.KKB.Models.Request;
using LinkPara.ApiGateway.Services.KKB.Models.Response;
using LinkPara.HttpProviders.KKB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.KKB
{

    public class KKBController : ApiControllerBase
    {
        private readonly IKKBHttpClient _kkbHttpClient;
        public KKBController(IKKBHttpClient kkbHttpClient)
        {
            _kkbHttpClient = kkbHttpClient;
        }

        [Authorize(Policy = "Kkb:Read")]
        [HttpPost("validate-iban")]
        public async Task<ActionResult<ValidateIbanResponse>> ValidateIban(ValidateIbanRequest request)
        {
            return await _kkbHttpClient.ValidateIbanAsync(request);
        }

        [Authorize(Policy = "Kkb:Read")]
        [HttpPost("inquire-iban")]
        public async Task<InquireIbanResponse> InquireIban(InquireIbanRequest request)
        {
            return await _kkbHttpClient.IbanInquireAsync(request);
        }
    }
}
