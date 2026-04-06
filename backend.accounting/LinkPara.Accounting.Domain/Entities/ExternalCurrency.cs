using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Accounting.Domain.Entities;

public class ExternalCurrency : AuditEntity, ITrackChange
{
    public int ExternalCurrencyId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string AccountCode { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
}
