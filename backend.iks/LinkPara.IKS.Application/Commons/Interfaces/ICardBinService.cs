using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Response;

namespace LinkPara.IKS.Application.Commons.Interfaces
{
	public interface ICardBinService
	{
		Task<IKSResponse<CardBinResponse>> GetCardBinRangeAsync(CardBinRequest request);
	}
}
