using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDues.Command.DeleteMerchantDue;

public class DeleteMerchantDueCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteMerchantDueCommandHandler : IRequestHandler<DeleteMerchantDueCommand>
{
    private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public DeleteMerchantDueCommandHandler(IGenericRepository<MerchantDue> merchantDueRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _merchantDueRepository = merchantDueRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(DeleteMerchantDueCommand request, CancellationToken cancellationToken)
    {
        var merchantDue = await _merchantDueRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (merchantDue is null)
        {
            throw new NotFoundException(nameof(MerchantDue), request.Id);
        }

        merchantDue.RecordStatus = RecordStatus.Passive;
        await _merchantDueRepository.UpdateAsync(merchantDue);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteMerchantDue",
                SourceApplication = "PF",
                Resource = "MerchantDue",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"MerchantId", merchantDue.MerchantId.ToString()},
                    {"MerchantDueId", request.Id.ToString()}
                }
            });

        return Unit.Value;
    }
}