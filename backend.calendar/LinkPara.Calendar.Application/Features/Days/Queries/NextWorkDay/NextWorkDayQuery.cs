using LinkPara.Calendar.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Calendar.Application.Features.Days.Queries.NextWorkDay;

public class NextWorkDayQuery : IRequest<DateTime>
{
    public string CountryCode { get; set; }
    public DateTime Date { get; set; }
}

public class NextWorkDayQueryHandler : IRequestHandler<NextWorkDayQuery, DateTime>
{
    private readonly IDayInfoService _service;

    public NextWorkDayQueryHandler(IDayInfoService service)
    {
        _service = service;
    }

    public async Task<DateTime> Handle(NextWorkDayQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetNextWorkDayAsync(request.CountryCode, request.Date);
    }
}
