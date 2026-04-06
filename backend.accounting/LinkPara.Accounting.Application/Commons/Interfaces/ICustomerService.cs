using LinkPara.Accounting.Application.Features.Customers;
using LinkPara.Accounting.Application.Features.Customers.Commands.SaveCustomer;
using LinkPara.Accounting.Application.Features.Customers.Queries.GetFilterCustomer;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Accounting.Application.Commons.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> GetByIdAsync(Guid id);
    Task<PaginatedList<CustomerDto>> GetFilterCustomerAsync(GetFilterCustomerQuery request);
    Task SaveCustomerAsync(SaveCustomerCommand request);
    Task<Dictionary<string,CustomerDto>> GetCustomersByCodesAsync(List<string> customerCodes);
    Task<CustomerDto> GetCustomerByCodeAsync(string customerCode);
}
