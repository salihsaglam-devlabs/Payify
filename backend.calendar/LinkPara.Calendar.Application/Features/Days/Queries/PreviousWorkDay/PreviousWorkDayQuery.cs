using LinkPara.Calendar.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Calendar.Application.Features.Days.Queries.PreviousWorkDay;

public class PreviousWorkDayQuery : IRequest<DateTime>
{
    public string CountryCode { get; set; }
    public DateTime Date { get; set; }
}

public class PreviousWorkDayQueryHandler : IRequestHandler<PreviousWorkDayQuery, DateTime>
{
    private readonly IDayInfoService _service;

    public PreviousWorkDayQueryHandler(IDayInfoService service)
    {
        _service = service;
    }

    public async Task<DateTime> Handle(PreviousWorkDayQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetPreviousWorkDayAsync(request.CountryCode, request.Date);
    }
}
