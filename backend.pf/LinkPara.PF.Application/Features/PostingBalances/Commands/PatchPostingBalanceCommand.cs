using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PostingBalances.Commands;

public class PatchPostingBalanceCommand : IRequest
{
    public Guid Id { get; set; }
    public JsonPatchDocument<PatchPostingBalanceRequest> PostingBalance { get; set; }
}

public class PatchPostingBalanceCommandHandler : IRequestHandler<PatchPostingBalanceCommand>
{
    private readonly IGenericRepository<PostingBalance> _genericRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PostingBalance> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IMoneyTransferService _moneyTransferService;
    public PatchPostingBalanceCommandHandler(
        IGenericRepository<PostingBalance> genericRepository,
        IMapper mapper,
        ILogger<PostingBalance> logger,
        IAuditLogService auditLogService,
        IMoneyTransferService moneyTransferService)
    {
        _genericRepository = genericRepository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
        _moneyTransferService = moneyTransferService;
    }
    public async Task<Unit> Handle(PatchPostingBalanceCommand request, CancellationToken cancellationToken)
    {
        var postingBalance = await _genericRepository.GetByIdAsync(request.Id);

        if (postingBalance is null)
        {
            throw new NotFoundException(nameof(PostingBalance), request.Id);
        }

        if (postingBalance.MoneyTransferStatus != PostingMoneyTransferStatus.PaymentDelivered)
        {
            throw new PostingUpdateBankNotPaymentWaitingException();
        }

        try
        {
            var postingBalanceMap = _mapper.Map<PatchPostingBalanceRequest>(postingBalance);
            request.PostingBalance.ApplyTo(postingBalanceMap);
            _mapper.Map(postingBalanceMap, postingBalance);

            await _moneyTransferService.UpdateTransferBankAsync(new UpdateTransferBankRequest
            {
                MoneyTransferTransactionId = postingBalance.MoneyTransferReferenceId,
                TransferBankCode = postingBalanceMap.MoneyTransferBankCode
            });

            await _genericRepository.UpdateAsync(postingBalance);
            
            var relatedPostingBalances = await _genericRepository
                .GetAll()
                .Where(s => s.MoneyTransferReferenceId == postingBalance.MoneyTransferReferenceId && s.Id != postingBalance.Id)
                .ToListAsync(cancellationToken: cancellationToken);
            
            relatedPostingBalances.ForEach(s =>
            {
                s.MoneyTransferBankCode = postingBalanceMap.MoneyTransferBankCode;
                s.MoneyTransferBankName = postingBalanceMap.MoneyTransferBankName;
            });

            await _genericRepository.UpdateRangeAsync(relatedPostingBalances);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "PatchPostingBalance",
                SourceApplication = "PF",
                Resource = "PostingBalance",
                Details = new Dictionary<string, string>
                {
                       {"Id", postingBalance.Id.ToString() },
                       {"BankName", postingBalance.MoneyTransferBankName}
                }
            });

            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "PatchPostingBalanceError : {Exception}", exception);
            throw;
        }
    }
}
