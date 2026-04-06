using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantLogo;

public class GetMerchantLogoQuery : IRequest<MerchantLogoDto>
{
    public Guid MerchantId { get; set; }
}

public class GetMerchantLogoQueryHandler : IRequestHandler<GetMerchantLogoQuery, MerchantLogoDto>
{
    private readonly IGenericRepository<MerchantLogo> _merchantLogoRepository;
    private readonly IMapper _mapper;

    public GetMerchantLogoQueryHandler(
        IGenericRepository<MerchantLogo> merchantLogoRepository,
        IMapper mapper)
    {
        _merchantLogoRepository = merchantLogoRepository;
        _mapper = mapper;
    }

    public async Task<MerchantLogoDto> Handle(GetMerchantLogoQuery query, CancellationToken cancellationToken)
    {
        var linkLogo = await _merchantLogoRepository.GetAll()
            .Where(l => 
                l.RecordStatus == RecordStatus.Active && 
                l.MerchantId == query.MerchantId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return linkLogo is null ? new MerchantLogoDto() : _mapper.Map<MerchantLogoDto>(linkLogo);
    }
}