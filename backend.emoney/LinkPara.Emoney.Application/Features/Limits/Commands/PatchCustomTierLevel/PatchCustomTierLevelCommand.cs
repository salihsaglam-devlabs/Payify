using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PatchCustomTierLevel;

public class PatchCustomTierLevelCommand : IRequest
{
    public Guid Id { get; set; }
    public JsonPatchDocument<CustomTierLevelDto> PatchCustomTierLevel { get; set; }
}

public class PatchCustomizedTierLevelCommandHandler : IRequestHandler<PatchCustomTierLevelCommand>
{
    private readonly IGenericRepository<TierLevel> _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public PatchCustomizedTierLevelCommandHandler(IGenericRepository<TierLevel> repository,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(PatchCustomTierLevelCommand command, CancellationToken cancellationToken)
    {
        var dbTierLevel = await _repository.GetByIdAsync(command.Id);

        if (dbTierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel));
        }

        var requestTierLevelDto = _mapper.Map<CustomTierLevelDto>(dbTierLevel);

        command.PatchCustomTierLevel.ApplyTo(requestTierLevelDto);

        if (dbTierLevel.TierLevelType == TierLevelType.Custom)
        {
            _mapper.Map(requestTierLevelDto, dbTierLevel);
        }
        else
        {
            PopulateOnlyPermittedFields(requestTierLevelDto, dbTierLevel);
        }
        
        await _repository.UpdateAsync(dbTierLevel);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateCustomizedTierLevel",
                SourceApplication = "Emoney",
                Resource = "TierLevel",
                Details = new Dictionary<string, string>
                {
                       {"Id", dbTierLevel.Id.ToString() },
                       {"Name", dbTierLevel.Name }
                }
            });

        return Unit.Value;
    }

    private static void PopulateOnlyPermittedFields(CustomTierLevelDto requestTierLevelDto, TierLevel dbTierLevel)
    {
        dbTierLevel.MaxBalanceLimitEnabled = requestTierLevelDto.MaxBalanceLimitEnabled;
        dbTierLevel.MaxBalance = requestTierLevelDto.MaxBalance;
        dbTierLevel.MaxInternalTransferLimitEnabled = requestTierLevelDto.MaxInternalTransferLimitEnabled;
        dbTierLevel.DailyMaxInternalTransferAmount = requestTierLevelDto.DailyMaxInternalTransferAmount;
        dbTierLevel.DailyMaxInternalTransferCount = requestTierLevelDto.DailyMaxInternalTransferCount;
        dbTierLevel.MonthlyMaxInternalTransferAmount = requestTierLevelDto.MonthlyMaxInternalTransferAmount;
        dbTierLevel.MonthlyMaxInternalTransferCount = requestTierLevelDto.MonthlyMaxInternalTransferCount;
        dbTierLevel.MaxDepositLimitEnabled = requestTierLevelDto.MaxDepositLimitEnabled;
        dbTierLevel.DailyMaxDepositAmount = requestTierLevelDto.DailyMaxDepositAmount;
        dbTierLevel.DailyMaxDepositCount = requestTierLevelDto.DailyMaxDepositCount;
        dbTierLevel.MonthlyMaxDepositAmount = requestTierLevelDto.MonthlyMaxDepositAmount;
        dbTierLevel.MonthlyMaxDepositCount = requestTierLevelDto.MonthlyMaxDepositCount;
        dbTierLevel.MaxWithdrawalLimitEnabled = requestTierLevelDto.MaxWithdrawalLimitEnabled;
        dbTierLevel.DailyMaxWithdrawalAmount = requestTierLevelDto.DailyMaxWithdrawalAmount;
        dbTierLevel.DailyMaxWithdrawalCount = requestTierLevelDto.DailyMaxWithdrawalCount;
        dbTierLevel.MonthlyMaxWithdrawalAmount = requestTierLevelDto.MonthlyMaxWithdrawalAmount;
        dbTierLevel.MonthlyMaxWithdrawalCount = requestTierLevelDto.MonthlyMaxWithdrawalCount;
        dbTierLevel.MaxInternationalTransferLimitEnabled = requestTierLevelDto.MaxInternationalTransferLimitEnabled;
        dbTierLevel.DailyMaxInternationalTransferAmount = requestTierLevelDto.DailyMaxInternationalTransferAmount;
        dbTierLevel.DailyMaxInternationalTransferCount = requestTierLevelDto.DailyMaxInternationalTransferCount;
        dbTierLevel.MonthlyMaxInternationalTransferAmount = requestTierLevelDto.MonthlyMaxInternationalTransferAmount;
        dbTierLevel.MonthlyMaxInternationalTransferCount = requestTierLevelDto.MonthlyMaxInternationalTransferCount;
        dbTierLevel.MaxOwnIbanWithdrawalLimitEnabled = requestTierLevelDto.MaxOwnIbanWithdrawalLimitEnabled;
        dbTierLevel.DailyMaxOwnIbanWithdrawalCount = requestTierLevelDto.DailyMaxOwnIbanWithdrawalCount;
        dbTierLevel.MonthlyMaxOwnIbanWithdrawalCount = requestTierLevelDto.MonthlyMaxOwnIbanWithdrawalCount;
        dbTierLevel.MaxOtherIbanWithdrawalLimitEnabled = requestTierLevelDto.MaxOtherIbanWithdrawalLimitEnabled;
        dbTierLevel.DailyMaxOtherIbanWithdrawalCount = requestTierLevelDto.DailyMaxOtherIbanWithdrawalCount;
        dbTierLevel.DailyMaxDistinctOtherIbanWithdrawalCount = requestTierLevelDto.DailyMaxDistinctOtherIbanWithdrawalCount;
        dbTierLevel.DailyMaxOtherIbanWithdrawalAmount = requestTierLevelDto.DailyMaxOtherIbanWithdrawalAmount;
        dbTierLevel.MonthlyMaxOtherIbanWithdrawalCount = requestTierLevelDto.MonthlyMaxOtherIbanWithdrawalCount;
        dbTierLevel.MonthlyMaxDistinctOtherIbanWithdrawalCount = requestTierLevelDto.MonthlyMaxDistinctOtherIbanWithdrawalCount;
        dbTierLevel.MonthlyMaxOtherIbanWithdrawalAmount = requestTierLevelDto.MonthlyMaxOtherIbanWithdrawalAmount;
        dbTierLevel.MaxOnUsPaymentLimitEnabled = requestTierLevelDto.MaxOnUsPaymentLimitEnabled;
        dbTierLevel.DailyMaxOnUsPaymentAmount = requestTierLevelDto.DailyMaxOnUsPaymentAmount;
        dbTierLevel.DailyMaxOnUsPaymentCount = requestTierLevelDto.DailyMaxOnUsPaymentCount;
        dbTierLevel.MonthlyMaxOnUsPaymentAmount = requestTierLevelDto.MonthlyMaxOnUsPaymentAmount;
        dbTierLevel.MonthlyMaxOnUsPaymentCount = requestTierLevelDto.MonthlyMaxOnUsPaymentCount;
    }
}