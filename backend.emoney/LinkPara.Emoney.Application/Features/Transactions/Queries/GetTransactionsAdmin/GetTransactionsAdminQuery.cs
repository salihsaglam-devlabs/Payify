using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsAdmin;

public class GetTransactionsAdminQuery : SearchQueryParams, IRequest<PaginatedList<TransactionAdminDto>>
{
    [FromQuery(Name = "TransactionTypes")]
    public string TransactionTypesString { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string CurrencyCode { get; set; }
    public string Tag { get; set; }
    public string ExternalReferenceId { get; set; }
    public string Description { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? WalletId { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ReceiverBankCode { get; set; }
    public string ReceiverName { get; set; }
    public int? SenderBankCode { get; set; }
    public string SenderName { get; set; }
    public TransactionDirection? TransactionDirection { get; set; }
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
    public string WalletNumber { get; set; }
}

public class GetAdminTransactionsQueryHandler : IRequestHandler<GetTransactionsAdminQuery, PaginatedList<TransactionAdminDto>>
{
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IGenericRepository<Transaction> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IGenericRepository<WithdrawRequest> _withdrawRequestRepository;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IContextProvider _contextProvider;


    public GetAdminTransactionsQueryHandler(IGenericRepository<Transaction> repository,
        IMapper mapper,
        IGenericRepository<AccountUser> accountUserRepository,
        IStringLocalizerFactory stringLocalizerFactory,
        IGenericRepository<WithdrawRequest> withdrawRequestRepository,
        IUserActivityLogService userActivityLogService,
        IContextProvider contextProvider,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _accountUserRepository = accountUserRepository;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _withdrawRequestRepository = withdrawRequestRepository;
        _userActivityLogService = userActivityLogService;
        _contextProvider = contextProvider;
        _cardTopupRequestRepository = cardTopupRequestRepository;
    }

    public async Task<PaginatedList<TransactionAdminDto>> Handle(GetTransactionsAdminQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Include(x => x.Wallet)
            .Include(x => x.Currency)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId.Value);

            if (accountUser != null)
            {
                query = query.Where(x => x.Wallet.AccountId == accountUser.AccountId);
            }
        }

        query = ApplyEnumFilterAsync(request, query);

        query = ApplyStringFilterAsync(request, query);

        query = ApplyGuidFilterAsync(request, query);

        query = ApplyDateFilterAsync(request, query);

        query = ApplyIntFilterAsync(request, query);

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

            if (p.PaymentMethod == PaymentMethod.CreditCard)
            {
                var paymentCardType = _cardTopupRequestRepository.GetAll()
                                .Where(c => c.Id == p.CardTopupRequestId)
                                .Select(c => c.CardType)
                                .FirstOrDefault();
                
                p.CardType = paymentCardType;
            }

            var relatedTransactions = _repository.GetAll()
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


            p.Amount = p.TransactionType == TransactionType.Deposit &&
                       p.PaymentMethod == PaymentMethod.CreditCard
                ? p.Amount - (p.TaxAmount + p.CommissionAmount) : p.Amount;

            p.TotalAmount = p.TransactionType == TransactionType.Deposit &&
                            p.PaymentMethod != PaymentMethod.CreditCard
                ? p.Amount - (p.TaxAmount + p.CommissionAmount)
                : p.Amount + (p.TaxAmount + p.CommissionAmount);


            p.TagTitle = !string.IsNullOrEmpty(p.TagTitle) ? _tagLocalizer.GetString(p.TagTitle) : p.TagTitle;

            p.TotalAmountText = AmountTextConverter.DecimalToWords(p.TotalAmount);
        });
        await _userActivityLogService.UserActivityLogAsync(
                            new UserActivityLog
                            {
                                LogDate = DateTime.Now,
                                Operation = "AccountTransactions",
                                Resource = "Transactions",
                                SourceApplication = "Emoney",
                                ViewerId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                                : Guid.Empty,
                                ViewedId = Guid.Empty
                            }

                        );
        return result;
    }

    private static IQueryable<Transaction> ApplyEnumFilterAsync(GetTransactionsAdminQuery request, IQueryable<Transaction> query)
    {
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
        if (request.RecordStatus.HasValue)
        {
            query = query.Where(x => x.RecordStatus == request.RecordStatus);
        }

        return query;
    }
    private static IQueryable<Transaction> ApplyStringFilterAsync(GetTransactionsAdminQuery request, IQueryable<Transaction> query)
    {
        if (request.CurrencyCode is not null)
        {
            query = query.Where(x => x.CurrencyCode == request.CurrencyCode);
        }

        if (request.Tag is not null)
        {
            query = query.Where(x => x.Tag == request.Tag);
        }

        if (request.ExternalReferenceId is not null)
        {
            query = query.Where(x => x.ExternalReferenceId == request.ExternalReferenceId);
        }

        if (request.Description is not null)
        {
            query = query.Where(x => x.Description.Contains(request.Description));
        }

        if (request.ReceiverName is not null)
        {
            query = query.Where(x => x.ReceiverName.ToLower().Contains(request.ReceiverName.ToLower()));
        }

        if (request.SenderName is not null)
        {
            query = query.Where(x => x.SenderName.ToLower().Contains(request.SenderName.ToLower()));
        }

        if (request.WalletNumber is not null)
        {
            query = query.Where(x => x.Wallet.WalletNumber == request.WalletNumber);
        }

        return query;
    }
    private static IQueryable<Transaction> ApplyGuidFilterAsync(GetTransactionsAdminQuery request, IQueryable<Transaction> query)
    {
        if (request.TransactionId.HasValue)
        {
            query = query.Where(x => x.Id == request.TransactionId.Value || x.RelatedTransactionId == request.TransactionId.Value);
        }
        if (request.WalletId.HasValue)
        {
            query = query.Where(x => x.WalletId == request.WalletId);
        }

        if (request.RelatedTransactionId.HasValue)
        {
            query = query.Where(x => x.RelatedTransactionId == request.RelatedTransactionId);
        }

        return query;
    }
    private static IQueryable<Transaction> ApplyDateFilterAsync(GetTransactionsAdminQuery request, IQueryable<Transaction> query)
    {
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
    private static IQueryable<Transaction> ApplyIntFilterAsync(GetTransactionsAdminQuery request, IQueryable<Transaction> query)
    {
        if (request.ReceiverBankCode is not null)
        {
            query = query.Where(x => x.ReceiverBankCode == request.ReceiverBankCode);
        }

        if (request.SenderBankCode is not null)
        {
            query = query.Where(x => x.SenderBankCode == request.SenderBankCode);
        }

        return query;
    }
}
