using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CostProfileDto
{
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}

public class CostProfileItemDto
{
    public CardTransactionType CardTransactionType { get; set; }
    public ProfileCardType ProfileCardType { get; set; }

    public int InstallmentNumber { get; set; }
    public int InstallmentNumberEnd { get; set; }
    public decimal CommissionRate { get; set; }
    public int BlockedDayNumber { get; set; }
    public bool IsActive { get; set; }
    public bool? InstallmentSupport { get; set; }
    public List<CostProfileInstallmentDto> CostProfileInstallments { get; set; }
}
