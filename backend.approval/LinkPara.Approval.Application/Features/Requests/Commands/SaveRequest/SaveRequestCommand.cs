using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.Approval.Application.Commons.Interfaces;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using Microsoft.Extensions.Localization;
using LinkPara.Approval.Application.Commons.Exceptions;

namespace LinkPara.Approval.Application.Features.Requests.Commands.SaveRequest;

public class SaveRequestCommand : IRequest<ApprovalResponse>
{
    public Guid UserId { get; set; }
    public List<Guid> MakerRoleIds { get; set; }
    public Guid CaseId { get; set; }
    public string Body { get; set; }
    public ActionType ActionType { get; set; }
    public string Resource { get; set; }
    public string Url { get; set; }
    public string QueryParameters { get; set; }
    public string DisplayName { get; set; }
    public string MakerFullName { get; set; }
}

public class SaveRequestCommandHandler : IRequestHandler<SaveRequestCommand, ApprovalResponse>
{
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IGenericRepository<MakerChecker> _makerCheckerRepository;
    private readonly ILogger<SaveRequestCommandHandler> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IRoleService _roleService;
    private readonly IStringLocalizer _localizer;


    public SaveRequestCommandHandler(IGenericRepository<Request> requestRepository,
        IGenericRepository<MakerChecker> makerCheckerRepository,
        ILogger<SaveRequestCommandHandler> logger,
        IAuditLogService auditLogService,
        IMapper mapper,
        IEmailSenderService emailSenderService,
        IRoleService roleService,
        IStringLocalizerFactory factory)
    {
        _requestRepository = requestRepository;
        _makerCheckerRepository = makerCheckerRepository;
        _logger = logger;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _emailSenderService = emailSenderService;
        _roleService = roleService;
        _localizer = factory.Create("Tags", "LinkPara.Approval.API");
    }

    public async Task<ApprovalResponse> Handle(SaveRequestCommand request, CancellationToken cancellationToken)
    {
        var makerChecker = await _makerCheckerRepository
            .GetAll()
            .Include(m => m.Case)
            .Where(s =>
                s.CaseId == request.CaseId
                && request.MakerRoleIds.Contains(s.MakerRoleId)
                && s.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);

        if (makerChecker is null)
        {
            _logger.LogInformation("ThereIsNoCheckerForThisMaker CaseId: {CaseId}, MakerRoleIds: {MakerRoleIds}", request.CaseId, string.Join(',', request.MakerRoleIds));

            return new ApprovalResponse
            {
                IsSuccess = false,
                Message = "ThereIsNoCheckerForThisMaker"
            };
        }

        var operationType = GetActionOperationType(request.ActionType);

        if (await IsExistAsync(request))
        {
            throw new SameRecordException();

        }

        var approvalRequest = new Request
        {
            ActionType = request.ActionType,
            Body = request.Body,
            QueryParameters = request.QueryParameters,
            CreatedBy = request.UserId.ToString(),
            Resource = request.Resource,
            Status = ApprovalStatus.FirstApprovePending,
            Url = request.Url,
            MakerRoleId = makerChecker.MakerRoleId,
            MakerUserId = request.UserId,
            CheckerRoleId = makerChecker.CheckerRoleId,
            SecondCheckerRoleId = makerChecker.SecondCheckerRoleId,
            DisplayName = request.DisplayName,
            MakerFullName = request.MakerFullName,
            UpdateDate = DateTime.Now,
            OperationType = operationType,
            ModuleName = makerChecker.Case.ModuleName
        };

        var dbApprovalRequest = await _requestRepository.AddAsync(approvalRequest);

        if (dbApprovalRequest.CheckerRoleId != Guid.Empty)
        {
            await SendNotificationMail(dbApprovalRequest);
        }

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SaveRequest",
            SourceApplication = "Approval",
            Resource = "Request",
            Details = new Dictionary<string, string>
            {
                {"UserId", request.UserId.ToString() },
                {"MakerRoleId", makerChecker.MakerRoleId.ToString() },
                {"MakerFullName", request.MakerFullName },
            }
        });

        return new ApprovalResponse
        {
            RequestId = approvalRequest.Id,
            Status = approvalRequest.Status,
            Request = _mapper.Map<RequestDto>(dbApprovalRequest),
            IsSuccess = true
        };
    }

    private async Task SendNotificationMail(Request approvalRequest)
    {
        try
        {
            var users = await _roleService.GetUsersByRoleIdAsync(approvalRequest.CheckerRoleId);

            if (users != null && users.Any())
            {
                foreach (var user in users)
                {
                    var mail = CreateFirstApprovalMail(user, approvalRequest.ReferenceId, approvalRequest.DisplayName);
                    await _emailSenderService.SendEmailAsync(mail);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("SendFirstApprovalMailError RoleId: {CheckerRoleId} Exception: {Exception}", approvalRequest.CheckerRoleId, e);
        }
    }

    private async Task<bool> IsExistAsync(SaveRequestCommand request)
    {
        return await _requestRepository.GetAll().AnyAsync(x =>
            x.ActionType == request.ActionType &&
            x.Body == request.Body &&
            x.QueryParameters == request.QueryParameters &&
            x.Resource == request.Resource &&
            x.Url == request.Url &&
            (x.Status == ApprovalStatus.FirstApprovePending || x.Status == ApprovalStatus.SecondApprovePending)
        );
    }

    private RequestOperationType GetActionOperationType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Delete:
                return RequestOperationType.Delete;
            case ActionType.Post:
                return RequestOperationType.Create;
            case ActionType.Put:
            case ActionType.Patch:
                return RequestOperationType.Update;
            default:
                throw new InvalidOperationException();
        }
    }

    private SendEmail CreateFirstApprovalMail(UserDto user, long referenceId, string displayName)
    {
        var templateData = new Dictionary<string, string>
        {
            { "subject", _localizer.GetString("ApprovalNotification") },
            { "content", $"{referenceId} - {displayName} {_localizer.GetString("FirstCheckerMailInfo")}" }
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
