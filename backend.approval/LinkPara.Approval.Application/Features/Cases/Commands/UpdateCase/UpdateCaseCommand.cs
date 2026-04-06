using LinkPara.Approval.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Cases.Commands.UpdateCase;

public class UpdateCaseCommand : IRequest
{
    public Guid Id { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class UpdateCaseCommandHandler : IRequestHandler<UpdateCaseCommand>
{
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IAuditLogService _auditLogService;

    public UpdateCaseCommandHandler(IGenericRepository<Case> caseRepository,
                                    IAuditLogService auditLogService)
    {
        _caseRepository = caseRepository;
        _auditLogService = auditLogService; 
    }
    public async Task<Unit> Handle(UpdateCaseCommand request, CancellationToken cancellationToken)
    {
        var approvalCase = await _caseRepository.GetAll()
                                                .Include(s => s.MakerCheckers)
                                                .FirstOrDefaultAsync(s => s.Id == request.Id
                                                , cancellationToken);
        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), request.Id);
        }

        approvalCase.RecordStatus = request.RecordStatus;

        if (approvalCase.MakerCheckers is not null)
        {
            foreach (var makerCheker in approvalCase.MakerCheckers)
            {
                makerCheker.RecordStatus = request.RecordStatus;
            }
        }

        await _caseRepository.UpdateAsync(approvalCase);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "UpdateCase",
            SourceApplication = "Approval",
            Resource = "Case",
            Details = new Dictionary<string, string>
            {
                {"Id", approvalCase.Id.ToString() },
                {"ModuleName", approvalCase.ModuleName },
                {"ActionName", approvalCase.ActionName }
            }
        });

        return Unit.Value;
    }
}