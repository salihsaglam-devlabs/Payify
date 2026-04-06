using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models;
using MediatR;

namespace LinkPara.Fraud.Application.Features.OngoingMonitorings.Commands.RemoveOngoingMonitoring;

public class RemoveOngoingMonitoringCommand : IRequest<BaseResponse>
{
    public string ReferenceNumber { get; set; }
}
public class RemoveOngoingMonitoringCommandHandler : IRequestHandler<RemoveOngoingMonitoringCommand, BaseResponse>
{
    private readonly ISearchService _searchService;

    public RemoveOngoingMonitoringCommandHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<BaseResponse> Handle(RemoveOngoingMonitoringCommand request, CancellationToken cancellationToken)
    {
       return await _searchService.RemoveOngoingAsync(request.ReferenceNumber);
    }
}