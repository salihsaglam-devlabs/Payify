using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Models.Merchants
{
    public class UpdateMerchantPanelDto
    {
        public string WebSiteUrl { get; set; }
        public string Iban { get; set; }
        public int BankCode { get; set; }
        public string CompanyEmail { get; set; }
    }
}
