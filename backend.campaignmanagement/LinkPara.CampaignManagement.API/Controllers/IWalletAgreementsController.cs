using Microsoft.AspNetCore.Mvc;
using LinkPara.CampaignManagement.Application.Features.Agreements.Queries.GetAgreements;
using LinkPara.CampaignManagement.Application.Features.Agreements;
using Microsoft.AspNetCore.Authorization;

namespace LinkPara.CampaignManagement.API.Controllers;

public class IWalletAgreementsController : ApiControllerBase
{
    /// <summary>
    /// Get Agreements
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "IWalletAgreement:ReadAll")]
    [HttpGet]
    public async Task<List<AgreementDto>> GetAgreementsAsync()
    {
        return await Mediator.Send(new GetAgreementsQuery());
    }
}
