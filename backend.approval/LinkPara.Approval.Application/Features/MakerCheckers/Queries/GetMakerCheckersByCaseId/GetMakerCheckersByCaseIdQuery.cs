using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerCheckersByCaseId;

public class GetMakerCheckersByCaseIdQuery : IRequest<List<MakerCheckerDto>>
{
    public Guid CaseId { get; set; }
}
public class GetMakerCheckersByCaseIdQueryHandler : IRequestHandler<GetMakerCheckersByCaseIdQuery, List<MakerCheckerDto>>
{
    private readonly IGenericRepository<MakerChecker> _repository;
    private readonly IMapper _mapper;

    public GetMakerCheckersByCaseIdQueryHandler(IGenericRepository<MakerChecker> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<List<MakerCheckerDto>> Handle(GetMakerCheckersByCaseIdQuery request, CancellationToken cancellationToken)
    {
        var makerCheckers = await _repository.GetAll()
                                         .Where(s => s.CaseId == request.CaseId
                                         && s.RecordStatus == RecordStatus.Active).ToListAsync();

        return _mapper.Map<List<MakerCheckerDto>>(makerCheckers);
    }
}
