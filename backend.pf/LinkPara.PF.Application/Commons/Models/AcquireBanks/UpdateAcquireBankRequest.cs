using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.AcquireBanks;

public class UpdateAcquireBankRequest : IMapFrom<AcquireBank>
{
    public int BankCode { get; set; }
    public int EndOfDayHour { get; set; }
    public int EndOfDayMinute { get; set; }
    public bool AcceptAmex { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public bool HasSubmerchantIntegration { get; set; }
    public bool AllowNotOnUs { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
}
