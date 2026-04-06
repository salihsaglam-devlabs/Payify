using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Cases.Queries.GetById;

public class GetByIdQuery : IRequest<CaseDto>
{
    public Guid Id { get; set; }
}

public class GetByIdQueryHandler : IRequestHandler<GetByIdQuery, CaseDto>
{
    private readonly IGenericRepository<Case> _caseRepository;
    private readonly IMapper _mapper;
    public GetByIdQueryHandler(IGenericRepository<Case> caseRepository,
        IMapper mapper)
    {
        _caseRepository = caseRepository;
        _mapper = mapper;
    }

    public async Task<CaseDto> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        var approvalCase = await _caseRepository.GetAll()
                                                .Include(s => s.MakerCheckers
                                                             .Where(x => x.RecordStatus == RecordStatus.Active)
                                                        )
                                                .FirstOrDefaultAsync(s => s.Id == request.Id
                                                , cancellationToken);
        if (approvalCase is null)
        {
            throw new NotFoundException(nameof(Case), request.Id);
        }

        return _mapper.Map<CaseDto>(approvalCase);
    }
}
