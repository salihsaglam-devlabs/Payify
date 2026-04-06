using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Queries.GetCallCenterNotificationStatus;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Commands.SendCallCenterNotificationAction;
public class SendCallCenterNotificationActionCommand : IRequest<CallCenterNotificationActionResponse>
{
    public Guid PushNotificationId { get; set; }
    public CallCenterNotificationStatus Action { get; set; }
}

public class SendCallCenterNotificationActionCommandHandler : IRequestHandler<SendCallCenterNotificationActionCommand, CallCenterNotificationActionResponse>
{
    private readonly IGenericRepository<CallCenterNotificationLog> _callcenterRepository;
    private readonly ILogger<GetCallCenterNotificationStatusQueryHandler> _logger;
    private readonly IStringLocalizer _localizer;

    public SendCallCenterNotificationActionCommandHandler(
        IGenericRepository<CallCenterNotificationLog> callcenterRepository,
        ILogger<GetCallCenterNotificationStatusQueryHandler> logger,
        IStringLocalizerFactory factory)
    {
        _callcenterRepository = callcenterRepository;
        _logger = logger;
        _localizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
    }

    public async Task<CallCenterNotificationActionResponse> Handle(SendCallCenterNotificationActionCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CallCenterNotificationActionResponse();
       
        try
        {
            response.IsSuccess = true;
            var notificationLog = await _callcenterRepository.GetByIdAsync(request.PushNotificationId);
            var now = DateTime.Now;

            if (notificationLog != null)
            {
                if (notificationLog.Status != CallCenterNotificationStatus.Approve && notificationLog.Status != CallCenterNotificationStatus.Reject)
                {

                    notificationLog.Status = notificationLog.Status == CallCenterNotificationStatus.Pending && now > notificationLog.ExpireDate ? CallCenterNotificationStatus.Expired : request.Action;
                    
                    await _callcenterRepository.UpdateAsync(notificationLog);

                    if (notificationLog.Status == CallCenterNotificationStatus.Expired)
                    {
                        response.IsSuccess = false;
                        response.ErrorMessage = _localizer.GetString("NotificationExpired").Value;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = notificationLog.Status == CallCenterNotificationStatus.Approve ? _localizer.GetString("NotificationAlreadyApproved").Value : _localizer.GetString("NotificationAlreadyRejected").Value;
                   
                }
            }
        }
        catch (Exception e)
        {
            response.IsSuccess = false;
            response.ErrorMessage = e.Message;
            _logger.LogError($"SendCallCenterNotificationActionCommand: {e}");
        }

        return response;
    }
}
