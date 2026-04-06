using AutoMapper;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Application.Features.Holidays.Queries.GetHolidays;

public class GetHolidaysQuery : IRequest<List<HolidayDto>>
{
    public string CountryCode { get; set; }
}

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, List<HolidayDto>>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Holiday> _repository;

    public GetHolidaysQueryHandler(IMapper mapper, IGenericRepository<Holiday> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<List<HolidayDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        var holidays = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.CountryCode))
        {
            holidays = holidays.Where(s => s.CountryCode == request.CountryCode);
        }

        var response = await holidays.ToListAsync(cancellationToken: cancellationToken);

        return _mapper.Map<List<HolidayDto>>(response);
    }
}
