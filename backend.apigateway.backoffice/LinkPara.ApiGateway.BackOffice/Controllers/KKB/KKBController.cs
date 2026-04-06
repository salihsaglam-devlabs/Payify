using LinkPara.ApiGateway.BackOffice.Services.KKB.HttpClients;
using LinkPara.HttpProviders.KKB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.KKB
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
    }
}
