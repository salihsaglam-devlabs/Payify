using LinkPara.Accounting.Application.Commons.Attributes;
using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Customers.Commands.SaveCustomer;

public class SaveCustomerCommand : IRequest
{
    public Guid Id { get; set; }
    [Audit]
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Audit]
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string IdentityNumber { get; set; }
    public string Title { get; set; }
    public string CurrencyCode { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
    public string City { get; set; }
    public string CityCode { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public string Address { get; set; }
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string TaxOfficeCode { get; set; }
    public string Town { get; set; }
    public string District { get; set; }
}
public class SaveCustomerCommandHandler : IRequestHandler<SaveCustomerCommand>
{
    private readonly ICustomerService _customerService;

    public SaveCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Unit> Handle(SaveCustomerCommand request, CancellationToken cancellationToken)
    {
        await _customerService.SaveCustomerAsync(request);
        return Unit.Value;
    }
}