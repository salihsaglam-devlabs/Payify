using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Models.Enums;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Cases.Commands.SaveCase;

public class SaveCaseCommand : IRequest
{
    public string BaseUrl { get; set; }
    public string Resource { get; set; }
    public ActionType Action { get; set; }
    public string DisplayName { get; set; }
}

public class SaveCaseCommandHandler : IRequestHandler<SaveCaseCommand>
{
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IAuditLogService _auditLogService;

    public SaveCaseCommandHandler(IGenericRepository<Case> caseRepository,
                                  IAuditLogService auditLogService)
    {
        _caseRepository = caseRepository;
        _auditLogService = auditLogService; 
    }
    public async Task<Unit> Handle(SaveCaseCommand request, CancellationToken cancellationToken)
    {
        if (await IsExistAsync(request))
            throw new DuplicateRecordException();

        var approvalCase = new Case
        {
            Action = request.Action,
            BaseUrl = request.BaseUrl,
            Resource = request.Resource,
            DisplayName = request.DisplayName
        };

        await _caseRepository.AddAsync(approvalCase);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SaveCase",
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

    private async Task<bool> IsExistAsync(SaveCaseCommand request)
    {
        return await _caseRepository.GetAll().AnyAsync(x =>
            x.BaseUrl == request.BaseUrl &&
            x.Resource == request.Resource &&
            x.Action == request.Action && 
            x.RecordStatus == RecordStatus.Active
        );
    }
}