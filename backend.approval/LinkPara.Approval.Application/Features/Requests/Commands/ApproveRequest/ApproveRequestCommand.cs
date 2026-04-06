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

public class ApproveRequestCommand : IRequest<ApprovalResponse>
{
    public Guid UserId { get; set; }
    public Guid RequestId { get; set; }
    public List<Guid> CheckerRoleIds { get; set; }
    public string CheckerFullName { get; set; }
    public string Description { get; set; }
}
public class ApproveRequestCommandHandler : IRequestHandler<ApproveRequestCommand, ApprovalResponse>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IRoleService _roleService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly ILogger<ApproveRequestCommandHandler> _logger;
    private readonly IStringLocalizer _localizer;


    public ApproveRequestCommandHandler(IGenericRepository<Request> requestRepository,
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
    
    public async Task<ApprovalResponse> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        var approvalRequest = await _requestRepository.GetByIdAsync(request.RequestId);

        if(approvalRequest is null)
        {
            throw new NotFoundException(nameof(Request), request.RequestId);
        }

        if (!(approvalRequest.Status == ApprovalStatus.FirstApprovePending
              || approvalRequest.Status == ApprovalStatus.SecondApprovePending))
        {
            throw new InvalidStatusException(approvalRequest.Status.ToString());
        }

        if (approvalRequest.MakerUserId == request.UserId)
        {
            throw new MakerAndCheckerAreSameException();
        }
        
        if(approvalRequest.Status == ApprovalStatus.FirstApprovePending)
        {
            if(request.CheckerRoleIds.Contains(approvalRequest.CheckerRoleId))
            {
                approvalRequest.Status = approvalRequest.SecondCheckerRoleId == Guid.Empty 
                                                        ? ApprovalStatus.Approved 
                                                        : ApprovalStatus.SecondApprovePending;
                approvalRequest.CheckerUserId = request.UserId;
                approvalRequest.CheckerFullName = request.CheckerFullName;
                approvalRequest.FirstApproverDescription = request.Description;
                approvalRequest.FirstApproveDate = DateTime.Now;
            }
            else 
            {
                throw new NotRelevantApproverException();
            }
        }
        else
        {
            if (request.CheckerRoleIds.Contains(approvalRequest.SecondCheckerRoleId))
            {
                approvalRequest.Status = ApprovalStatus.Approved;
                approvalRequest.SecondCheckerUserId = request.UserId;
                approvalRequest.SecondCheckerFullName = request.CheckerFullName;
                approvalRequest.SecondApproverDescription = request.Description;
                approvalRequest.SecondApproveDate = DateTime.Now;
            }
            else
            {
                throw new NotRelevantApproverException();
            }
        }

        await _requestRepository.UpdateAsync(approvalRequest);
        
        if (approvalRequest.Status == ApprovalStatus.SecondApprovePending)
        {
            await SendNotificationMail(approvalRequest);
        }
        
        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "ApproveRequest",
            SourceApplication = "Approval",
            Resource = "Request",
            Details = new Dictionary<string, string>
            {
                {"UserId", request.UserId.ToString() },
                {"CheckerRoleIds", string.Join(',',request.CheckerRoleIds)},
                {"CheckerFullName", request.CheckerFullName },
            }
        });

        return new ApprovalResponse
        {
            RequestId = approvalRequest.Id,
            Status = approvalRequest.Status,
            Request = _mapper.Map<RequestDto>(approvalRequest)
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
