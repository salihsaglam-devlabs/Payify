using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount.Queries.GetCallCenterNotificationStatus;
public class GetCallCenterNotificationStatusQuery : SearchQueryParams, IRequest<CallCenterNotificationStatusResponse>
{
    public Guid NotificationId { get; set; }
}

public class GetCallCenterNotificationStatusQueryHandler : IRequestHandler<GetCallCenterNotificationStatusQuery, CallCenterNotificationStatusResponse>
{
    private readonly ILogger<GetCallCenterNotificationStatusQueryHandler> _logger;
    private readonly IGenericRepository<CallCenterNotificationLog> _callcenterRepository;

    public GetCallCenterNotificationStatusQueryHandler(
        ILogger<GetCallCenterNotificationStatusQueryHandler> logger,
        IGenericRepository<CallCenterNotificationLog> callcenterRepository)
    {
        _logger = logger;
        _callcenterRepository = callcenterRepository;
    }

    public async Task<CallCenterNotificationStatusResponse> Handle(GetCallCenterNotificationStatusQuery request,
        CancellationToken cancellationToken)
    {
        var response = new CallCenterNotificationStatusResponse();
        var notificationLog = new CallCenterNotificationLog();
        try
        {
            notificationLog = await _callcenterRepository.GetByIdAsync(request.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"CallCenterNotificationLog NotFound - Input:{request.NotificationId} - Error: {ex}");
            response.Status = CallCenterNotificationStatus.NotFound;
            notificationLog = null;
        }
        
        if (notificationLog != null)
        {
            response.Status = notificationLog.Status;
            var now = DateTime.Now;

            if (notificationLog.Status == CallCenterNotificationStatus.Pending && now > notificationLog.ExpireDate)
            {
                notificationLog.Status = CallCenterNotificationStatus.Expired;
                await _callcenterRepository.UpdateAsync(notificationLog);

                response.Status = CallCenterNotificationStatus.Expired;
            }
        }
        
        return response;
    }
}
