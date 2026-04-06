using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Response;
using LinkPara.IKS.Application.Features.CardBin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IKS.API.Controllers
{
	public class CardBinController : ApiControllerBase
	{
		/// <summary>
		/// Card Bin Ranges
		/// </summary>
		/// <returns></returns>
		[Authorize(Policy = "CardBin:Read")]
		[HttpGet("cardBinRanges")]
		public async Task<IKSResponse<CardBinResponse>> GetCardBinRangesAsync()
		{
			return await Mediator.Send(new GetCardBinQuery());
		}
	}
}
