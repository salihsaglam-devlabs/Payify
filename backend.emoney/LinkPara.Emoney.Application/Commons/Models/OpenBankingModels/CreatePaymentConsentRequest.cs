using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.OpenBankingOperations;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;

namespace LinkPara.Emoney.Application.Commons.Models.OpenBankingModels;

public class CreatePaymentConsentRequest : IMapFrom<CreatePaymentConsentCommand>
{
    public string YonTipi { get; set; }
    public string OhkTur { get; set; }
    public string DrmKod { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public RetailPersonOb Alc { get; set; }
    public QrPaymentOb Kkod { get; set; }
    public RetailPersonOb Gon { get; set; }
    public PaymentConsentDetailOb OdmAyr { get; set; }
    public PaymentConsentCompanyDto IsyOdmBlg { get; set; }
    public PaymentConsentInformationDto OdmBsltm { get; set; }
}
