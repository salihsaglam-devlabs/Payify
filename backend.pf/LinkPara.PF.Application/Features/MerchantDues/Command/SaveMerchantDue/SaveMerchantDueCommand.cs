using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDues.Command.SaveMerchantDue;

public class SaveMerchantDueCommand : IRequest
{
    public Guid MerchantId { get; set; }
    public Guid DueProfileId { get; set; }
}

public class SaveHostedPaymentCommandHandler : IRequestHandler<SaveMerchantDueCommand>
{
    private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
    private readonly IGenericRepository<DueProfile> _dueProfileRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    
    public SaveHostedPaymentCommandHandler(
        IGenericRepository<MerchantDue> merchantDueRepository,
        IGenericRepository<DueProfile> dueProfileRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider
        )
    {
        _merchantDueRepository = merchantDueRepository;
        _dueProfileRepository = dueProfileRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }
    
    public async Task<Unit> Handle(SaveMerchantDueCommand request, CancellationToken cancellationToken)
    {
        var dueProfile = await _dueProfileRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.DueProfileId, cancellationToken: cancellationToken);

        if (dueProfile is null)
        {
            throw new NotFoundException(nameof(DueProfile), request.DueProfileId);
        }

        var isDueTypeAlreadyExist = await _merchantDueRepository.GetAll()
            .Include(s => s.DueProfile)
            .AnyAsync(s =>
            s.MerchantId == request.MerchantId && s.DueProfile.DueType == dueProfile.DueType &&
            s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (isDueTypeAlreadyExist)
        {
            throw new DueAlreadyActiveForMerchantException();
        }

        var merchantDue = new MerchantDue
        {
            MerchantId = request.MerchantId,
            DueProfileId = dueProfile.Id,
            TotalExecutionCount = 0,
            LastExecutionDate = DateTime.MinValue
        };

        await _merchantDueRepository.AddAsync(merchantDue);
        
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveMerchantDue",
                SourceApplication = "PF",
                Resource = "MerchantDue",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"MerchantId", request.MerchantId.ToString()},
                    {"DueProfileId", request.DueProfileId.ToString()}
                }
            });
        
        return Unit.Value;
    }
}