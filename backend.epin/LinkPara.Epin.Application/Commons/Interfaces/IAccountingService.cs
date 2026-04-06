using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IAccountingService
{
    Task PostAccountingPaymentAsync(Order order, string currencyCode, OperationType operationType);
}
