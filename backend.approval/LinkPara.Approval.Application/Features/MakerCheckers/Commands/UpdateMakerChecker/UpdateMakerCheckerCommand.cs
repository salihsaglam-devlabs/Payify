using LinkPara.Approval.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.UpdateMakerChecker;

public class UpdateMakerCheckerCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public Guid MakerRoleId { get; set; }
    public Guid CheckerRoleId { get; set; }
    public Guid SecondCheckerRoleId { get; set; }
}


public class UpdateMakerCheckerCommandHandler : IRequestHandler<UpdateMakerCheckerCommand>
{
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IGenericRepository<MakerChecker> _makerCheckerRepository;
    private readonly IAuditLogService _auditLogService;

    public UpdateMakerCheckerCommandHandler(IGenericRepository<Case> caseRepository,
        IGenericRepository<MakerChecker> makerCheckerRepository,
        IAuditLogService auditLogService)
    {
        _caseRepository = caseRepository;
        _makerCheckerRepository = makerCheckerRepository;
        _auditLogService = auditLogService; 
    }
    public async Task<Unit> Handle(UpdateMakerCheckerCommand request, CancellationToken cancellationToken)
    {
        var approvalCase = await _caseRepository.GetAll()
                                         .FirstOrDefaultAsync(s => s.Id == request.CaseId
                                                                , cancellationToken: cancellationToken);

        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), request.CaseId);
        }

        var makerChecker = await _makerCheckerRepository.GetAll()
                                         .FirstOrDefaultAsync(s => s.Id == request.Id
                                                                , cancellationToken: cancellationToken);

        if (makerChecker is null)
        {
            throw new NotFoundException(nameof(MakerChecker), request.Id);
        }

        makerChecker.MakerRoleId = request.MakerRoleId;
        makerChecker.CheckerRoleId = request.CheckerRoleId;
        makerChecker.SecondCheckerRoleId = request.SecondCheckerRoleId;

        await _makerCheckerRepository.UpdateAsync(makerChecker);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "UpdateMakerChecker",
            SourceApplication = "Approval",
            Resource = "MakerChecker",
            Details = new Dictionary<string, string>
            {
                {"Id", makerChecker.Id.ToString() },
                {"CaseId", makerChecker.CaseId.ToString() },
            }
        });

        return Unit.Value;
    }
}

