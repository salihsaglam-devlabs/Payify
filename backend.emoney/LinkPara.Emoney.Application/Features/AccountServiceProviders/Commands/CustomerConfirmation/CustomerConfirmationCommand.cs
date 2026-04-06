using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands;

public class CustomerConfirmationCommand : IRequest<CustomerConfirmationDto>
{
    public Contract Contract { get; set; }
}

public class Contract
{
    public string ConsentType { get; set; }
    public string CustomerType { get; set; }
    public string IdentityType { get; set; }
    public string IdentityValue { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }
    public decimal Amount { get; set; }
    public string SenderIban { get; set; }
    public string ReceiverIban { get; set; }
    public string SenderAccRef { get; set; }
    public string SenderTitle { get; set; }
    public string ReceiverTitle { get; set; }
    public string AddressType { get; set; }
    public string AddressValue { get; set; }
    public string PaymentResource { get; set; }
    public string PaymentPurpose { get; set; }
    public string Currency { get; set; }
    public string DecoupledIdType { get; set; }
    public string DecoupledIdValue { get; set; }
}

public class CustomerConfirmationCommandHandler : IRequestHandler<CustomerConfirmationCommand, CustomerConfirmationDto>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IUserService _userService;

    public CustomerConfirmationCommandHandler(IGenericRepository<Account> accountRepository,
        IUserService userService)
    {
        _accountRepository = accountRepository;
        _userService = userService;
    }

    public async Task<CustomerConfirmationDto> Handle(CustomerConfirmationCommand request, CancellationToken cancellationToken)
    {
        if (request.Contract.ConsentType == "AIS")
        {
            var accountQuery = _accountRepository.GetAll()
                .Include(x => x.AccountUsers)
                .Where(x => x.RecordStatus == RecordStatus.Active);

            if (request.Contract.IdentityType.Equals("K"))
            {
                accountQuery = accountQuery.Where(x => x.IdentityNumber == request.Contract.IdentityValue);
            }

            if (request.Contract.CustomerType.Equals("B"))
            {
                accountQuery = accountQuery.Where(x => x.AccountType == AccountType.Individual);
            }
            else
            {
                accountQuery = accountQuery.Where(x => x.AccountType == AccountType.Corporate);
                //corporateIdentityType
                //corporateIdentityValue ? 
            }

            var account = await accountQuery.FirstOrDefaultAsync();

            if (account is null)
            {
                throw new NotFoundException(nameof(Account));
            }

            var usersIds = account.AccountUsers.Select(x => x.UserId).ToList();

            var deviceInfos = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest { UserIdList = usersIds });

            var result = new CustomerConfirmationDto
            {
                IsCustomerValid = true,
                CustomerId = account.Id.ToString(),
                HasMultipleCustomerFound = false,
                HasMobileDevice = false,
            };

            if (deviceInfos is not null)
            {
                result.HasMobileDevice = deviceInfos.Any(x => x.DeviceInfo.DeviceType == "Phone");
            }

            return result;
        }

        return new CustomerConfirmationDto();
    }
}