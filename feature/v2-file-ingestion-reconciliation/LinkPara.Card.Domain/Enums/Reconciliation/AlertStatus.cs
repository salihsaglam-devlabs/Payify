using System.ComponentModel;

namespace LinkPara.Card.Domain.Enums.Reconciliation;
public enum AlertStatus : byte
{
    [Description("Alert has been created but not yet processed.")]
    Pending = 0,

    [Description("Alert is currently being processed.")]
    Processing = 1,

    [Description("Alert has been successfully consumed.")]
    Consumed = 2,

    [Description("An error occurred while processing the alert.")]
    Failed = 3,

    [Description("Alert has been intentionally ignored.")]
    Ignored = 4
}