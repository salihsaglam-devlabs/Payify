using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Models.Merchants
{
    public class MerchantSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public MerchantType MerchantType { get; set; }
        public MerchantStatus MerchantStatus { get; set; }
        public IntegrationMode IntegrationMode { get; set; }
        public Guid CustomerId { get; set; }
        public string CommercialTitle { get; set; }
        public string TaxAdministration { get; set; }
        public string TaxNumber { get; set; }
        public Guid AuthorizedPersonId { get; set; }
        public string AuthorizedPersonCompanyEmail { get; set; }
        public string WebSiteUrl { get; set; }
        public bool Is3dRequired { get; set; }
        public bool IsManuelPayment3dRequired { get; set; }
        public bool IsLinkPayment3dRequired { get; set; }
        public bool IsHostedPayment3dRequired { get; set; }
        public bool IsCvvPaymentAllowed { get; set; }
        public bool IsPostAuthAmountHigherAllowed { get; set; }
        public bool IsReturnApproved { get; set; }
        public bool InstallmentAllowed { get; set; }
        public bool IsExcessReturnAllowed { get; set; }
        public bool InternationalCardAllowed { get; set; }
        public bool PreAuthorizationAllowed { get; set; }
        public bool FinancialTransactionAllowed { get; set; }
        public bool PaymentAllowed { get; set; }
        public bool PaymentReverseAllowed { get; set; }
        public bool PaymentReturnAllowed { get; set; }
        public string PricingProfileNumber { get; set; }
        public Guid ParentMerchantId { get; set; }
        public string ParentMerchantName { get; set; }
        public string ParentMerchantNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public PostingPaymentChannel PostingPaymentChannel { get; set; }
        public List<MerchantBankAccountDto> MerchantBankAccounts { get; set; }
        public List<MerchantApiKeyDto> MerchantApiKeyList { get; set; }
        public List<MerchantWalletDto> MerchantWallets { get; set; }
    }
}
