using LinkPara.Approval.Application.Commons.Exceptions;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Approval.Application.Features.Requests.Commands.RejectRequest;

public class RejectRequestCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid RequestId { get; set; }
    public List<Guid> CheckerRoleIds { get; set; }
    public string CheckerFullName { get; set; }
    public string Reason { get; set; }
}

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand>
{
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IAuditLogService _auditLogService;

    public RejectRequestCommandHandler(IGenericRepository<Request> requestRepository,
        IAuditLogService auditLogService)
    {
        _requestRepository = requestRepository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(RejectRequestCommand request, CancellationToken cancellationToken)
    {
        var approvalRequest = await _requestRepository.GetByIdAsync(request.RequestId);

        if (approvalRequest is null)
        {
            throw new NotFoundException(nameof(Request), request.RequestId);
        }

        if (!(approvalRequest.Status == ApprovalStatus.FirstApprovePending
           || approvalRequest.Status == ApprovalStatus.SecondApprovePending))
        {
            throw new InvalidStatusException(approvalRequest.Status.ToString());
        }

        if (approvalRequest.Status == ApprovalStatus.FirstApprovePending 
            && request.CheckerRoleIds.Contains(approvalRequest.CheckerRoleId)) {

            approvalRequest.Reason = request.Reason;
            approvalRequest.CheckerUserId = request.UserId;
            approvalRequest.CheckerFullName = request.CheckerFullName;
            approvalRequest.Status = ApprovalStatus.Rejected;

        }
        else if (approvalRequest.Status == ApprovalStatus.SecondApprovePending
            && request.CheckerRoleIds.Contains(approvalRequest.SecondCheckerRoleId))
        {

            approvalRequest.Reason = request.Reason;
            approvalRequest.SecondCheckerUserId = request.UserId;
            approvalRequest.SecondCheckerFullName = request.CheckerFullName;
            approvalRequest.Status = ApprovalStatus.Rejected;
        }
        else
        {
            throw new NotRelevantApproverException();
        }

        if(approvalRequest.Status == ApprovalStatus.Rejected)
        {
            approvalRequest.RejectDate = DateTime.Now;
        }

        await _requestRepository.UpdateAsync(approvalRequest);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "RejectRequest",
            SourceApplication = "Approval",
            Resource = "Request",
            Details = new Dictionary<string, string>
            {
                {"UserId", request.UserId.ToString() },
                {"CheckerRoleId", string.Join(',',request.CheckerRoleIds) },
                {"CheckerFullName", request.CheckerFullName },
            }
        });

        return Unit.Value;
    }
}