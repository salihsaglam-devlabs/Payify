using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response;
using LinkPara.IKS.Application.Features.Merchant.Command.SaveMerchant;
using LinkPara.IKS.Application.Features.Merchant.Command.UpdateMerchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IKS.API.Controllers
{
    public class MerchantsController : ApiControllerBase
    {
        [Authorize(Policy = "MerchantIks:Create")]
        [HttpPost("")]
        public async Task<IKSResponse<MerchantResponse>> SaveMerchantAsync(SaveMerchantCommand command)
        {
            return await Mediator.Send(command);
        }

        /// <summary>
        /// Updates a merchant
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIks:Update")]
        [HttpPut("")]
        public async Task<IKSResponse<MerchantResponse>> UpdateAsync(UpdateMerchantCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
