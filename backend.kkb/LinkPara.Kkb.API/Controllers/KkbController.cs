using LinkPara.Kkb.Application.Features.Kkb;
using LinkPara.Kkb.Application.Features.Kkb.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Kkb.API.Controllers
{
    public class KkbController : ApiControllerBase
    {
        [Authorize(Policy = "Kkb:Read")]
        [HttpPost("validate-iban")]
        public async Task<ValidateIbanResponse> ValidateIban(ValidateIbanQuery query)
        {
            return await Mediator.Send(query);
        }

        [Authorize(Policy = "Kkb:Read")]
        [HttpPost("inquire-iban")]
        public async Task<InquireIbanResponse> InquireIban(InquireIbanQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}
