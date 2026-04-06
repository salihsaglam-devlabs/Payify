using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Features.Currencies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionSummary;

public class GetTransactionSummaryQuery : IRequest<TransactionSummaryDto>
{
    public string WalletNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string CurrencyCode { get; set; }
    public Guid UserId { get; set; }
}

public class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
{
    private readonly IGenericRepository<Transaction> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IContextProvider _contextProvider;

    public GetTransactionSummaryQueryHandler(IGenericRepository<Transaction> repository,
        IMapper mapper,
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<Currency> currencyRepository,
        IGenericRepository<AccountUser> accountUserRepository,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _mapper = mapper;
        _walletRepository = walletRepository;
        _currencyRepository = currencyRepository;
        _accountUserRepository = accountUserRepository;
        _contextProvider = contextProvider;
    }

    public async Task<TransactionSummaryDto> Handle(GetTransactionSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var accountUser = await _accountUserRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), request.UserId);
        }

        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        var walletList = await _walletRepository.GetAll()
                            .Where(x => x.AccountId == accountUser.AccountId)
                            .ToListAsync();

        if (!walletList.Any())
        {
            throw new NotFoundException(nameof(Wallet));
        }

        var result = new TransactionSummaryDto();

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            walletList = walletList.Where(x => x.WalletNumber == request.WalletNumber).ToList();

            if (!walletList.Any())
            {
                throw new NotFoundException(nameof(Wallet));
            }
            result.WalletNumber = request.WalletNumber;
        }

        foreach (var walletId in walletList.Select(wallet => wallet.Id))
        {
            result.MoneyIn += _repository.GetAll()
                   .Where(x => (x.TransactionStatus == TransactionStatus.Completed
                    || x.TransactionStatus == TransactionStatus.Pending)
                    && x.TransactionType == TransactionType.Deposit
                    && x.WalletId == walletId
                    && x.CurrencyCode == request.CurrencyCode
                    && x.TransactionDate >= request.StartDate
                    && x.TransactionDate <= request.EndDate)
                   .Include(x => x.Currency)
                   .Sum(x => x.Amount);

            result.MoneyOut += _repository.GetAll().
                      Where(x => (x.TransactionStatus == TransactionStatus.Completed
                      || x.TransactionStatus == TransactionStatus.Pending)
                      && x.TransactionType == TransactionType.Withdraw
                      && x.WalletId == walletId
                      && x.CurrencyCode == request.CurrencyCode
                      && x.TransactionDate >= request.StartDate
                      && x.TransactionDate <= request.EndDate)
                      .Include(x => x.Currency)
                      .Sum(x => x.Amount);
        }

        result.Net = result.MoneyIn - result.MoneyOut;
        var currency = await _currencyRepository.GetAll().
                      Where(x => x.Code == request.CurrencyCode).
                      FirstOrDefaultAsync(cancellationToken);

        if (currency != null)
        {
            result.Currency = _mapper.Map<CurrencyDto>(currency);
        }

        return result;
    }
}