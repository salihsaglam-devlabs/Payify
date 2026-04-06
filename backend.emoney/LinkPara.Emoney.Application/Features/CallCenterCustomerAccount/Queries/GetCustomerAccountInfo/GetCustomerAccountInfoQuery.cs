using AutoMapper;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Application.Features.Transactions;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;

namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Queries.GetCustomerAccountInfo;
public class GetCustomerAccountInfoQuery : SearchQueryParams, IRequest<CustomerAccountInfoResponse>
{
    [FromQuery(Name = "TransactionTypes")]
    public string TransactionTypesString { get; set; }
    public Guid AccountId { get; set; }
    public Guid ConfirmationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TransactionDirection? TransactionDirection { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
    internal List<TransactionType> TransactionTypes
    {
        get
        {
            var list = new List<TransactionType>();

            if (TransactionTypesString is not null)
            {
                TransactionTypesString.Split(",").ToList().ForEach(x =>
                {
                    list.Add(Enum.Parse<TransactionType>(x));
                });
            }

            return list;
        }
    }
}

public class GetCustomerAccountInfoQueryHandler : IRequestHandler<GetCustomerAccountInfoQuery, CustomerAccountInfoResponse>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<AccountKycChange> _accountKycChangeRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<GetCustomerAccountInfoQueryHandler> _logger;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IStringLocalizer _errorLocalizer;
    private readonly IGenericRepository<WithdrawRequest> _withdrawRequestRepository;
    private readonly IGenericRepository<CallCenterNotificationLog> _callcenterRepository;
    private readonly ILimitService _limitService;

