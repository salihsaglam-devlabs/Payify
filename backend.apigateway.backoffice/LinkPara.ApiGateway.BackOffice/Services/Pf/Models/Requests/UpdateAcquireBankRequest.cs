using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateAcquireBankRequest
{
    public Guid Id { get; set; }
    public int BankCode { get; set; }
    public int EndOfDayHour { get; set; }
    public int EndOfDayMinute { get; set; }
    public bool AcceptAmex { get; set; }
    public bool HasSubmerchantIntegration { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public bool RestrictOwnCardNotOnUs { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
}
