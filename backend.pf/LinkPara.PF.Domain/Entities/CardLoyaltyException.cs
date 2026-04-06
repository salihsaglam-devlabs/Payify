using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CardLoyaltyException : AuditEntity
{
    public int BankCode { get; set; }
    public Bank Bank { get; set; }
    public int CounterBankCode { get; set; }
    public Bank CounterBank { get; set; }
    public bool AllowOnUs { get; set; }
    public bool AllowInstallment { get; set; }
    public bool AllowPoint { get; set; }
}