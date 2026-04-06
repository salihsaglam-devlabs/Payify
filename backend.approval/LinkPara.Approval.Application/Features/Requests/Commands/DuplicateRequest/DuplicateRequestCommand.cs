using AutoMapper;
using LinkPara.Approval.Application.Commons.Exceptions;
using LinkPara.Approval.Application.Commons.Interfaces;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Approval.Application.Features.Requests.Commands.ApproveRequest;

public class DuplicateRequestCommand : IRequest<ApprovalResponse>
{
    public Guid UserId { get; set; }
    public Guid RequestId { get; set; }
}
public class DuplicateRequestCommandHandler : IRequestHandler<DuplicateRequestCommand, ApprovalResponse>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IRoleService _roleService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly ILogger<ApproveRequestCommandHandler> _logger;
    private readonly IStringLocalizer _localizer;


    public DuplicateRequestCommandHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IRoleService roleService,
        IEmailSenderService emailSenderService,
        ILogger<ApproveRequestCommandHandler> logger,
        IStringLocalizerFactory factory)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _roleService = roleService;
        _emailSenderService = emailSenderService;
        _logger = logger;
        _localizer = factory.Create("Tags", "LinkPara.Approval.API");
    }

    public async Task<ApprovalResponse> Handle(DuplicateRequestCommand request, CancellationToken cancellationToken)
    {
        var approvalRequest = await _requestRepository.GetByIdAsync(request.RequestId);


        if (approvalRequest is null)
        {
            throw new NotFoundException(nameof(Request), request.RequestId);
        }

        if (approvalRequest.Status != ApprovalStatus.Error)
        {
            throw new InvalidStatusException(approvalRequest.Status.ToString());
        }

        var duplicatedApprovalRequest = new Request
        {
            ActionType = approvalRequest.ActionType,
            Body = approvalRequest.Body,
            QueryParameters = approvalRequest.QueryParameters,
            Resource = approvalRequest.Resource,
            Status = string.IsNullOrEmpty(approvalRequest.FirstApproverDescription) ? ApprovalStatus.FirstApprovePending : ApprovalStatus.SecondApprovePending,
            FirstApproverDescription = approvalRequest.FirstApproverDescription,
            FirstApproveDate = approvalRequest.FirstApproveDate,
            CheckerUserId = approvalRequest.CheckerUserId,
            CheckerFullName = approvalRequest.CheckerFullName,
            Reason = approvalRequest.Reason,
            Url = approvalRequest.Url,
            MakerRoleId = approvalRequest.MakerRoleId,
            MakerUserId = approvalRequest.MakerUserId,
            CheckerRoleId = approvalRequest.CheckerRoleId,
            SecondCheckerRoleId = approvalRequest.SecondCheckerRoleId,
            DisplayName = approvalRequest.DisplayName,
            MakerFullName = approvalRequest.MakerFullName,
            UpdateDate = DateTime.Now,
            OperationType = approvalRequest.OperationType,
            ModuleName = approvalRequest.ModuleName,
            MakerDescription = approvalRequest.ReferenceId.ToString(),
            CreateDate = DateTime.Now,
            CreatedBy = request.UserId.ToString()
        };

        var dbApprovalRequest = await _requestRepository.AddAsync(duplicatedApprovalRequest);
        approvalRequest.Status = ApprovalStatus.Duplicated;
        await _requestRepository.UpdateAsync(approvalRequest);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DuplicateRequest",
            SourceApplication = "Approval",
            Resource = "Request",
            Details = new Dictionary<string, string>
            {
                {"UserId", request.UserId.ToString() },
                {"CheckerRoleIds", string.Join(',',duplicatedApprovalRequest.CheckerRoleId)},
                {"CheckerFullName", duplicatedApprovalRequest.CheckerFullName },
                {"DuplicatedRequestId",  request.RequestId.ToString()}
            }
        });

        return new ApprovalResponse
        {
            RequestId = duplicatedApprovalRequest.Id,
            Status = duplicatedApprovalRequest.Status,
            Request = _mapper.Map<RequestDto>(duplicatedApprovalRequest)
        };
    }

    private async Task SendNotificationMail(Request approvalRequest)
    {
        try
        {
            var users = await _roleService.GetUsersByRoleIdAsync(approvalRequest.SecondCheckerRoleId);
            if (users != null && users.Any())
            {
                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var mail = CreateSecondApprovalMail(user, approvalRequest.ReferenceId, approvalRequest.DisplayName);
                        await _emailSenderService.SendEmailAsync(mail);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("SendSecondApprovalMailError - RoleId: {RoleId} Exception: {Exception}", approvalRequest.SecondCheckerRoleId, e);

        }
    }

    private SendEmail CreateSecondApprovalMail(UserDto user, long referenceId, string displayName)
    {
        var templateData = new Dictionary<string, string>
        {
            { "subject", _localizer.GetString("ApprovalNotification") },
            { "content", $"{referenceId} - {displayName} {_localizer.GetString("SecondCheckerMailInfo")}" }
        };
        return new SendEmail
        {
            ToEmail = user.Email,
            TemplateName = string.Empty,
            DynamicTemplateData = templateData,
            AttachmentList = null,
            Attachment = null
        };
    }
}
