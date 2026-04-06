using AutoMapper;
using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Features.Customers;
using LinkPara.Accounting.Application.Features.Customers.Commands.SaveCustomer;
using LinkPara.Accounting.Application.Features.Customers.Queries.GetFilterCustomer;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Accounting.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly IGenericRepository<Customer> _repository;
    private readonly IMapper _mapper;
    private readonly IAccountingService _accountingService;
    private readonly IContextProvider _contextProvider;

    public CustomerService(IGenericRepository<Customer> repository,
        IMapper mapper,
        IAccountingService accountingService,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _mapper = mapper;
        _accountingService = accountingService;
        _contextProvider = contextProvider;
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Customer), id);
        }

        return _mapper.Map<CustomerDto>(entity);
    }

    public async Task<PaginatedList<CustomerDto>> GetFilterCustomerAsync(GetFilterCustomerQuery request)
    {
        var customers = _repository.GetAll();


        if (!string.IsNullOrEmpty(request.Q))
        {
            customers = customers.Where(b => b.Code.Contains(request.Q));
        }

        if (request.CreateDateStart is not null)
        {
            customers = customers.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            customers = customers.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.IsSuccess is not null)
        {
            customers = customers.Where(b => b.IsSuccess == request.IsSuccess);
        }

        return await customers
            .PaginatedListWithMappingAsync<Customer, CustomerDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveCustomerAsync(SaveCustomerCommand request)
    {
        await _accountingService.CreateCustomerAsync(new AccountingCustomer
        {
            Code = request.Code,
            CurrencyCode = request.CurrencyCode,
            Email = request.Email,
            FirstName = request.FirstName,
            IdentityNumber = request.IdentityNumber,
            LastName = request.LastName,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Title = request.Title,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            AccountingCustomerType = request.AccountingCustomerType,
            City = request.City,
            CityCode = request.CityCode,
            Country = request.Country,
            CountryCode = request.CountryCode,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            TaxOfficeCode = request.TaxOfficeCode,
            District = request.District,
            Town = request.Town
        });
    }

    public async Task<Dictionary<string, CustomerDto>> GetCustomersByCodesAsync(List<string> customerCodes)
    {
        var customers = await _repository.GetAll().Where(x => customerCodes.Any(c => c == x.Code)).ToListAsync();

        var result = new Dictionary<string, CustomerDto>();

        if (customers.Count <= 0)
        {
            return result;
        }

        customers.ForEach(item =>
        {
            if (!result.ContainsKey(item.Code))
            {
                var customer = _mapper.Map<CustomerDto>(item);
                result.Add(customer.Code, customer);
            }
        });

        return result;
    }

    public async Task<CustomerDto> GetCustomerByCodeAsync(string customerCode)
    {
        var customer = await _repository.GetAll().Where(x => x.Code == customerCode).SingleOrDefaultAsync();

        if (customer is not null)
        {
            return _mapper.Map<CustomerDto>(customer);
        }

        return new CustomerDto { };
    }
}
