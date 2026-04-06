using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDues.Command.UpdateMerchantDue;

public class UpdateMerchantDueCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid DueProfileId { get; set; }
}

public class UpdateMerchantDueCommandHandler : IRequestHandler<UpdateMerchantDueCommand>
{
    private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
    private readonly IGenericRepository<DueProfile> _dueProfileRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public UpdateMerchantDueCommandHandler(IGenericRepository<MerchantDue> merchantDueRepository,
        IGenericRepository<DueProfile> dueProfileRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _merchantDueRepository = merchantDueRepository;
        _dueProfileRepository = dueProfileRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(UpdateMerchantDueCommand request, CancellationToken cancellationToken)
    {
        var merchantDue = await _merchantDueRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (merchantDue is null)
        {
            throw new NotFoundException(nameof(MerchantDue), request.Id);
        }
        
        var dueProfile = await _dueProfileRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.DueProfileId, cancellationToken: cancellationToken);

        if (dueProfile is null)
        {
            throw new NotFoundException(nameof(DueProfile), request.DueProfileId);
        }

        var isDueTypeAlreadyExist = await _merchantDueRepository.GetAll()
            .Include(s => s.DueProfile)
            .AnyAsync(s =>
            s.Id != merchantDue.Id &&
            s.MerchantId == merchantDue.MerchantId && 
            s.DueProfile.DueType == dueProfile.DueType && 
            s.RecordStatus == RecordStatus.Active
            , cancellationToken: cancellationToken);

        if (isDueTypeAlreadyExist)
        {
            throw new DueAlreadyActiveForMerchantException();
        }

        var oldProfileId = merchantDue.DueProfileId;
        merchantDue.DueProfileId = request.DueProfileId;
        await _merchantDueRepository.UpdateAsync(merchantDue);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateMerchantDue",
                SourceApplication = "PF",
                Resource = "MerchantDue",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"MerchantId", merchantDue.MerchantId.ToString()},
                    {"NewDueProfileId", request.DueProfileId.ToString()},
                    {"OldDueProfileId", oldProfileId.ToString()}
                }
            });

        return Unit.Value;
    }
}