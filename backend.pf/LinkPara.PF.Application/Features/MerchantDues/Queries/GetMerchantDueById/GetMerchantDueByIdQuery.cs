using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDues.Queries.GetMerchantDueById;

public class GetMerchantDueByIdQuery : IRequest<MerchantDueDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantDueByIdQueryHandler : IRequestHandler<GetMerchantDueByIdQuery, MerchantDueDto>
{
    private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
    private readonly IMapper _mapper;

    public GetMerchantDueByIdQueryHandler(IGenericRepository<MerchantDue> merchantDueRepository, IMapper mapper)
    {
        _merchantDueRepository = merchantDueRepository;
        _mapper = mapper;
    }
    
    public async Task<MerchantDueDto> Handle(GetMerchantDueByIdQuery request, CancellationToken cancellationToken)
    {
        var merchantDue = await _merchantDueRepository.GetAll()
            .Include(b => b.DueProfile)
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);
        
        if (merchantDue is null)
        {
            throw new NotFoundException(nameof(MerchantDue), request.Id);
        }
        
        return _mapper.Map<MerchantDueDto>(merchantDue);
    }
}