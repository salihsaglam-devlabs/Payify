using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkPara.HttpProviders.IKS.Models.Response;

namespace LinkPara.HttpProviders.CardBin;

public interface ICardBinService
{
	Task<IKSResponse<CardBinResponse>> GetCardBinRangeAsync();
}
