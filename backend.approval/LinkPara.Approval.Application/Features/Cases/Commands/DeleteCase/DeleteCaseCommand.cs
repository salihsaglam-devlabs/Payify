using LinkPara.Approval.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Cases.Commands.DeleteCase;

public class DeleteCaseCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCaseCommandHandler : IRequestHandler<DeleteCaseCommand>
{
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IAuditLogService _auditLogService;

    public DeleteCaseCommandHandler(IGenericRepository<Case> caseRepository,
        IAuditLogService auditLogService)
    {
        _caseRepository = caseRepository;
        _auditLogService = auditLogService; 
    }
    public async Task<Unit> Handle(DeleteCaseCommand request, CancellationToken cancellationToken)
    {
        var approvalCase = await _caseRepository.GetAll().Include(s => s.MakerCheckers)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), request.Id);
        }

        approvalCase.RecordStatus = RecordStatus.Passive;

        if (approvalCase.MakerCheckers is not null)
        {
            foreach (var makerCheker in approvalCase.MakerCheckers)
            {
                makerCheker.RecordStatus = RecordStatus.Passive;
            }
        }

        await _caseRepository.UpdateAsync(approvalCase);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteCase",
                SourceApplication = "Approval",
                Resource = "Case",
                Details = new Dictionary<string, string>
                {
                     {"Id", approvalCase.Id.ToString() },
                }
            });

        return Unit.Value;
    }
}