using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantContents.Queries.GetMerchantContentById;

public class GetMerchantContentByIdQuery : IRequest<MerchantContentDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantContentByIdQueryHandler : IRequestHandler<GetMerchantContentByIdQuery, MerchantContentDto>
{
    private readonly IGenericRepository<MerchantContent> _repository;
    private readonly IMapper _mapper;
    
    public GetMerchantContentByIdQueryHandler(
        IGenericRepository<MerchantContent> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MerchantContentDto> Handle(GetMerchantContentByIdQuery request, CancellationToken cancellationToken)
    {
        var merchantContent = await _repository
            .GetAll()
            .Include(lc => lc.Contents.Where(a => a.RecordStatus == RecordStatus.Active))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (merchantContent == null)
        {
            throw new NotFoundException(nameof(MerchantContent), request.Id);
        }
        
        return _mapper.Map<MerchantContentDto>(merchantContent);
    }
}