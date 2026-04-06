
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IAccountingService
{
    Task PostAccountingPaymentAsync(AccountingPayment payment);
}
