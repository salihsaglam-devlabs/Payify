using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;

public class CardBinRequest
{
    public int offset { get; set; }
    public int limit { get; set; }
    public ulong bin { get; set; }
}