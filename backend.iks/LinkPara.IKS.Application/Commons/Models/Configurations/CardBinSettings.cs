using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.IKS.Application.Commons.Models.Configurations
{
	public class CardBinSettings
	{
		public string Client_ID { get; set; }
		public string Client_Secret { get; set; }
		public string Grant_Type { get; set; }
		public string Scope { get; set; }
		public string TokenEndPoint { get; set; }
		public string CardBinEndPoint { get; set; }
		public string APIEndPoint { get; set; }
		public string CreateMerchantEndPoint { get; set; }
		public string UpdateMerchantEndPoint { get; set; }
		public string TerminalEndPoint { get; set; }
		public string TerminalCreateEndPoint { get; set; }
		public string AnnulmentsEndPoint { get; set; }
		public string AnnulmentCodesEndPoint { get; set; }
		public string MerchantsQueryEndPoint { get; set; }
		public string CsrFilePassword { get; set; }
		public string AnnulmentsQueryEndPoint { get; set; }
	}
}
