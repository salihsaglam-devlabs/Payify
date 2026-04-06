using AutoMapper;
using EFCore.BulkExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.KPS.Models;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Customers;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchant;
using LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantInstallmentTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantTransactions;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkIntegrationModeUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPermissionUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPricingProfileUpdate;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Transactions;
using CustomerDto = LinkPara.PF.Application.Commons.Models.Customers.CustomerDto;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantService : IMerchantService
{
    private readonly ILogger<MerchantService> _logger;
    private readonly IMapper _mapper;
    private readonly PfDbContext _context;
    private readonly IGenericRepository<Merchant> _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IIksPfService _iksPfService;
    private readonly ICustomerService _customerService;
    private readonly IRestrictionService _restrictionService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IUserService _userService;
    private readonly IKKBService _kkbService;
    private readonly IKpsService _kpsService;
    private readonly ISearchService _searchService;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    private readonly IBus _bus;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _merchantInstallmentTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IPricingProfileService _pricingProfileService;
    private readonly ILocationService _locationService;

    public MerchantService(ILogger<MerchantService> logger, IMapper mapper,
        PfDbContext context,
        IGenericRepository<Merchant> repository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IVaultClient vaultClient,
        IGenericRepository<Customer> customerRepository,
        IIksPfService iksPfService,
        ICustomerService customerService,
        IRestrictionService restrictionService,
        IApplicationUserService applicationUserService,
        IUserService userService,
        IKKBService kkbService,
        IKpsService kpsService,
        ILocationService locationService,
        IStringLocalizerFactory factory, IParameterService parameterService, ISearchService searchService, IBus bus, IGenericRepository<MerchantTransaction> merchantTransactionRepository, IGenericRepository<BankTransaction> bankTransactionRepository, IGenericRepository<Vpos> vposRepository, IPricingProfileService pricingProfileService, IGenericRepository<MerchantInstallmentTransaction> merchantInstallmentTransactionRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _repository = repository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _vaultClient = vaultClient;
        _customerRepository = customerRepository;
        _iksPfService = iksPfService;
        _customerService = customerService;
        _restrictionService = restrictionService;
        _applicationUserService = applicationUserService;
        _userService = userService;
        _kkbService = kkbService;
        _kpsService = kpsService;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _parameterService = parameterService;
        _searchService = searchService;
        _bus = bus;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _vposRepository = vposRepository;
        _pricingProfileService = pricingProfileService;
        _locationService = locationService;
        _merchantInstallmentTransactionRepository = merchantInstallmentTransactionRepository;
    }

    public async Task ApproveMerchant(ApproveMerchantCommand command)
    {
        var merchant = await _context.Merchant
            .Include(b => b.MerchantVposList).ThenInclude(c => c.Vpos)
            .Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == command.MerchantId);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), command.MerchantId);
        }

        try
        {
            var oldMerchantStatus = merchant.MerchantStatus;

            var isIksEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "IksEnabled");
            if (isIksEnabled)
            {
                if (merchant.MerchantStatus == MerchantStatus.Active || merchant.MerchantStatus == MerchantStatus.Closed || merchant.MerchantStatus == MerchantStatus.RiskApproval)
                {
                    await _iksPfService.IKSSaveMerchantAsync(merchant);
                }
            }

            if (oldMerchantStatus == merchant.MerchantStatus)
            {
                merchant.MerchantStatus = command.MerchantStatus;
            }

            merchant.RejectReason = command.RejectReason;
            merchant.ParameterValue = command.ParameterValue;

            _context.Merchant.Update(merchant);

            if ((oldMerchantStatus == MerchantStatus.PendingIKS || oldMerchantStatus == MerchantStatus.Pending || oldMerchantStatus == MerchantStatus.Draft || oldMerchantStatus == MerchantStatus.RiskApproval) && command.MerchantStatus == MerchantStatus.Active)
            {
                var createCustomerResponse = await CreateCustomerAsync(merchant);
                var customerId = merchant.CustomerId;

                merchant.Customer.CustomerNumber = createCustomerResponse.CustomerNumber;
                merchant.Customer.CustomerId = createCustomerResponse.CustomerId;

                await PublishCustomerNumberUpdateAsync(createCustomerResponse, customerId);

            }

            if (merchant.Customer is not null)
            {
                merchant.Customer.CustomerStatus = merchant.MerchantStatus switch
                {
                    MerchantStatus.Reject => CustomerStatus.Rejected,
                    MerchantStatus.Active => CustomerStatus.Active,
                    _ => merchant.Customer.CustomerStatus
                };
            }

            await _context.SaveChangesAsync();

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "ApproveMerchant",
                    SourceApplication = "PF",
                    Resource = "Merchant",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"MerchantId", command.MerchantId.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantApproveError : {exception}");
            throw;
        }
    }
    private async Task<CreateCustomerResponse> CreateCustomerAsync(Merchant merchant)
    {
        var product = new CustomerProductDto
        {
            OpeningDate = DateTime.Now,
            MerchantId = merchant.Id,
            ProductType = ProductType.PF,
            CustomerProductStatus = CustomerProductStatus.Inactive
        };

        var customerRequest = CreateCustomerRequest(merchant);

        customerRequest.CustomerType = merchant.Customer.CompanyType switch
        {
            CompanyType.Individual => CustomerType.Individual,
            CompanyType.Corporate => CustomerType.Corporate,
            CompanyType.Enterprise => CustomerType.Enterprise,
            _ => customerRequest.CustomerType
        };

        customerRequest.CreateCustomerProducts.Add(product);

        return await _customerService.CreateCustomerAsync(customerRequest);
    }
    private CreateCustomerRequest CreateCustomerRequest(Merchant merchant)
    {

        var address = new CustomerAddressDto();
        var productList = new List<CustomerProductDto>();
        var addressList = new List<CustomerAddressDto>();
        var customerRequest = new CreateCustomerRequest();

        if (merchant.Customer.CompanyType == CompanyType.Individual)
        {
            customerRequest.FirstName = merchant.Customer.AuthorizedPerson.Name;
            customerRequest.LastName = merchant.Customer.AuthorizedPerson.Surname;
            customerRequest.BirthDate = merchant.Customer.AuthorizedPerson.BirthDate;
            customerRequest.NationCountry = "TR";
            customerRequest.NationCountryId = "TR";
            customerRequest.IdentityNumber = merchant.Customer.AuthorizedPerson.IdentityNumber;
            customerRequest.DocumentType = DocumentType.Identity;
        }
        customerRequest.CommercialTitle = merchant.Customer.CommercialTitle;
        customerRequest.TradeRegistrationNumber = merchant.Customer.TradeRegistrationNumber;
        customerRequest.TaxNumber = merchant.Customer.TaxNumber;
        customerRequest.TaxAdministration = merchant.Customer.TaxAdministration;
        customerRequest.UserId = _applicationUserService.ApplicationUserId;

        address.DistrictId = merchant.Customer.District;
        address.District = merchant.Customer.DistrictName;
        address.CountryId = merchant.Customer.Country;
        address.Country = merchant.Customer.CountryName;
        address.City = merchant.Customer.CityName;
        address.CityId = merchant.Customer.City;
        address.Address = merchant.Customer.Address;
        address.AddressType = AddressType.Company;
        address.PostalCode = merchant.Customer.PostalCode;
        addressList.Add(address);

        customerRequest.CreateCustomerAddresses = addressList;
        customerRequest.CreateCustomerProducts = productList;
        customerRequest.CreateCustomerPhones = CreatePhonesDto(merchant);
        customerRequest.CreateCustomerEmails = CreateEmailsDto(merchant);

        return customerRequest;
    }
    private async Task PublishCustomerNumberUpdateAsync(CreateCustomerResponse response, Guid customerId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.UpdateCustomerNumber"));
        await endpoint.Send(new UpdateCustomerNumber
        {
            CustomerId = customerId,
            CustomerManagementId = response.CustomerId,
            CustomerNumber = response.CustomerNumber
        }, tokenSource.Token);
    }

    public async Task DeleteAsync(DeleteMerchantCommand command)
    {
        var merchant = await _repository.GetAll()
            .Include(b => b.Customer)
            .ThenInclude(c => c.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), command.Id);
        }

        try
        {
            merchant.RecordStatus = RecordStatus.Passive;
            merchant.MerchantStatus = MerchantStatus.Closed;

            var createCustomerRequest = await PopulateCustomerRequestAsync(merchant);
            var createCustomerResponse = await _customerService.CreateCustomerAsync(createCustomerRequest);
            if (createCustomerResponse.IsChanged)
            {
                merchant.Customer.CustomerNumber = createCustomerResponse.CustomerNumber;
                merchant.Customer.CustomerId = createCustomerResponse.CustomerId;
            }

            await _repository.UpdateAsync(merchant);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteMerchant",
                    SourceApplication = "PF",
                    Resource = "Merchant",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString() },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantDeleteError : {exception}");
            throw;
        }
    }

    public async Task<MerchantDto> GetByIdAsync(Guid id)
    {
        await _restrictionService.IsUserAuthorizedAsync(id);

        var merchant = await _repository.GetAll().Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson).Include(b => b.MerchantBankAccounts.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantVposList.Where(b => b.RecordStatus == RecordStatus.Active)
            .OrderBy(b => b.Priority))
            .ThenInclude(b => b.Vpos).Include(b => b.MerchantIntegrator)
            .Include(b => b.MerchantScores).Include(b => b.TechnicalContact)
            .Include(b => b.MerchantDocuments.Where(b => b.RecordStatus == RecordStatus.Active && b.MerchantTransactionId == null)
            .OrderByDescending(b => b.UpdateDate))
            .Include(b => b.MerchantUsers.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantLimits.Where(b => b.RecordStatus == RecordStatus.Active))
            .Include(b => b.MerchantWallets.Where(a => a.RecordStatus == RecordStatus.Active))
            .Include(b => b.Nace)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), id);
        }

        return _mapper.Map<MerchantDto>(merchant);
    }

    public async Task<MerchantDto> GetByIdWithOptionsAsync(Guid id, SubQueryOptions options)
    {
        var query = _repository.GetAll();

        if (options.HasFlag(SubQueryOptions.MerchantBankAccounts))
        {
            query.Include(b => b.MerchantBankAccounts);
        }

        if (options.HasFlag(SubQueryOptions.MerchantVposList))
        {
            query.Include(b => b.MerchantVposList
                    .Where(b => b.RecordStatus == RecordStatus.Active)
                    .OrderBy(b => b.Priority))
                .ThenInclude(b => b.Vpos);
        }

        if (options.HasFlag(SubQueryOptions.MerchantIntegrator))
        {
            query.Include(b => b.MerchantIntegrator);
        }

        if (options.HasFlag(SubQueryOptions.MerchantScores))
        {
            query.Include(b => b.MerchantScores);
        }

        if (options.HasFlag(SubQueryOptions.TechnicalContact))
        {
            query.Include(b => b.TechnicalContact);
        }

        if (options.HasFlag(SubQueryOptions.MerchantDocuments))
        {
            query.Include(b => b.MerchantDocuments
                .Where(b => b.RecordStatus == RecordStatus.Active)
                .OrderByDescending(b => b.UpdateDate));
        }

        if (options.HasFlag(SubQueryOptions.MerchantUsers))
        {
            query.Include(b => b.MerchantUsers
                .Where(b => b.RecordStatus == RecordStatus.Active));
        }

        if (options.HasFlag(SubQueryOptions.MerchantLimits))
        {
            query.Include(b => b.MerchantLimits
                .Where(b => b.RecordStatus == RecordStatus.Active));
        }

        if (options.HasFlag(SubQueryOptions.MerchantApiKeyList))
        {
            query.Include(b => b.MerchantApiKeyList);
        }

        if (options.HasFlag(SubQueryOptions.MerchantWallets))
        {
            query.Include(b => b.MerchantWallets);
        }

        var merchant = await query
            .FirstOrDefaultAsync(b => b.Id == id);

        return _mapper.Map<MerchantDto>(merchant);
    }

    public async Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantQuery request)
    {
        var merchantList = _repository.GetAll()
            .Include(b => b.Customer)
            .ThenInclude(c => c.AuthorizedPerson)
            .Include(b => b.MerchantBlockageList)
            .Include(m => m.MerchantUsers)
            .Include(m => m.MerchantBankAccounts)
            .Include(m => m.MerchantWallets)
            .Include(m => m.TechnicalContact)
            .Include(m => m.Nace)
            .AsQueryable();

        if (request.MerchantId is not null)
        {
            merchantList = merchantList
                    .Where(b => b.Id == request.MerchantId);
        }
        else if (!string.IsNullOrWhiteSpace(request.MerchantNumber))
        {
            merchantList = merchantList
                .Where(b => b.Number == request.MerchantNumber);
        }
        else
        {
            if (!string.IsNullOrEmpty(request.MerchantName))
            {
                merchantList = merchantList
                    .Where(b => b.Name.Contains(request.MerchantName));
            }

            if (!string.IsNullOrEmpty(request.Q))
            {
                merchantList = merchantList.Where(b => b.Name.Contains(request.Q));
            }

            if (request.CompanyType is not null)
            {
                merchantList = merchantList
                        .Where(b => b.Customer.CompanyType == request.CompanyType);
            }

            if (request.CreateDateStart is not null)
            {
                merchantList = merchantList.Where(b => b.CreateDate
                                   >= request.CreateDateStart);
            }

            if (request.CreateDateEnd is not null)
            {
                merchantList = merchantList.Where(b => b.CreateDate
                                   <= request.CreateDateEnd);
            }

            if (request.MerchantStatus is not null)
            {
                merchantList = merchantList
                        .Where(b => b.MerchantStatus == request.MerchantStatus);
            }

            if (request.ApplicationChannel is not null)
            {
                merchantList = merchantList
                        .Where(b => b.ApplicationChannel == request.ApplicationChannel);
            }

            if (request.IntegrationMode is not null)
            {
                var mode = request.IntegrationMode.ToString();
                merchantList = merchantList
                        .Where(b => ((string)(object)b.IntegrationMode).Contains(mode));
            }

            if (request.CountryCode is not null)
            {
                merchantList = merchantList
                        .Where(b => b.Customer.Country == request.CountryCode);
            }

            if (request.CityCode is not null)
            {
                merchantList = merchantList
                        .Where(b => b.Customer.City == request.CityCode);
            }

            if (request.MerchantType is not null)
            {
                merchantList = merchantList
                    .Where(b => b.MerchantType == request.MerchantType);
            }

            if (request.IsBlockage is not null)
            {
                if (request.IsBlockage is true)
                {
                    merchantList = merchantList
                                       .Where(b => b.MerchantBlockageList.Any());
                }
                else
                {
                    merchantList = merchantList
                       .Where(b => !b.MerchantBlockageList.Any());
                }
            }

            if (request.PostingPaymentChannel is not null)
            {
                merchantList = merchantList
                    .Where(b => b.PostingPaymentChannel == request.PostingPaymentChannel);
            }

            if (request.ParentMerchantId is not null)
            {
                merchantList = merchantList
                        .Where(m => m.ParentMerchantId == request.ParentMerchantId);
            }

            if (!string.IsNullOrWhiteSpace(request.PricingProfileNumber))
            {
                merchantList = merchantList
                    .Where(m => m.PricingProfileNumber == request.PricingProfileNumber);
            }

            if (request.IsInstallmentAllowed != null)
            {
                merchantList = merchantList.Where(m => m.InstallmentAllowed == request.IsInstallmentAllowed);
            }

            if (request.IsPreAuthAllowed != null)
            {
                merchantList = merchantList.Where(m => m.PreAuthorizationAllowed == request.IsPreAuthAllowed);
            }

            if (request.IsInternationalCardAllowed != null)
            {
                merchantList = merchantList.Where(m => m.InternationalCardAllowed == request.IsInternationalCardAllowed);
            }

            if (request.IsReturnAllowed != null)
            {
                merchantList = merchantList.Where(m => m.PaymentReturnAllowed == request.IsReturnAllowed);
            }

            if (request.IsReverseAllowed != null)
            {
                merchantList = merchantList.Where(m => m.PaymentReverseAllowed == request.IsReverseAllowed);
            }

            if (request.InsurancePaymentAllowed != null)
            {
                merchantList = merchantList.Where(m => m.InsurancePaymentAllowed == request.InsurancePaymentAllowed);
            }

            if (!string.IsNullOrEmpty(request.MccCode))
            {
                merchantList = merchantList
                    .Where(m => m.MccCode == request.MccCode);
            }
            
            if (!string.IsNullOrEmpty(request.NaceCode))
            {
                merchantList = merchantList
                    .Where(m => m.NaceCode == request.NaceCode);
            }

            if (request.PosType is not null)
            {
                merchantList = merchantList.Where(m => m.PosType == request.PosType);
            }

            if (request.MoneyTransferStartHourStart is not null)
            {
                merchantList = merchantList
                    .Where(m => m.MoneyTransferStartHour >= request.MoneyTransferStartHourStart);
            }
            
            if (request.MoneyTransferStartHourFinish is not null)
            {
                merchantList = merchantList
                    .Where(m => m.MoneyTransferStartHour <= request.MoneyTransferStartHourFinish);
            }
            
            if (request.MoneyTransferStartMinuteStart is not null)
            {
                merchantList = merchantList
                    .Where(m => m.MoneyTransferStartMinute >= request.MoneyTransferStartMinuteStart);
            }
            
            if (request.MoneyTransferStartMinuteFinish is not null)
            {
                merchantList = merchantList
                    .Where(m => m.MoneyTransferStartMinute <= request.MoneyTransferStartMinuteFinish);
            }
        }

        return await merchantList
            .PaginatedListWithMappingAsync<Merchant, MerchantDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task PatchMerchant(Merchant command)
    {
        _context.Merchant.Update(command);
        await _context.SaveChangesAsync();
    }

    public async Task<MerchantResponse> UpdateAsync(UpdateMerchantCommand command)
    {
        var merchant = await ValidateMerchantAsync(command);

        var strategy = _context.Database.CreateExecutionStrategy();

        await CheckValidationsAsync(command, merchant);
        
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                UpdateAuthorizedPerson(merchant, command.Customer.AuthorizedPerson);

                if (command.TechnicalContact != null && !String.IsNullOrEmpty(command.TechnicalContact.IdentityNumber))
                {
                    if (command.TechnicalContact.Id != Guid.Empty)
                    {
                        UpdateTechnicalContact(merchant, command.TechnicalContact);
                    }
                    else
                    {
                        var contactPerson = _mapper.Map<ContactPerson>(command.TechnicalContact);
                        contactPerson.ContactPersonType = ContactPersonType.TechnicalPerson;

                        await _context.ContactPerson.AddAsync(contactPerson);

                        merchant.ContactPersonId = contactPerson.Id;
                    }

                }

                if (command.PricingProfileNumber != merchant.PricingProfileNumber)
                {
                    var pricingProfileFilter = await _pricingProfileService.GetFilterListAsync(
                        new GetFilterPricingProfileQuery
                        {
                            PricingProfileNumber = command.PricingProfileNumber
                        });
                    var pricingProfile = pricingProfileFilter.Items[0];
                    if (command.IsPaymentToMainMerchant != pricingProfile.IsPaymentToMainMerchant)
                    {
                        throw new InvalidMerchantPricingProfileException();
                    }
                }
                
                var defaultMoneyTransferStartHour = await MoneyTransferHourHelper.GetMoneyTransferHourAsync(_parameterService, _logger);

                merchant.Name = command.Name;
                merchant.MerchantType = command.MerchantType;
                merchant.ApplicationChannel = command.ApplicationChannel;
                merchant.IntegrationMode = command.PosType == PosType.Physical ? IntegrationMode.Unknown : command.IntegrationMode;
                merchant.MerchantStatus = command.MerchantStatus != MerchantStatus.Draft ? MerchantStatus.Pending : MerchantStatus.Draft;
                merchant.AgreementDate = command.AgreementDate;
                merchant.MccCode = command.MccCode;
                merchant.NaceCode = command.NaceCode;
                merchant.Language = command.Language;
                merchant.SalesPersonId = command.SalesPersonId;
                merchant.PaymentDueDay = command.PaymentDueDay;
                merchant.Is3dRequired = command.PosType == PosType.Physical ? false : command.Is3dRequired;
                merchant.IsManuelPayment3dRequired = command.PosType == PosType.Physical ? false : command.IsManuelPayment3dRequired;
                merchant.IsLinkPayment3dRequired = command.PosType == PosType.Physical ? false : command.IsLinkPayment3dRequired;
                merchant.IsHostedPayment3dRequired = command.PosType == PosType.Physical ? false : command.IsHostedPayment3dRequired;
                merchant.IsCvvPaymentAllowed = command.PosType == PosType.Physical ? false : command.IsCvvPaymentAllowed;
                merchant.IsReturnApproved = command.PosType == PosType.Physical ? false : command.IsReturnApproved;
                merchant.IsPostAuthAmountHigherAllowed = command.IsPostAuthAmountHigherAllowed;
                merchant.IsDocumentRequired = command.IsDocumentRequired;
                merchant.InstallmentAllowed = command.PosType == PosType.Physical ? false : command.InstallmentAllowed;
                merchant.IsExcessReturnAllowed = command.PosType == PosType.Physical ? false : command.IsExcessReturnAllowed;
                merchant.HalfSecureAllowed = command.PosType == PosType.Physical ? false : command.HalfSecureAllowed;
                merchant.InternationalCardAllowed = command.PosType == PosType.Physical ? false : command.InternationalCardAllowed;
                merchant.PreAuthorizationAllowed = command.PosType == PosType.Physical ? false : command.PreAuthorizationAllowed;
                merchant.FinancialTransactionAllowed = command.FinancialTransactionAllowed;
                merchant.PaymentAllowed = command.PaymentAllowed;
                merchant.PaymentReturnAllowed = command.PosType == PosType.Physical ? false : command.PaymentReturnAllowed;
                merchant.PaymentReverseAllowed = command.PosType == PosType.Physical ? false : command.PaymentReverseAllowed;
                merchant.PricingProfileNumber = command.PricingProfileNumber;
                merchant.MerchantIntegratorId = command.MerchantIntegratorId;
                merchant.RejectReason = command.RejectReason;
                merchant.WebSiteUrl = command.WebSiteUrl;
                merchant.MonthlyTurnover = command.MonthlyTurnover;
                merchant.PhoneCode = command.PhoneCode;
                merchant.HostingTaxNo = command.PosType == PosType.Physical ? string.Empty : command.HostingTaxNo;
                merchant.HostingTradeName = command.PosType == PosType.Physical ? string.Empty : command.HostingTradeName;
                merchant.HostingUrl = command.PosType == PosType.Physical ? string.Empty : command.HostingUrl;
                merchant.PostingPaymentChannel = command.PostingPaymentChannel;
                merchant.IsPaymentToMainMerchant = command.IsPaymentToMainMerchant;
                merchant.InsurancePaymentAllowed = command.PosType == PosType.Physical ? false : command.InsurancePaymentAllowed;
                merchant.EstablishmentDate = command.EstablishmentDate;
                merchant.BusinessModel = command.BusinessModel;
                merchant.BusinessActivity = command.BusinessActivity;
                merchant.BranchCount = command.BranchCount;
                merchant.EmployeeCount = command.EmployeeCount;
                merchant.MoneyTransferStartHour = command.MoneyTransferStartHour;
                merchant.MoneyTransferStartMinute = command.MoneyTransferStartMinute;

                if (command.MerchantType == MerchantType.SubMerchant)
                {
                    merchant.ParentMerchantId = command.ParentMerchantId;
                    merchant.ParentMerchantName = command.ParentMerchantName;
                    merchant.ParentMerchantNumber = command.ParentMerchantNumber;
                }

                if (command.MerchantType == MerchantType.MainMerchant)
                {
                    merchant.IsInvoiceCommissionReflected = command.IsInvoiceCommissionReflected;
                }

                if (merchant.MerchantStatus == MerchantStatus.Draft)
                {
                    merchant.PosType = command.PosType;
                }

                _context.Merchant.Update(merchant);

                await UpdateCustomerAsync(merchant, command.Customer);

                var activeCommandBankAccount =
                    command.MerchantBankAccounts.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);
                var activeMerchantBankAccount = merchant.MerchantBankAccounts.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

                if (activeMerchantBankAccount is not null)
                {
                    if (activeCommandBankAccount is null || activeCommandBankAccount.BankCode == -1)
                    {
                        merchant.MerchantBankAccounts.ForEach(s => { s.RecordStatus = RecordStatus.Passive; });
                        _context.MerchantBankAccount.UpdateRange(merchant.MerchantBankAccounts);
                    }
                    else
                    {
                        if (activeCommandBankAccount.Iban != activeMerchantBankAccount.Iban ||
                            activeCommandBankAccount.BankCode != activeMerchantBankAccount.BankCode)
                        {
                            merchant.MerchantBankAccounts.ForEach(s => { s.RecordStatus = RecordStatus.Passive; });
                            _context.MerchantBankAccount.UpdateRange(merchant.MerchantBankAccounts);
                        
                            await _context.MerchantBankAccount.AddAsync(new MerchantBankAccount
                            {
                                CreateDate = DateTime.Now,
                                MerchantId = merchant.Id,
                                CreatedBy = parseUserId.ToString(),
                                RecordStatus = RecordStatus.Active,
                                BankCode = activeCommandBankAccount.BankCode,
                                Iban = activeCommandBankAccount.Iban.Trim().Replace(" ", string.Empty)
                            });
                        
                            await _bus.Publish(new MerchantIbanChanged
                            {
                                MerchantName = merchant.Name,
                                MerchantNumber = merchant.Number,
                                OldIban = activeMerchantBankAccount.Iban,
                                NewIban = activeCommandBankAccount.Iban
                            }, CancellationToken.None);
                        }
                    }
                }
                else
                {
                    if (activeCommandBankAccount is not null && activeCommandBankAccount.BankCode != -1)
                    {
                        await _context.MerchantBankAccount.AddAsync(new MerchantBankAccount
                        {
                            CreateDate = DateTime.Now,
                            MerchantId = merchant.Id,
                            CreatedBy = parseUserId.ToString(),
                            RecordStatus = RecordStatus.Active,
                            BankCode = activeCommandBankAccount.BankCode,
                            Iban = activeCommandBankAccount.Iban.Trim().Replace(" ", string.Empty),

                        });
                        
                        await _bus.Publish(new MerchantIbanChanged
                        {
                            MerchantName = merchant.Name,
                            MerchantNumber = merchant.Number,
                            OldIban = "-",
                            NewIban = activeCommandBankAccount.Iban
                        }, CancellationToken.None);
                    }
                }

                if (command.MerchantWallets is not null && command.MerchantWallets.Any())
                {
                    var activeCommandWallet = command.MerchantWallets
                        .FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

                    var activeMerchantWallet =
                        merchant.MerchantWallets.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

                    if (activeCommandWallet is null && merchant.PostingPaymentChannel == PostingPaymentChannel.Wallet)
                    {
                        throw new ActiveMerchantWalletRequiredException();
                    }

                    if (activeCommandWallet is not null)
                    {
                        if (activeMerchantWallet is null)
                        {
                            await _context.MerchantWallet.AddAsync(new MerchantWallet
                            {
                                MerchantId = merchant.Id,
                                WalletNumber = activeCommandWallet.WalletNumber.Trim().Replace(" ", string.Empty),
                                RecordStatus = RecordStatus.Active
                            });
                        }
                        else if (activeMerchantWallet.WalletNumber != activeCommandWallet.WalletNumber)
                        {
                            merchant.MerchantWallets.ForEach(s => { s.RecordStatus = RecordStatus.Passive; });
                            _context.MerchantWallet.UpdateRange(merchant.MerchantWallets);
                            await _context.MerchantWallet.AddAsync(new MerchantWallet
                            {
                                MerchantId = merchant.Id,
                                WalletNumber = activeCommandWallet.WalletNumber.Trim().Replace(" ", string.Empty),
                                RecordStatus = RecordStatus.Active
                            });
                        }
                    }
                }

                if (command.MerchantScores.Any())
                {
                    if (merchant.MerchantScores.Any())
                    {
                        var scoreItem = merchant.MerchantScores.FirstOrDefault();
                        var newScoreItem = command.MerchantScores.FirstOrDefault();

                        scoreItem.GoogleRank = newScoreItem.GoogleRank;
                        scoreItem.FindeksScore = newScoreItem.FindeksScore;
                        scoreItem.HasFindeksRiskReport = newScoreItem.HasFindeksRiskReport;
                        scoreItem.HasScoreCard = newScoreItem.HasScoreCard;
                        scoreItem.AlexaRank = newScoreItem.AlexaRank;
                        scoreItem.ScoreCardScore = newScoreItem.ScoreCardScore;
                        scoreItem.MerchantId = merchant.Id;

                        _context.MerchantScore.Update(scoreItem);
                    }
                    else
                    {
                        var merchantScore = command.MerchantScores.Select(b => new MerchantScore
                        {
                            CreateDate = DateTime.Now,
                            CreatedBy = parseUserId.ToString(),
                            RecordStatus = RecordStatus.Active,
                            GoogleRank = b.GoogleRank,
                            FindeksScore = b.FindeksScore,
                            HasFindeksRiskReport = b.HasFindeksRiskReport,
                            HasScoreCard = b.HasScoreCard,
                            AlexaRank = b.AlexaRank,
                            ScoreCardScore = b.ScoreCardScore,
                            MerchantId = merchant.Id,
                        }).FirstOrDefault();

                        await _context.MerchantScore.AddAsync(merchantScore);
                    }
                }

                if (command.MerchantVposList.Any())
                {
                    foreach (var vposItem in command.MerchantVposList)
                    {
                        var merchantVposItem = merchant.MerchantVposList.FirstOrDefault(x => x.VposId == vposItem.VposId);

                        var vpos = await _vposRepository.GetByIdAsync(vposItem.VposId);

                        var terminalStatus = TerminalStatus.PendingRequest;

                        if (vpos.IsTopUpVpos == true || vpos.IsOnUsVpos == true)
                        {
                            terminalStatus = TerminalStatus.Active;
                        }

                        if (merchantVposItem != null && merchantVposItem.Id != Guid.Empty)
                        {
                            merchantVposItem.VposId = vposItem.VposId;
                            merchantVposItem.MerchantId = merchant.Id;
                            merchantVposItem.Priority = vposItem.Priority;
                            merchantVposItem.SubMerchantCode = vposItem.SubMerchantCode;
                            merchantVposItem.TerminalNo = vposItem.TerminalNo;
                            merchantVposItem.ProviderKey = vposItem.ProviderKey;
                            merchantVposItem.ApiKey = vposItem.ApiKey;
                            merchantVposItem.Password = vposItem.Password;
                            merchantVposItem.TerminalStatus = terminalStatus;

                            _context.MerchantVpos.Update(merchantVposItem);
                        }
                        else
                        {
                            var merchantVposOb = new MerchantVpos()
                            {
                                VposId = vposItem.VposId,
                                MerchantId = merchant.Id,
                                Priority = vposItem.Priority,
                                SubMerchantCode = vposItem.SubMerchantCode,
                                TerminalNo = vposItem.TerminalNo,
                                ProviderKey = vposItem.ProviderKey,
                                ApiKey = vposItem.ApiKey,
                                Password = vposItem.Password,
                                TerminalStatus = terminalStatus
                            };

                            await _context.MerchantVpos.AddAsync(merchantVposOb);
                        }
                    }
                }

                if (command.MerchantDocuments.Any())
                {
                    foreach (var documentItem in command.MerchantDocuments)
                    {
                        var merchantsDocument = merchant.MerchantDocuments.FirstOrDefault(x => x.DocumentId == documentItem.DocumentId);

                        if (merchantsDocument != null && merchantsDocument.DocumentId != Guid.Empty)
                        {
                            merchantsDocument.DocumentId = documentItem.DocumentId;
                            merchantsDocument.DocumentTypeId = documentItem.DocumentTypeId;
                            merchantsDocument.DocumentName = documentItem.DocumentName;
                            merchantsDocument.MerchantId = merchant.Id;
                            merchantsDocument.UpdateDate = DateTime.Now;
                            merchantsDocument.RecordStatus = documentItem.RecordStatus;

                            _context.MerchantDocument.Update(merchantsDocument);
                        }
                        else
                        {
                            var merchantDocOb = new MerchantDocument()
                            {
                                DocumentId = documentItem.DocumentId,
                                DocumentTypeId = documentItem.DocumentTypeId,
                                DocumentName = documentItem.DocumentName,
                                MerchantId = merchant.Id,
                                UpdateDate = DateTime.Now,
                                RecordStatus = documentItem.RecordStatus,
                            };

                            await _context.MerchantDocument.AddAsync(merchantDocOb);
                        }

                    }
                }

                await _context.SaveChangesAsync();

                scope.Complete();

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdateMerchant",
                        SourceApplication = "PF",
                        Resource = "Merchant",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                    {"Id", command.Id.ToString() },
                    {"Name", command.Name },
                        }
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError($"MerchantSaveError : {exception}");
                throw;
            }
        });

        return new MerchantResponse
        {
            MerchantStatus = merchant.MerchantStatus,
            MerchantId = merchant.Id
        };

    }
    private async Task UserBlacklistControlAsync(SearchByNameRequest searchRequest)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        var blackListTitleControl = await _searchService.GetSearchByName(searchRequest);

        if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
        {
            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
        }
    }
    private async Task<Merchant> ValidateMerchantAsync(UpdateMerchantCommand command)
    {
        var merchant = await _repository.GetAll()
            .Include(b => b.MerchantBankAccounts)
            .Include(b => b.MerchantWallets)
            .Include(b => b.MerchantVposList)
            .Include(b => b.MerchantScores)
            .Include(b => b.MerchantDocuments)
            .Include(b=>b.MerchantPhysicalDevices)
            .Include(b => b.Customer)
            .ThenInclude(c => c.AuthorizedPerson)
            .Include(c => c.TechnicalContact)
            .FirstOrDefaultAsync(b => b.Id == command.Id);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), command.Id);
        }

        if (command.MerchantBankAccounts.Any())
        {
            var activeCommandBankAccount = command.MerchantBankAccounts
                .FirstOrDefault(m => m.RecordStatus == RecordStatus.Active);

            if (activeCommandBankAccount is not null && activeCommandBankAccount.BankCode != -1)
            {
                var bank = await _context.Bank
                    .FirstOrDefaultAsync(b => b.Code == activeCommandBankAccount.BankCode);

                if (bank is null)
                {
                    throw new NotFoundException(nameof(Bank), activeCommandBankAccount.BankCode);
                }
            }
        }

        if (merchant.MerchantStatus != MerchantStatus.Draft && merchant.PosType != command.PosType)
        {
            throw new CannotChangePosTypeException();
        }

        if ((command.MerchantVposList.Any() || merchant.MerchantVposList.Any(b=>b.RecordStatus == RecordStatus.Active)) && command.PosType == PosType.Physical)
        {
            throw new InvalidPosTypeException();
        }

        if (merchant.MerchantPhysicalDevices.Any(b => b.RecordStatus == RecordStatus.Active) && command.PosType == PosType.Virtual)
        {
            throw new InvalidPosTypeException();
        }

        var mccCode = await _context.Mcc
            .FirstOrDefaultAsync(b => b.Code == command.MccCode);

        if (mccCode is null && command.MerchantStatus != MerchantStatus.Draft)
        {
            throw new NotFoundException(nameof(Mcc), command.MccCode);
        }
        
        var naceCode = await _context.Nace
            .FirstOrDefaultAsync(b => b.Code == command.NaceCode);
        
        if (naceCode is null && command.MerchantStatus != MerchantStatus.Draft)
        {
            throw new NotFoundException(nameof(Nace), command.NaceCode);
        }

        var pricingProfile = await _context.PricingProfile
            .FirstOrDefaultAsync(b => b.PricingProfileNumber == command.PricingProfileNumber && b.ProfileStatus == ProfileStatus.InUse);

        if (pricingProfile is null && command.MerchantStatus != MerchantStatus.Draft)
        {
            throw new NotFoundException(nameof(PricingProfile), command.PricingProfileNumber);
        }

        if (pricingProfile is not null &&
                (
                    (merchant.MerchantType is MerchantType.SubMerchant && pricingProfile.ProfileType == ProfileType.Standard)
                    ||
                    ((merchant.MerchantType is MerchantType.MainMerchant or MerchantType.StandartMerchant or MerchantType.EasyMerchant) && pricingProfile.ProfileType == ProfileType.SubMerchant)
                )
            )
        {
            throw new InvalidPricingProfileTypeException();
        }

        return merchant;
    }
    private async Task CheckValidationsAsync(UpdateMerchantCommand command, Merchant merchant)
    {
        var isKkbEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "KkbEnabled");

        var isKpsEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "PFKpsEnabled");

        var IsBlacklistCheckEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");

        if (IsBlacklistCheckEnabled)
        {
            var oldTitle = merchant.Customer.CommercialTitle;
            var oldAuthorizedPerson = $"{merchant.Customer.AuthorizedPerson.Name}{merchant.Customer.AuthorizedPerson.Surname}";

            if (!oldTitle.Equals(command.Customer.CommercialTitle))
            {
                SearchByNameRequest searchTitleRequest = new()
                {
                    Name = command.Customer.CommercialTitle,
                    SearchType = SearchType.Corporate,
                    FraudChannelType = FraudChannelType.Backoffice
                };

                await UserBlacklistControlAsync(searchTitleRequest);
            }

            if (!oldAuthorizedPerson.Equals($"{command.Customer.AuthorizedPerson.Name}{command.Customer.AuthorizedPerson.Surname}"))
            {
                SearchByNameRequest searchRequest = new()
                {
                    Name = $"{command.Customer.AuthorizedPerson.Name} {command.Customer.AuthorizedPerson.Surname}",
                    BirthYear = command.Customer.AuthorizedPerson.BirthDate.Year.ToString(),
                    SearchType = SearchType.Corporate,
                    FraudChannelType = FraudChannelType.Backoffice
                };

                await UserBlacklistControlAsync(searchRequest);
            }
        }

        if (isKkbEnabled)
        {
            var isValid = await ValidateIbanAsync(command);

            if (!isValid)
            {
                _logger.LogError($"PatchMerchantCommandError: IBAN is not valid : {command.MerchantBankAccounts.FirstOrDefault()?.Iban}");
                throw new IbanValidationFailedException();
            }
        }

        if (isKpsEnabled)
        {
            var isValid = await ValidateIdentityAsync(command);
            if (!isValid)
            {
                _logger.LogError($"PatchMerchantCommandError: Identity is not valid");
                throw new IdentityValidationFailedException();
            }
        }
    }
    private async Task<bool> ValidateIbanAsync(UpdateMerchantCommand merchant)
    {
        var validateIban = new ValidateIbanRequest()
        {
            Iban = merchant.MerchantBankAccounts.FirstOrDefault()?.Iban,
            IdentityNo = merchant.Customer.CompanyType == CompanyType.Individual
        ? merchant.Customer.AuthorizedPerson.IdentityNumber
        : merchant.Customer.TaxNumber,
        };
        var res = await _kkbService.ValidateIban(validateIban);
        return res.IsValid;
    }
    private async Task<bool> ValidateIdentityAsync(UpdateMerchantCommand merchant)
    {
        var validateIdentity = new ValidateIdentityRequest()
        {
            IdentityNo = merchant.Customer.AuthorizedPerson.IdentityNumber,
            Name = merchant.Customer.AuthorizedPerson.Name,
            Surname = merchant.Customer.AuthorizedPerson.Surname,
            BirthDate = merchant.Customer.AuthorizedPerson.BirthDate
        };
        var res = await _kpsService.ValidateIdentity(validateIdentity);
        return res.IsValid;
    }
    private async Task UpdateCustomerAsync(Merchant merchant, CustomerDto newCustomer)
    {
        if (merchant.Customer.Address != newCustomer.Address)
        {
            var addAddressRequest = new AddAddressRequest
            {
                CustomerId = merchant.Customer.CustomerId,
                CountryId = merchant.Customer.Country,
                Country = merchant.Customer.CountryName,
                CityId = newCustomer.City,
                City = newCustomer.CityName,
                DistrictId = newCustomer.District,
                District = newCustomer.DistrictName,
                PostalCode = newCustomer.PostalCode,
                Address = newCustomer.Address,
                Primary = true,
                CityIso2 = null,
                CountryIso2 = null,
                AddressType = AddressType.Company
            };
            var getCities = await _locationService.GetCityByCode(addAddressRequest.CountryId);
            var getCityIso2 = getCities.FirstOrDefault(x => x.CityCode == merchant.Customer.City);
            addAddressRequest.CityIso2 = getCityIso2?.Iso2;
            await _customerService.AddCustomerAddressAsync(addAddressRequest);
        }

        merchant.Customer.CompanyType = newCustomer.CompanyType;
        merchant.Customer.CommercialTitle = newCustomer.CommercialTitle;
        merchant.Customer.TradeRegistrationNumber = newCustomer.TradeRegistrationNumber;
        merchant.Customer.TaxAdministration = newCustomer.TaxAdministration;
        merchant.Customer.TaxNumber = newCustomer.TaxNumber;
        merchant.Customer.City = newCustomer.City;
        merchant.Customer.CityName = newCustomer.CityName;
        merchant.Customer.Country = newCustomer.Country;
        merchant.Customer.CountryName = newCustomer.CountryName;
        merchant.Customer.District = newCustomer.District;
        merchant.Customer.DistrictName = newCustomer.DistrictName;
        merchant.Customer.PostalCode = newCustomer.PostalCode;
        merchant.Customer.Address = newCustomer.Address;

        _context.Customer.Update(merchant.Customer);
    }

    private static void UpdateAuthorizedPerson(Merchant merchant, MerchantContactPersonDto newAuthorizedPerson)
    {
        var authorizedPerson = merchant.Customer.AuthorizedPerson;

        authorizedPerson.ContactPersonType = ContactPersonType.AuthorizedPerson;
        authorizedPerson.Name = newAuthorizedPerson.Name;
        authorizedPerson.Surname = newAuthorizedPerson.Surname;
        authorizedPerson.IdentityNumber = newAuthorizedPerson.IdentityNumber;
        authorizedPerson.BirthDate = newAuthorizedPerson.BirthDate;
        authorizedPerson.CompanyPhoneNumber = newAuthorizedPerson.CompanyPhoneNumber;
        authorizedPerson.MobilePhoneNumber = newAuthorizedPerson.MobilePhoneNumber;
        authorizedPerson.MobilePhoneNumberSecond = newAuthorizedPerson.MobilePhoneNumberSecond;
        authorizedPerson.Email = newAuthorizedPerson.Email;
        authorizedPerson.CompanyEmail = newAuthorizedPerson.CompanyEmail;
    }

    private static void UpdateTechnicalContact(Merchant merchant, MerchantContactPersonDto newTechnicalContact)
    {
        var technicalContact = merchant.TechnicalContact;

        technicalContact.ContactPersonType = ContactPersonType.TechnicalPerson;
        technicalContact.Name = newTechnicalContact.Name;
        technicalContact.Surname = newTechnicalContact.Surname;
        technicalContact.IdentityNumber = newTechnicalContact.IdentityNumber;
        technicalContact.BirthDate = newTechnicalContact.BirthDate;
        technicalContact.CompanyPhoneNumber = newTechnicalContact.CompanyPhoneNumber;
        technicalContact.MobilePhoneNumber = newTechnicalContact.MobilePhoneNumber;
        technicalContact.MobilePhoneNumberSecond = newTechnicalContact.MobilePhoneNumberSecond;
        technicalContact.Email = newTechnicalContact.Email;
        technicalContact.CompanyEmail = newTechnicalContact.CompanyEmail;
    }

    private async Task UpdateCustomerStatusAsync(Merchant merchant)
    {
        if (merchant.Customer is not null)
        {
            var customerRequest = await PopulateCustomerRequestAsync(merchant);

            merchant.Customer.CustomerStatus = merchant.MerchantStatus switch
            {
                MerchantStatus.Reject => CustomerStatus.Rejected,
                MerchantStatus.Active => CustomerStatus.Active,
                _ => merchant.Customer.CustomerStatus
            };

            var createCustomerResponse = await _customerService.CreateCustomerAsync(customerRequest);
            if (createCustomerResponse.IsChanged)
            {
                merchant.Customer.CustomerNumber = createCustomerResponse.CustomerNumber;
                merchant.Customer.CustomerId = createCustomerResponse.CustomerId;
            }
            await _customerRepository.UpdateAsync(merchant.Customer);

        }
    }

    private async Task<CreateCustomerRequest> PopulateCustomerRequestAsync(Merchant merchant)
    {
        var customer = merchant.Customer;
        var parentCustomer = await _customerService.GetCustomerAsync(customer.CustomerId);
        var customerType = customer.CompanyType switch
        {
            CompanyType.Corporate => CustomerType.Corporate,
            CompanyType.Enterprise => CustomerType.Enterprise,
            _ => CustomerType.Individual
        };

        var customerRequest = new CreateCustomerRequest
        {
            UserId = _contextProvider.CurrentContext.UserId != null
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            CustomerId = customer.CustomerId,
            CommercialTitle = customer.CommercialTitle,
            TradeRegistrationNumber = customer.TradeRegistrationNumber,
            TaxAdministration = customer.TaxAdministration,
            TaxNumber = customer.TaxNumber,
            CreateCustomerProducts = parentCustomer.CustomerProducts,
            CustomerType = customerType
        };

        if (customer.CompanyType == CompanyType.Individual)
        {
            customerRequest.FirstName = customer.AuthorizedPerson.Name;
            customerRequest.LastName = customer.AuthorizedPerson.Surname;
            customerRequest.BirthDate = customer.AuthorizedPerson.BirthDate;
            customerRequest.NationCountry = "TR";
            customerRequest.NationCountryId = "TR";
            customerRequest.IdentityNumber = customer.AuthorizedPerson.IdentityNumber;
            customerRequest.DocumentType = DocumentType.Identity;
        }

        var pfProduct = customerRequest.CreateCustomerProducts.FirstOrDefault(s => s.MerchantId == merchant.Id);
        if (pfProduct is null)
        {
            pfProduct = new CustomerProductDto
            {
                OpeningDate = DateTime.Now,
                MerchantId = merchant.Id,
                ProductType = ProductType.PF,
                CustomerProductStatus = CustomerProductStatus.Active
            };
            customerRequest.CreateCustomerProducts.Add(pfProduct);
        }

        var productStatus = merchant.MerchantStatus switch
        {
            MerchantStatus.Active => CustomerProductStatus.Active,
            MerchantStatus.Annulment => CustomerProductStatus.Suspended,
            _ => CustomerProductStatus.Inactive
        };

        if (pfProduct.CustomerProductStatus != productStatus)
        {
            switch (productStatus)
            {
                case CustomerProductStatus.Suspended:
                    pfProduct.SuspendedDate = DateTime.Now;
                    pfProduct.RecordStatus = RecordStatus.Passive;
                    break;
                case CustomerProductStatus.Inactive:
                    pfProduct.ClosingDate = DateTime.Now;
                    pfProduct.RecordStatus = RecordStatus.Passive;
                    break;
                default:
                    pfProduct.ReopeningDate = DateTime.Now;
                    pfProduct.RecordStatus = RecordStatus.Active;
                    break;
            }

            pfProduct.CustomerProductStatus = productStatus;
        }

        customerRequest.CreateCustomerPhones = CreatePhonesDto(merchant);
        customerRequest.CreateCustomerEmails = CreateEmailsDto(merchant);
        customerRequest.CreateCustomerAddresses = CreateAddressesDto();

        return customerRequest;
    }

    private List<CustomerPhoneDto> CreatePhonesDto(Merchant merchant)
    {
        var phoneList = new List<CustomerPhoneDto>();
        var customerIndividualPhone = new CustomerPhoneDto()
        {
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = merchant.Customer.AuthorizedPerson.MobilePhoneNumber,
            Primary = true,
            PhoneType = PhoneType.Individual,
        };
        var customerCompanyPhone = new CustomerPhoneDto()
        {
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = merchant.Customer.AuthorizedPerson.CompanyPhoneNumber,
            Primary = false,
            PhoneType = PhoneType.Company,
        };

        phoneList.Add(customerIndividualPhone);
        phoneList.Add(customerCompanyPhone);

        return phoneList;
    }

    private List<CustomerEmailDto> CreateEmailsDto(Merchant merchant)
    {
        var emailList = new List<CustomerEmailDto>();
        var customerCompanyEmail = new CustomerEmailDto()
        {
            Email = merchant.Customer.AuthorizedPerson.CompanyEmail,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Company,
            Primary = true,

        };
        var customerIndividualEmail = new CustomerEmailDto()
        {
            Email = merchant.Customer.AuthorizedPerson.Email,
            EmailType = HttpProviders.CustomerManagement.Models.Enums.EmailType.Individual,
            Primary = false,

        };
        emailList.Add(customerIndividualEmail);
        emailList.Add(customerCompanyEmail);

        return emailList;
    }

    private List<CustomerAddressDto> CreateAddressesDto()
    {
        return new List<CustomerAddressDto>();
    }

    public async Task UpdateMerchantHistory(List<MerchantHistoryDto> command)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

        var user = await _userService.GetUserAsync(Guid.Parse(parseUserId));

        var merchantHistoryList = command.Select(b => new MerchantHistory
        {
            NewData = b.NewData,
            PermissionOperationType = b.PermissionOperationType,
            Detail = b.Detail,
            OldData = b.OldData,
            MerchantId = b.MerchantId,
            CreatedNameBy = user is not null ? (user.FirstName + " " + user.LastName) : string.Empty,
        }).ToList();

        await _context.MerchantHistory.AddRangeAsync(merchantHistoryList);

        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedList<MerchantTransactionDto>> GetMerchantTransactionList(GetAllMerchantTransactionQuery request)
    {
        var transactions = _context.MerchantTransaction
            .Include(b => b.AcquireBank).Include(b => b.IssuerBank)
            .Include(b => b.Merchant)
            .Where(b => b.TransactionStatus != Domain.Enums.TransactionStatus.Pending);

        if (!string.IsNullOrEmpty(request.CardFirstNumbers))
        {
            transactions = transactions.Where(b => b.CardNumber.Substring(0, 6).Contains(request.CardFirstNumbers));
        }

        if (!string.IsNullOrEmpty(request.CardLastNumbers))
        {
            transactions = transactions.Where(b => b.CardNumber.Substring(12, 4).Contains(request.CardLastNumbers));
        }

        if (!string.IsNullOrEmpty(request.BankOrderId))
        {
            transactions = transactions.Where(b => b.OrderId.Contains(request.BankOrderId));
        }

        if (!string.IsNullOrEmpty(request.ConversationId))
        {
            transactions = transactions.Where(b => b.ConversationId.Contains(request.ConversationId));
        }

        if (request.AcquireBankCode is not null)
        {
            transactions = transactions.Where(b => b.AcquireBankCode
                               == request.AcquireBankCode);
        }
        
        if (request.IssuerBankCode is not null)
        {
            transactions = transactions.Where(b => b.IssuerBankCode
                                                   == request.IssuerBankCode);
        }

        if (request.MerchantId is not null)
        {
            transactions = transactions.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.SubMerchantId is not null)
        {
            transactions = transactions.Where(b => b.SubMerchantId
                               == request.SubMerchantId);
        }

        if (request.ParentMerchantId is not null)
        {
            transactions = transactions.Where(b => b.Merchant.ParentMerchantId
                               == request.ParentMerchantId);
        }

        if (request.CreateDateStart is not null)
        {
            transactions = transactions.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            transactions = transactions.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.TransactionStatus is not null)
        {
            transactions = transactions.Where(b => b.TransactionStatus
                               == request.TransactionStatus);
        }

        if (request.TransactionType is not null)
        {
            transactions = transactions.Where(b => b.TransactionType
                               == request.TransactionType);
        }

        if (request.IsChargeBack is not null)
        {
            transactions = transactions.Where(b => b.IsChargeback == request.IsChargeBack);
        }

        if (request.IsSuspecious is not null)
        {
            transactions = transactions.Where(b => b.IsSuspecious == request.IsSuspecious);
        }

        if (request.IsManualReturn is not null)
        {
            transactions = transactions.Where(b => b.IsManualReturn == request.IsManualReturn);
        }

        if (request.IsOnUsPayment is not null)
        {
            transactions = transactions.Where(b => b.IsOnUsPayment == request.IsOnUsPayment);
        }       
        
        if (request.IsInsurancePayment is not null)
        {
            transactions = transactions.Where(b => b.IsInsurancePayment == request.IsInsurancePayment);
        }

        if (request.IsPerInstallment is not null)
        {
            transactions = transactions.Where(b => b.IsPerInstallment == request.IsPerInstallment);
        }

        if (!string.IsNullOrEmpty(request.CreatedNameBy))
        {
            transactions = transactions.Where(b => b.CreatedNameBy.ToLower().Contains(request.CreatedNameBy.ToLower()));
        }

        if (request.BlockageStatus is not null)
        {
            transactions = transactions.Where(b => b.BlockageStatus
                                                   == request.BlockageStatus);
        }

        if (request.PfTransactionSource is not null)
        {
            transactions = transactions.Where(b => b.PfTransactionSource
                                                   == request.PfTransactionSource);
        }

        var mapMerchantTransactionList = await (
          from trans in transactions
          join parentMerchant in _context.Merchant
              on trans.Merchant.ParentMerchantId equals parentMerchant.Id into parentMerchantGroup
          from parentMerchant in parentMerchantGroup.DefaultIfEmpty()
          select new MerchantTransactionDto
          {
              Id = trans.Id,
              MerchantId = trans.MerchantId,
              SubMerchantId = trans.SubMerchantId,
              SubMerchantName = trans.SubMerchantName,
              SubMerchantNumber = trans.SubMerchantNumber,
              ParentMerchantName = parentMerchant != null ? parentMerchant.Name : null,
              ParentMerchantNumber = parentMerchant != null ? parentMerchant.Number : null,
              Merchant = _mapper.Map<TransactionMerchantResponse>(trans.Merchant),
              ConversationId = trans.ConversationId,
              IpAddress = trans.IpAddress,
              TransactionType = trans.TransactionType,
              TransactionStatus = trans.TransactionStatus,
              OrderId = trans.OrderId,
              Amount = trans.Amount,
              PointAmount = trans.PointAmount,
              PointCommissionAmount = trans.PointCommissionAmount,
              PointCommissionRate = trans.PointCommissionRate,
              ServiceCommissionAmount = trans.ServiceCommissionAmount,
              ServiceCommissionRate = trans.ServiceCommissionRate,
              Currency = trans.Currency,
              InstallmentCount = trans.InstallmentCount,
              BinNumber = trans.BinNumber,
              CardNumber = trans.CardNumber,
              HasCvv = trans.HasCvv,
              HasExpiryDate = trans.HasExpiryDate,
              IsInternational = trans.IsInternational,
              IsAmex = trans.IsAmex,
              IsReverse = trans.IsReverse,
              IsManualReturn = trans.IsManualReturn,
              IsOnUsPayment = trans.IsOnUsPayment,
              IsInsurancePayment = trans.IsInsurancePayment,
              ReverseDate = trans.ReverseDate,
              IsReturn = trans.IsReturn,
              ReturnDate = trans.ReturnDate,
              ReturnAmount = trans.ReturnAmount,
              ReturnedTransactionId = trans.ReturnedTransactionId,
              IsPreClose = trans.IsPreClose,
              PreCloseDate = trans.PreCloseDate,
              PreCloseTransactionId = trans.PreCloseTransactionId,
              Is3ds = trans.Is3ds,
              ThreeDSessionId = trans.ThreeDSessionId,
              BankCommissionRate = trans.BankCommissionRate,
              BankCommissionAmount = trans.BankCommissionAmount,
              IssuerBankCode = trans.IssuerBankCode,
              IssuerBank = trans.IssuerBank,
              AcquireBankCode = trans.AcquireBankCode,
              AcquireBank = trans.AcquireBank,
              CardTransactionType = trans.CardTransactionType,
              IntegrationMode = trans.IntegrationMode,
              ResponseCode = trans.ResponseCode,
              ResponseDescription = trans.ResponseDescription,
              TransactionStartDate = trans.TransactionStartDate,
              TransactionEndDate = trans.TransactionEndDate,
              VposId = trans.VposId,
              LanguageCode = trans.LanguageCode,
              BatchStatus = trans.BatchStatus,
              CardType = trans.CardType,
              TransactionDate = trans.TransactionDate,
              IsChargeBack = trans.IsChargeback,
              IsSuspecious = trans.IsSuspecious,
              SuspeciousDescription = trans.SuspeciousDescription,
              LastChargebackActivityDate = trans.LastChargebackActivityDate,
              CreateDate = trans.CreateDate,
              MerchantCustomerName = trans.MerchantCustomerName,
              MerchantCustomerPhoneCode = trans.MerchantCustomerPhoneCode,
              MerchantCustomerPhoneNumber = trans.MerchantCustomerPhoneNumber,
              Description = trans.Description,
              CardHolderName = trans.CardHolderName,
              ReturnStatus = trans.ReturnStatus,
              CreatedNameBy = trans.CreatedNameBy,
              PfCommissionAmount = trans.PfCommissionAmount,
              PfNetCommissionAmount = trans.PfNetCommissionAmount,
              PfCommissionRate = trans.PfCommissionRate,
              PfPerTransactionFee = trans.PfPerTransactionFee,
              ParentMerchantCommissionAmount = trans.ParentMerchantCommissionAmount,
              ParentMerchantCommissionRate = trans.ParentMerchantCommissionRate,
              AmountWithoutCommissions = trans.AmountWithoutCommissions,
              AmountWithoutBankCommission = trans.AmountWithoutBankCommission,
              AmountWithoutParentMerchantCommission = trans.AmountWithoutParentMerchantCommission,
              PricingProfileItemId = trans.PricingProfileItemId,
              BsmvAmount = trans.BsmvAmount,
              ProvisionNumber = trans.ProvisionNumber,
              VposName = trans.VposName,
              PfPaymentDate = trans.PfPaymentDate,
              BankPaymentDate = trans.BankPaymentDate,
              BlockageStatus = trans.BlockageStatus,
              IsTopUpPayment = trans.IsTopUpPayment,
              PfTransactionSource = trans.PfTransactionSource,
              CardHolderIdentityNumber = trans.CardHolderIdentityNumber,
              EndOfDayStatus = trans.EndOfDayStatus,
              MerchantPhysicalPosId = trans.MerchantPhysicalPosId,
              PhysicalPosEodId = trans.PhysicalPosEodId,
              PhysicalPosOldEodId = trans.PhysicalPosOldEodId,
              IsPerInstallment = trans.IsPerInstallment
          }).OrderByDescending(b => b.CreateDate)
          .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);

        return mapMerchantTransactionList;
    }

    public async Task<MerchantTransactionDto> GetMerchantTransactionByIdAsync(Guid id)
    {
        var transaction = await (
        from trans in _context.MerchantTransaction
            .Include(b => b.AcquireBank)
            .Include(b => b.IssuerBank)
            .Include(b => b.Merchant)
        join parentMerchant in _context.Merchant
            on trans.Merchant.ParentMerchantId equals parentMerchant.Id into parentGroup
        from parent in parentGroup.DefaultIfEmpty()
        where trans.Id == id
        select new MerchantTransactionDto
        {
            Id = trans.Id,
            MerchantId = trans.MerchantId,
            SubMerchantId = trans.SubMerchantId,
            SubMerchantName = trans.SubMerchantName,
            SubMerchantNumber = trans.SubMerchantNumber,
            ParentMerchantName = parent != null ? parent.Name : null,
            ParentMerchantNumber = parent != null ? parent.Number : null,
            Merchant = _mapper.Map<TransactionMerchantResponse>(trans.Merchant),
            ConversationId = trans.ConversationId,
            IpAddress = trans.IpAddress,
            TransactionType = trans.TransactionType,
            TransactionStatus = trans.TransactionStatus,
            OrderId = trans.OrderId,
            Amount = trans.Amount,
            PointAmount = trans.PointAmount,
            PointCommissionAmount = trans.PointCommissionAmount,
            PointCommissionRate = trans.PointCommissionRate,
            ServiceCommissionAmount = trans.ServiceCommissionAmount,
            ServiceCommissionRate = trans.ServiceCommissionRate,
            Currency = trans.Currency,
            InstallmentCount = trans.InstallmentCount,
            BinNumber = trans.BinNumber,
            CardNumber = trans.CardNumber,
            HasCvv = trans.HasCvv,
            HasExpiryDate = trans.HasExpiryDate,
            IsInternational = trans.IsInternational,
            IsAmex = trans.IsAmex,
            IsReverse = trans.IsReverse,
            IsManualReturn = trans.IsManualReturn,
            IsOnUsPayment = trans.IsOnUsPayment,
            IsInsurancePayment = trans.IsInsurancePayment,
            ReverseDate = trans.ReverseDate,
            IsReturn = trans.IsReturn,
            ReturnDate = trans.ReturnDate,
            ReturnAmount = trans.ReturnAmount,
            ReturnedTransactionId = trans.ReturnedTransactionId,
            IsPreClose = trans.IsPreClose,
            PreCloseDate = trans.PreCloseDate,
            PreCloseTransactionId = trans.PreCloseTransactionId,
            Is3ds = trans.Is3ds,
            ThreeDSessionId = trans.ThreeDSessionId,
            BankCommissionRate = trans.BankCommissionRate,
            BankCommissionAmount = trans.BankCommissionAmount,
            IssuerBankCode = trans.IssuerBankCode,
            IssuerBank = trans.IssuerBank,
            AcquireBankCode = trans.AcquireBankCode,
            AcquireBank = trans.AcquireBank,
            CardTransactionType = trans.CardTransactionType,
            IntegrationMode = trans.IntegrationMode,
            ResponseCode = trans.ResponseCode,
            ResponseDescription = trans.ResponseDescription,
            TransactionStartDate = trans.TransactionStartDate,
            TransactionEndDate = trans.TransactionEndDate,
            VposId = trans.VposId,
            LanguageCode = trans.LanguageCode,
            BatchStatus = trans.BatchStatus,
            CardType = trans.CardType,
            TransactionDate = trans.TransactionDate,
            IsChargeBack = trans.IsChargeback,
            IsSuspecious = trans.IsSuspecious,
            SuspeciousDescription = trans.SuspeciousDescription,
            LastChargebackActivityDate = trans.LastChargebackActivityDate,
            CreateDate = trans.CreateDate,
            MerchantCustomerName = trans.MerchantCustomerName,
            MerchantCustomerPhoneCode = trans.MerchantCustomerPhoneCode,
            MerchantCustomerPhoneNumber = trans.MerchantCustomerPhoneNumber,
            Description = trans.Description,
            CardHolderName = trans.CardHolderName,
            ReturnStatus = trans.ReturnStatus,
            CreatedNameBy = trans.CreatedNameBy,
            PfCommissionAmount = trans.PfCommissionAmount,
            PfNetCommissionAmount = trans.PfNetCommissionAmount,
            PfCommissionRate = trans.PfCommissionRate,
            PfPerTransactionFee = trans.PfPerTransactionFee,
            ParentMerchantCommissionAmount = trans.ParentMerchantCommissionAmount,
            ParentMerchantCommissionRate = trans.ParentMerchantCommissionRate,
            AmountWithoutCommissions = trans.AmountWithoutCommissions,
            AmountWithoutBankCommission = trans.AmountWithoutBankCommission,
            AmountWithoutParentMerchantCommission = trans.AmountWithoutParentMerchantCommission,
            PricingProfileItemId = trans.PricingProfileItemId,
            BsmvAmount = trans.BsmvAmount,
            ProvisionNumber = trans.ProvisionNumber,
            VposName = trans.VposName,
            PfPaymentDate = trans.PfPaymentDate,
            BankPaymentDate = trans.BankPaymentDate,
            BlockageStatus = trans.BlockageStatus,
            PfTransactionSource = trans.PfTransactionSource,
            CardHolderIdentityNumber = trans.CardHolderIdentityNumber,
            EndOfDayStatus = trans.EndOfDayStatus,
            MerchantPhysicalPosId = trans.MerchantPhysicalPosId,
            PhysicalPosEodId = trans.PhysicalPosEodId,
            PhysicalPosOldEodId = trans.PhysicalPosOldEodId
        }).FirstOrDefaultAsync(b => b.Id == id);

        if (transaction is null)
        {
            throw new NotFoundException(nameof(MerchantTransaction), id);
        }

        var bankTransaction = await _bankTransactionRepository.GetAll()
            .Include(b => b.AcquireBank).Include(b => b.IssuerBank)
            .FirstOrDefaultAsync(b => b.MerchantTransactionId == transaction.Id);

        if (transaction.PfTransactionSource == PfTransactionSource.VirtualPos)
        {
            var vpos = await _vposRepository.GetAll().Include(b => b.VposBankApiInfos
                    .Where(b => b.RecordStatus == RecordStatus.Active))
                .ThenInclude(b => b.Key)
                .FirstOrDefaultAsync(b => b.Id == transaction.VposId);
            transaction.Vpos = _mapper.Map<VposDto>(vpos);
        }
        
        transaction.BankTransaction = _mapper.Map<BankTransactionDto>(bankTransaction);

        if (transaction.IsPreClose && transaction.PreCloseTransactionId is not null)
        {
            transaction.PreCloseAmount =
                await _merchantTransactionRepository
                    .GetAll()
                    .Where(x => x.Id == Guid.Parse(transaction.PreCloseTransactionId))
                    .Select(x => x.Amount)
                    .FirstOrDefaultAsync();
        }

        return transaction;
    }

    public async Task MerchantPermissionBatchUpdateAsync(BulkPermissionUpdateCommand request)
    {
        var query = _context.Merchant.Include(b => b.Customer).AsQueryable();

        if (request.ParentMerchantIdList != null && request.ParentMerchantIdList.Any())
        {
            var parentIds = request.ParentMerchantIdList.ToList();
            query = query.Where(m => parentIds.Contains(m.ParentMerchantId.Value));
        }

        if (request.MainSubMerchantId.HasValue)
            query = query.Where(m => m.Id == request.MainSubMerchantId.Value);

        if (request.CityCode.HasValue)
            query = query.Where(m => m.Customer.City == request.CityCode.Value);

        if (request.IntegrationMode.HasValue)
        {
            var mode = request.IntegrationMode.ToString();
            query = query
                    .Where(b => ((string)(object)b.IntegrationMode).Contains(mode));
        }

        if (request.MerchantType.HasValue)
            query = query.Where(m => m.MerchantType == request.MerchantType.Value);

        if (request.MerchantStatus.HasValue)
            query = query.Where(m => m.MerchantStatus == request.MerchantStatus.Value);

        if (request.CreateDateStart.HasValue)
            query = query.Where(m => m.CreateDate >= request.CreateDateStart.Value);

        if (request.CreateDateEnd.HasValue)
            query = query.Where(m => m.CreateDate <= request.CreateDateEnd.Value);
        try
        {
            if (request.UpdateIntegrationMode != null)
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    int updatedCount = await query.ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.IntegrationMode, _ => request.UpdateIntegrationMode.Value));
                    _logger.LogError($"Successfully updated {updatedCount} merchants");
                });

            }
            else
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    int updatedCount = await query.ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.FinancialTransactionAllowed, m => request.FinancialTransactionAllowed ?? m.FinancialTransactionAllowed)
                        .SetProperty(m => m.PaymentAllowed, m => request.PaymentAllowed ?? m.PaymentAllowed)
                        .SetProperty(m => m.InstallmentAllowed, m => request.InstallmentAllowed ?? m.InstallmentAllowed)
                        .SetProperty(m => m.Is3dRequired, m => request.Is3dRequired ?? m.Is3dRequired)
                        .SetProperty(m => m.PreAuthorizationAllowed, m => request.PreAuthorizationAllowed ?? m.PreAuthorizationAllowed)
                        .SetProperty(m => m.IsPostAuthAmountHigherAllowed,
                            m => request.IsPostAuthAmountHigherAllowed ?? m.IsPostAuthAmountHigherAllowed)
                        .SetProperty(m => m.InternationalCardAllowed, m => request.InternationalCardAllowed ?? m.InternationalCardAllowed)
                        .SetProperty(m => m.PaymentReturnAllowed, m => request.PaymentReturnAllowed ?? m.PaymentReturnAllowed)
                        .SetProperty(m => m.IsExcessReturnAllowed, m => request.IsExcessReturnAllowed ?? m.IsExcessReturnAllowed)
                        .SetProperty(m => m.IsCvvPaymentAllowed, m => request.IsCvvPaymentAllowed ?? m.IsCvvPaymentAllowed)
                        .SetProperty(m => m.IsReturnApproved, m => request.IsReturnApproved ?? m.IsReturnApproved)
                        .SetProperty(m => m.PaymentReverseAllowed,
                            m => request.PaymentReverseAllowed ?? m.PaymentReverseAllowed));
                    _logger.LogError($"Successfully updated {updatedCount} merchants");
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk update merchant permissions");
            throw;
        }

    }

    public async Task MerchantIntegrationModeBatchUpdateAsync(BulkIntegrationModeUpdateCommand request)
    {

        var query = _context.Merchant.Include(b => b.Customer).AsQueryable();

        if (request.ParentMerchantIdList != null && request.ParentMerchantIdList.Any())
        {
            var parentIds = request.ParentMerchantIdList.ToList();
            query = query.Where(m => parentIds.Contains(m.ParentMerchantId.Value));
        }

        if (request.MainSubMerchantId.HasValue)
            query = query.Where(m => m.Id == request.MainSubMerchantId.Value);

        if (request.CityCode.HasValue)
            query = query.Where(m => m.Customer.City == request.CityCode.Value);

        if (request.IntegrationMode.HasValue)
        {
            var mode = request.IntegrationMode.ToString();
            query = query
                    .Where(b => ((string)(object)b.IntegrationMode).Contains(mode));
        }

        if (request.MerchantType.HasValue)
            query = query.Where(m => m.MerchantType == request.MerchantType.Value);

        if (request.MerchantStatus.HasValue)
            query = query.Where(m => m.MerchantStatus == request.MerchantStatus.Value);

        if (request.CreateDateStart.HasValue)
            query = query.Where(m => m.CreateDate >= request.CreateDateStart.Value);

        if (request.CreateDateEnd.HasValue)
            query = query.Where(m => m.CreateDate <= request.CreateDateEnd.Value);

        var merchantsToUpdate = await query
          .Select(m => new Merchant
          {
              Id = m.Id,
              IntegrationMode = m.IntegrationMode
          }).ToListAsync();

        if (!merchantsToUpdate.Any())
        {
            _logger.LogError("No merchants matched the update criteria.");
            return;
        }

        foreach (var merchant in merchantsToUpdate)
        {
            var integrationMode = merchant.IntegrationMode;

            if (request.IsApiMode.HasValue)
                integrationMode = request.IsApiMode.Value
                    ? integrationMode | IntegrationMode.Api
                    : integrationMode & ~IntegrationMode.Api;

            if (request.IsHppMode.HasValue)
                integrationMode = request.IsHppMode.Value
                    ? integrationMode | IntegrationMode.Hpp
                    : integrationMode & ~IntegrationMode.Hpp;

            if (request.IsManuelPaymentPageMode.HasValue)
                integrationMode = request.IsManuelPaymentPageMode.Value
                    ? integrationMode | IntegrationMode.ManuelPaymentPage
                    : integrationMode & ~IntegrationMode.ManuelPaymentPage;

            if (request.IsLinkPaymentPageMode.HasValue)
                integrationMode = request.IsLinkPaymentPageMode.Value
                    ? integrationMode | IntegrationMode.LinkPaymentPage
                    : integrationMode & ~IntegrationMode.LinkPaymentPage;

            if (request.IsOnUsMode.HasValue)
                integrationMode = request.IsOnUsMode.Value
                    ? integrationMode | IntegrationMode.OnUs
                    : integrationMode & ~IntegrationMode.OnUs;

            merchant.IntegrationMode = integrationMode;
        }
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.BulkUpdateAsync(merchantsToUpdate, new BulkConfig
            {
                UpdateByProperties = new List<string> { "Id" },
                PropertiesToInclude = new List<string> { "IntegrationMode" },
                BatchSize = 1000,
                SetOutputIdentity = false
            });

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Integration mode bulk update failed. Error: {Message}", ex.Message);
            throw;
        }
    }

    public async Task PricingProfileBatchUpdateAsync(BulkPricingProfileUpdateCommand request)
    {
        var query = _context.Merchant.Include(b => b.Customer).AsQueryable();

        if (request.ParentMerchantIdList != null && request.ParentMerchantIdList.Any())
        {
            var parentIds = request.ParentMerchantIdList.ToList();
            query = query.Where(m => parentIds.Contains(m.ParentMerchantId.Value));
        }

        if (request.MainSubMerchantId.HasValue)
            query = query.Where(m => m.Id == request.MainSubMerchantId.Value);

        if (request.CityCode.HasValue)
            query = query.Where(m => m.Customer.City == request.CityCode.Value);

        if (request.IntegrationMode.HasValue)
        {
            var mode = request.IntegrationMode.ToString();
            query = query
                    .Where(b => ((string)(object)b.IntegrationMode).Contains(mode));
        }

        if (request.MerchantType.HasValue)
            query = query.Where(m => m.MerchantType == request.MerchantType.Value);

        if (request.MerchantStatus.HasValue)
            query = query.Where(m => m.MerchantStatus == request.MerchantStatus.Value);

        if (request.CreateDateStart.HasValue)
            query = query.Where(m => m.CreateDate >= request.CreateDateStart.Value);

        if (request.CreateDateEnd.HasValue)
            query = query.Where(m => m.CreateDate <= request.CreateDateEnd.Value);

        var pricingProfile = await _context.PricingProfile
           .FirstOrDefaultAsync(b => b.PricingProfileNumber == request.PricingProfileNumber && b.ProfileStatus == ProfileStatus.InUse);

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), request.PricingProfileNumber);
        }

        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            if (pricingProfile.ProfileType == ProfileType.SubMerchant)
            {
                await strategy.ExecuteAsync(async () =>
                {
                    int updatedCount = await query.Where(b => b.MerchantType == MerchantType.SubMerchant)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.PricingProfileNumber, _ => request.PricingProfileNumber));
                    _logger.LogError($"Successfully updated {updatedCount} merchants");
                });
            }
            else
            {
                await strategy.ExecuteAsync(async () =>
                {
                    int updatedCount = await query.Where(b => b.MerchantType != MerchantType.SubMerchant)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.PricingProfileNumber, _ => request.PricingProfileNumber));
                    _logger.LogError($"Successfully updated {updatedCount} merchants");
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk update merchant pricing profiles");
            throw;
        }
    }

    public async Task<PaginatedList<MerchantInstallmentTransactionDto>> GetMerchantInstallmentTransactionList(GetAllMerchantInstallmentTransactionQuery request)
    {
        var merchantInstallmentTransactionList = _merchantInstallmentTransactionRepository
            .GetAll()
            .Where(b=>b.MerchantTransactionId == request.MerchantTransactionId)
            .OrderBy(b => b.InstallmentCount).AsQueryable();
               
        return await merchantInstallmentTransactionList
            .PaginatedListWithMappingAsync<MerchantInstallmentTransaction, MerchantInstallmentTransactionDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
