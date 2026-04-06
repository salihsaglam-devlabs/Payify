using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Application.Features.Days.Queries.IsHoliday;

public class IsHolidayQuery : IRequest<bool>
{
    public string CountryCode { get; set; }
    public DateTime Date { get; set; }
}

public class IsHolidayQueryHandler : IRequestHandler<IsHolidayQuery, bool>
{
    private readonly IDayInfoService _service;

    public IsHolidayQueryHandler(IDayInfoService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(IsHolidayQuery request, CancellationToken cancellationToken)
    {
        return await _service.IsHolidayAsync(request.CountryCode, request.Date);
    }
}
