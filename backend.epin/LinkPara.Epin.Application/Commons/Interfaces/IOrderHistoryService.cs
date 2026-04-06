
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IOrderHistoryService
{
    Task<List<OrderHistory>> GetOrderHistoryByDateFromServiceAsync(DateTime date);
    Task AddOrderHistoriesAsync(List<OrderHistory> orderHistory, DateTime reconciliationDate);
}
