using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Response
{
	public class CardBinResponse
	{
		public Binlist[] BinList { get; set; }
		public bool Success { get; set; }
		public object[] ValidationResult { get; set; }
	}

	public class Binlist
	{
		public string BinRangeMin { get; set; }
		public string BinRangeMax { get; set; }
		public bool Main { get; set; }
		public int CardNoLength { get; set; }
		public bool ValidateForCheckDigit { get; set; }
		public string CardTypeNo { get; set; }
		public string CardType { get; set; }
		public string BrandName { get; set; }
		public string BrandType { get; set; }
		public int MemberNo { get; set; }
		public string MemberName { get; set; }
		public bool ActiveForClearing { get; set; }
		public bool ActiveForFraud { get; set; }
	}
}
