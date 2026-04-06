using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionDetails;

public class GetTransactionDetailsQuery : IRequest<TransactionDto>
{
    public Guid TransactionId { get; set; }
}

public class GetTransactionDetailsQueryHandler : IRequestHandler<GetTransactionDetailsQuery, TransactionDto>
{
    private readonly ITransactionService _transactionService;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;

    public GetTransactionDetailsQueryHandler(ITransactionService transactionService,
        IUserActivityLogService userActivityLogService,
        IContextProvider contextProvider,
         IGenericRepository<AccountUser> accountUserRepository)
    {
        _accountUserRepository = accountUserRepository;
        _transactionService = transactionService;
        _userActivityLogService = userActivityLogService;
        _contextProvider = contextProvider;
        _contextProvider = contextProvider;
    }

    public async Task<TransactionDto> Handle(GetTransactionDetailsQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.GetTransactionWithDetailsAsync(request.TransactionId, cancellationToken);
        if (_contextProvider.CurrentContext?.Channel == "Backoffice")
        {
            var accountUser = _accountUserRepository.GetAll().Where(x => x.AccountId == transaction.Wallet.AccountId).FirstOrDefault();
            await _userActivityLogService.UserActivityLogAsync(
                            new UserActivityLog
                            {
                                LogDate = DateTime.Now,
                                Operation = "AccountTransactions/Detail",
                                Resource = "Transaction",
                                SourceApplication = "Emoney",
                                ViewerId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                                : Guid.Empty,
                                ViewedId = accountUser.UserId
                            }
                        );
        }

        return transaction;
    }
}