using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class BankHealthCheckTransactionService : IBankHealthCheckTransactionService
{
    private readonly ILogger<BankHealthCheckTransactionService> _logger;
    private readonly IGenericRepository<BankHealthCheckTransaction> _repository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    public BankHealthCheckTransactionService(ILogger<BankHealthCheckTransactionService> logger,
        IGenericRepository<BankHealthCheckTransaction> repository,
        IContextProvider contextProvider,
        IApplicationUserService applicationUserService,
        IGenericRepository<BankTransaction> bankTransactionRepository)
    {
        _logger = logger;
        _repository = repository;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _bankTransactionRepository = bankTransactionRepository;
    }

    public async Task SaveAsync(MerchantTransaction request)
    {
        try
        {
            var bankTransaction = await _bankTransactionRepository
               .GetAll()
               .Where(b => b.MerchantTransactionId == request.Id)
               .FirstOrDefaultAsync();

            if (bankTransaction is not null)
            {
                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

                var healthCheckTransaction = new BankHealthCheckTransaction
                {
                    AcquireBankCode = bankTransaction.AcquireBankCode,
                    TransactionStatus = bankTransaction.TransactionStatus,
                    TransactionType = bankTransaction.TransactionType,
                    BankTransactionDate = bankTransaction.CreateDate,
                    CreateDate = DateTime.Now,
                    CreatedBy = parseUserId
                };

                await _repository.AddAsync(healthCheckTransaction);

            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"BankHealthCheckTransactionAddError : {exception}");
            throw;
        }

    }
}
