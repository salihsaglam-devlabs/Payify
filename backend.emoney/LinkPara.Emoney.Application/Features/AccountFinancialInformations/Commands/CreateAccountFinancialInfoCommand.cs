using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountFinancialInformations.Commands;

public class CreateAccountFinancialInfoCommand : IRequest
{
    public string IncomeSource { get; set; }
    public string IncomeInformation { get; set; }
    public string MonthlyTransactionVolume { get; set; }
    public string MonthlyTransactionCount { get; set; }
    public Guid AccountId { get; set; }
}

public class CreateAccountFinancialInformationCommandHandler : IRequestHandler<CreateAccountFinancialInfoCommand>
{
    private readonly IGenericRepository<AccountFinancialInformation> _repository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    public CreateAccountFinancialInformationCommandHandler(
        IGenericRepository<AccountFinancialInformation> repository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }
    public async Task<Unit> Handle(CreateAccountFinancialInfoCommand request, CancellationToken cancellationToken)
    {
        var duplicateCheck = await _repository
            .GetAll()
            .AnyAsync(s => s.RecordStatus == RecordStatus.Active
                        && s.AccountId == request.AccountId);

        if (duplicateCheck)
        {
            throw new DuplicateRecordException();
        }
        var accountFinancialInformation = new AccountFinancialInformation
        {
            AccountId = request.AccountId,
            IncomeInformation = request.IncomeInformation,
            MonthlyTransactionCount = request.MonthlyTransactionCount,
            MonthlyTransactionVolume = request.MonthlyTransactionVolume,
            IncomeSource = request.IncomeSource
        };

        await _repository.AddAsync(accountFinancialInformation);

        var details = new Dictionary<string, string>
        {
            {"IncomeInformation", request.IncomeInformation},
            {"AccountId", request.AccountId.ToString() },
            {"MonthlyTransactionCount", request.MonthlyTransactionCount },
            {"MonthlyTransactionVolume", request.MonthlyTransactionVolume },
            {"IncomeSource", request.IncomeSource }
        };

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "CreateAccountFinancialInformation",
                Resource = "AccountFinancialInformation",
                SourceApplication = "Emoney",
                UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            }
        );

        return Unit.Value;
    }
}