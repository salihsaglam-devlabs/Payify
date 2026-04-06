using AutoMapper;
using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Queries.GetHolidayDetails;

public class GetHolidayDetailsQuery : IRequest<List<HolidayDetailDto>>
{
    public string HolidayId { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
}

public class GetHolidayDetailsQueryHandler : IRequestHandler<GetHolidayDetailsQuery, List<HolidayDetailDto>>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<HolidayDetail> _detailRepository;    

    public GetHolidayDetailsQueryHandler(IMapper mapper,
        IGenericRepository<HolidayDetail> repository)
    {
        _mapper = mapper;
        _detailRepository = repository;
    }

    public async Task<List<HolidayDetailDto>> Handle(GetHolidayDetailsQuery request, CancellationToken cancellationToken)
    {
        var details = _detailRepository.GetAll(s => s.Holiday);

        if (!string.IsNullOrEmpty(request.HolidayId))
        {
            var parseHolidayId = Guid.TryParse(request.HolidayId.ToString(), out var holidayId);

            if (parseHolidayId)
            {
                details = details.Where(s => s.HolidayId == holidayId);
            }
        }

        if (!string.IsNullOrEmpty(request.CountryCode))
        {
            details = details.Where(s => s.Holiday.CountryCode == request.CountryCode);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            details = details.Where(s => s.Holiday.Name.Contains(request.Name));
        }

        var response = await details.ToListAsync(cancellationToken: cancellationToken);

        return _mapper.Map<List<HolidayDetailDto>>(response);
    }
}



