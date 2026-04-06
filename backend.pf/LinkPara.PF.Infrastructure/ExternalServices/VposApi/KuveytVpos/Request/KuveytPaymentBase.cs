using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Request;

public class KuveytPaymentBase
{
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string MerchantId { get; set; }
    public string CustomerId { get; set; }
    public string UserName { get; set; }
    public string Pan { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string MerchantOrderId { get; set; }
    public string LanguageCode { get; set; }
    public string Cvv2 { get; set; }
    public string CardHolderName { get; set; }
    public string TransactionType { get; set; }
    public int InstallmentCount { get; set; }
    public string APIVersion { get; set; }
    public string Amount { get; set; }
    public string HashData { get; set; }
    public string HashPassword { get; set; }
    public string Currency { get; set; }
    public string TransactionSecurity { get; set; }
    public string PFSubMerchantId { get; set; }
    public string PFSubMerchantIdentityTaxNumber { get; set; }   
    public string BKMId { get; set; }
    public string VposSubMerchantId { get; set; }
}
