using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.OrderHistory;
using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace LinkPara.Epin.Infrastructure.Services;
public class OrderHistoryService : IOrderHistoryService
{
    private readonly IEpinHttpClient _httpClient; 
    private readonly IGenericRepository<OrderHistory> _repository;
    private readonly IApplicationUserService _applicationUserService;

    public OrderHistoryService(IEpinHttpClient httpClient,
        IGenericRepository<OrderHistory> repository,
        IApplicationUserService applicationUserService)
    {
        _httpClient = httpClient;
        _repository = repository;
        _applicationUserService = applicationUserService;
    }

    public async Task<List<OrderHistory>> GetOrderHistoryByDateFromServiceAsync(DateTime date)
    {
        var orderHistoryServiceDto = await _httpClient.GetOrderHistoryByDateAsync(date);
        return MapOrderHistoryServiceDtoToOrderHistory(orderHistoryServiceDto);
    }

    private List<OrderHistory> MapOrderHistoryServiceDtoToOrderHistory(OrderHistoryServiceResponse orderHistoryServiceDto)
    {
        var result = new List<OrderHistory>();

        foreach (var item in orderHistoryServiceDto.order_history)
        {
            if (item is null ||
                item.Products is null ||
                item.Products.Product is null ||
                item.Products.Product?.Count == 0 ||
                item.Products.Product?.FirstOrDefault().Pins?.Count == 0)
            {
                continue;
            }

            var orderHistory = new OrderHistory
            {
                ExternalId = item.order,
                Total = item.Total,
                TransactionDate = item.TransactionDate,
                Discount = item.Products.Product.FirstOrDefault().Discount,
                ExternalProductId = item.Products.Product.FirstOrDefault().Product,
                UnitPrice = item.Products.Product.FirstOrDefault().unit_price,
                Pin = item.Products.Product.FirstOrDefault().Pins.FirstOrDefault().Pin,
                Vat = item.Products.Product.FirstOrDefault().Vat,
                RecordStatus = RecordStatus.Active,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };

            result.Add(orderHistory);
        }

        return result;

    }

    public async Task AddOrderHistoriesAsync(List<OrderHistory> orderHistory, DateTime reconciliationDate)
    {
        await DeleteOrderHistoryByDateAsync(reconciliationDate);
        foreach(var item in orderHistory)
        {
            await _repository.AddAsync(item);
        }
    }

    private async Task DeleteOrderHistoryByDateAsync(DateTime reconciliationDate)
    {
        var orderHistories = await _repository.GetAll().Where(x=>x.TransactionDate.Date == reconciliationDate).ToListAsync();

        foreach (var item in orderHistories)
        {
            await _repository.DeleteAsync(item);
        }
    }
}