    public GetCustomerAccountInfoQueryHandler(
        IGenericRepository<Account> accountRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<GetCustomerAccountInfoQueryHandler> logger,
        IGenericRepository<Transaction> transactionRepository,
        IStringLocalizerFactory stringLocalizerFactory,
        IGenericRepository<WithdrawRequest> withdrawRequestRepository,
        IGenericRepository<CallCenterNotificationLog> callcenterRepository,
        IGenericRepository<AccountKycChange> accountKycChangeRepository,
        ILimitService limitService)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
        _transactionRepository = transactionRepository;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _errorLocalizer = stringLocalizerFactory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _withdrawRequestRepository = withdrawRequestRepository;
        _callcenterRepository = callcenterRepository;
        _accountKycChangeRepository = accountKycChangeRepository;
        _limitService = limitService;
    }

    public async Task<CustomerAccountInfoResponse> Handle(GetCustomerAccountInfoQuery request,
        CancellationToken cancellationToken)
    {
        CustomerAccountInfoResponse response = new CustomerAccountInfoResponse();

        var isCustomerConfirmedMessage = await IsCustomerConfirmed(request.ConfirmationId);

        if (String.IsNullOrEmpty(isCustomerConfirmedMessage))
        {
            try
            {
                response.AccountInformation = await GetCustomerAccountInformation(request);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetCustomerAccountInformation: {exception}", exception);
            }

            try
            {
                response.Wallets = await GetCustomerWallets(request.AccountId);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetCustomerWallets: {exception}", exception);
            }

            try
            {
                response.KycChanges = await GetCustomerKycChanges(request.AccountId, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetCustomerKycChanges: {exception}", exception);
            }

            try
            {
                response.AccountLimits = await GetCustomerLimitInfo(request.AccountId);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetCustomerLimitInfo: {exception}", exception);
            }

            try
            {
                response.AccountTransactions = await GetAccountTransactions(request);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetAccountTransactions: {exception}", exception);
            }
        }
        else
        {
            _logger.LogError($"GetCustomerAccountInfoQuery: Customer not confirmed.");
            response.ErrorMessage = isCustomerConfirmedMessage;
        }


        return response;
    }

    private async Task<string> IsCustomerConfirmed(Guid confirmationId)
    {
        var notificationLog = await _callcenterRepository.GetByIdAsync(confirmationId);
        var now = DateTime.Now;
        var isApproved = notificationLog != null && notificationLog.Status == CallCenterNotificationStatus.Approve;
        var isValidTime = notificationLog != null && now < notificationLog.ExpireDate;
        var responseMessage = !isApproved ? _errorLocalizer.GetString("CustomerNotApproved").Value : (!isValidTime ? _errorLocalizer.GetString("TimeOut").Value : String.Empty);
        return responseMessage;
    }
    private async Task<AccountDto> GetCustomerAccountInformation(GetCustomerAccountInfoQuery request)
    {
        var account = await _accountRepository
            .GetAll()
            .Include(s => s.AccountUsers)
            .FirstOrDefaultAsync(s => s.Id == request.AccountId);

        NullControlHelper.CheckAndThrowIfNull(account, request.AccountId, _logger);

        if (account.RecordStatus == RecordStatus.Passive
            && !string.IsNullOrEmpty(account.IdentityNumber))
        {
            Regex identityNumber = new Regex(@"(?<!\d)\d{11}(?!\d)");
            var value = identityNumber.Match(account.IdentityNumber);
            account.IdentityNumber = value.Success ? value.Value : account.IdentityNumber;
        }

        try
        {
            var user = await _userService.GetUserAsync(account.AccountUsers[0].UserId);

            if (!user.BirthDate.Equals(account.BirthDate))
            {
                account.BirthDate = user.BirthDate;

                await _accountRepository.UpdateAsync(account);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("GetAccountByIdQueryHandlerError: {exception}", e);
        }

        return _mapper.Map<AccountDto>(account);

    }

    private async Task<List<WalletDto>> GetCustomerWallets(Guid accountId)
    {
        var account = await _accountRepository
        .GetAll()
        .Include(s => s.Wallets)
        .ThenInclude(s => s.Currency)
        .SingleOrDefaultAsync(s => s.Id == accountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        return _mapper.Map<List<WalletDto>>(account.Wallets);

    }

    private async Task<List<AccountKycChangeDto>> GetCustomerKycChanges(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await _accountRepository
        .GetAll()
        .FirstOrDefaultAsync(s => s.Id == accountId, cancellationToken: cancellationToken);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        var kycChanges = await _accountKycChangeRepository
            .GetAll()
            .Where(k => k.AccountId == accountId)
            .OrderByDescending(k => k.CreateDate)
            .ToListAsync(cancellationToken: cancellationToken);

        return _mapper.Map<List<AccountKycChangeDto>>(kycChanges);
    }

    private async Task<AccountLimitDto> GetCustomerLimitInfo(Guid accountId)
    {
        var request = new GetAccountLimitsQuery() { AccountId = accountId, CurrencyCode = "TRY" };
        var result = await _limitService.GetAccountLimitsQuery(request);

        return result;
    }

    private async Task<PaginatedList<TransactionAdminDto>> GetAccountTransactions(GetCustomerAccountInfoQuery request)
    {
        var query = _transactionRepository.GetAll()
            .Include(x => x.Wallet)
            .Include(x => x.Currency)
            .AsQueryable();

        query = ApplyFiltersAsync(request, query);

        request.SortBy = String.IsNullOrEmpty(request.SortBy) ? "transactionDate" : request.SortBy;

        var result = await query
            .PaginatedListWithMappingAsync<Transaction, TransactionAdminDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        result.Items.ForEach(p =>
        {
            if (p.WithdrawRequestId != null
                && p.TransactionType == TransactionType.Withdraw
                && p.PaymentMethod == PaymentMethod.BankTransfer)
            {
                p.ReceiverIban = _withdrawRequestRepository
                                    .GetAll()
                                    .Where(w => w.Id == p.WithdrawRequestId)
                                    .Select(w => w.ReceiverIbanNumber)
                                    .FirstOrDefault();
            }

            var relatedTransactions = _transactionRepository.GetAll()
                .Where(s => s.RelatedTransactionId == p.Id)
                .ToList();

            if (relatedTransactions.Count > 0)
            {
                var bsmv = relatedTransactions.Select(x => new
                {
                    x.Tag,
                    x.Amount,
                    x.TransactionType
                }).FirstOrDefault(s => s.TransactionType == TransactionType.Tax);

                var pricing = relatedTransactions.Select(x => new
                {
                    x.Tag,
                    x.Amount,
                    x.TransactionType
                }).FirstOrDefault(s => s.TransactionType == TransactionType.Commission);

                p.TaxAmount = bsmv?.Amount ?? 0;
                p.CommissionAmount = pricing?.Amount ?? 0;
            }

            p.TotalAmount = p.TransactionType == TransactionType.Deposit
                ? p.Amount - (p.TaxAmount + p.CommissionAmount)
                : p.Amount + (p.TaxAmount + p.CommissionAmount);

            p.TagTitle = !string.IsNullOrEmpty(p.TagTitle) ? _tagLocalizer.GetString(p.TagTitle) : p.TagTitle;

            p.TotalAmountText = AmountTextConverter.DecimalToWords(p.TotalAmount);
        });
        return result;

    }

    private static IQueryable<Transaction> ApplyFiltersAsync(GetCustomerAccountInfoQuery request, IQueryable<Transaction> query)
    {
        if (request.AccountId != Guid.Empty)
        {
            query = query.Where(x => x.Wallet.AccountId == request.AccountId);
        }
        if (request.TransactionTypes.Count > 0)
        {
            query = query.Where(x => request.TransactionTypes.Contains(x.TransactionType));
        }
        if (request.TransactionDirection.HasValue)
        {
            query = query.Where(x => x.TransactionDirection == request.TransactionDirection);
        }
        if (request.PaymentMethod.HasValue)
        {
            query = query.Where(x => x.PaymentMethod == request.PaymentMethod);
        }
        if (request.TransactionStatus.HasValue)
        {
            query = query.Where(x => x.TransactionStatus == request.TransactionStatus);
        }
        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate >= request.StartDate);
        }
        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate <= request.EndDate);
        }

        return query;
    }
}
