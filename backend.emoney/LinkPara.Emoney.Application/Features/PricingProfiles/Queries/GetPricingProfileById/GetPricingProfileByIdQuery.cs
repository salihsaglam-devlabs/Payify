using AutoMapper;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileById;

public class GetPricingProfileByIdQuery : IRequest<PricingProfileDto>
{
    public Guid Id { get; set; }
}

public class GetPricingProfileByIdQueryHandler : IRequestHandler<GetPricingProfileByIdQuery, PricingProfileDto>
{
    private readonly IMapper _mapper;
    private readonly IPricingProfileService _service;

    public GetPricingProfileByIdQueryHandler(IMapper mapper, IPricingProfileService service)
    {
        _mapper = mapper;
        _service = service;
    }

    public async Task<PricingProfileDto> Handle(GetPricingProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _service.GetByIdAsync(request.Id);

        return _mapper.Map<PricingProfileDto>(profile);
    }
}
