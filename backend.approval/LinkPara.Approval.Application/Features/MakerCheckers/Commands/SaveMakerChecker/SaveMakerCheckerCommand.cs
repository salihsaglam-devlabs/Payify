using LinkPara.Approval.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.SaveMakerChecker;

public class SaveMakerCheckerCommand : IRequest
{
    public Guid CaseId { get; set; }
    public Guid MakerRoleId { get; set; }
    public Guid CheckerRoleId { get; set; }
    public Guid SecondCheckerRoleId { get; set; }
}

public class SaveMakerCheckerCommandHandler : IRequestHandler<SaveMakerCheckerCommand>
{

    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IGenericRepository<MakerChecker> _makerCheckerRepository;
    private readonly IAuditLogService _auditLogService;

    public SaveMakerCheckerCommandHandler(IGenericRepository<Case> caseRepository,
        IGenericRepository<MakerChecker> makerCheckerRepository,
        IAuditLogService auditLogService)
    {
        _caseRepository = caseRepository;
        _makerCheckerRepository = makerCheckerRepository;
        _auditLogService = auditLogService;
    }
    public async Task<Unit> Handle(SaveMakerCheckerCommand request, CancellationToken cancellationToken)
    {
        if (await IsExistAsync(request))
            throw new DuplicateRecordException();

        var approvalCase = await _caseRepository.GetAll()
                                         .FirstOrDefaultAsync(s => s.Id == request.CaseId 
                                                                && s.RecordStatus == RecordStatus.Active
                                                                , cancellationToken: cancellationToken);

        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), request.CaseId);
        }

        var makerChecker = new MakerChecker
        {
            CaseId = approvalCase.Id,
            MakerRoleId = request.MakerRoleId,
            CheckerRoleId = request.CheckerRoleId,
            SecondCheckerRoleId = request.SecondCheckerRoleId
        };

        await _makerCheckerRepository.AddAsync(makerChecker);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SaveMakerChecker",
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
    private async Task<bool> IsExistAsync(SaveMakerCheckerCommand request)
    {
        return await _makerCheckerRepository.GetAll().AnyAsync(x =>
            x.CaseId == request.CaseId &&
            x.MakerRoleId == request.MakerRoleId &&
            x.CheckerRoleId == request.CheckerRoleId &&
            x.SecondCheckerRoleId == request.SecondCheckerRoleId &&
            x.RecordStatus == RecordStatus.Active
        );
    }

}
