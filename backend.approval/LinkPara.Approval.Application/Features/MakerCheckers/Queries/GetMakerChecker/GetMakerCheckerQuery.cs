using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Queries.GetMakerChecker;

public class GetMakerCheckerQuery : IRequest<MakerCheckerDto>
{
    public Guid Id { get; set; }
}

public class GetMakerCheckerQueryHandler : IRequestHandler<GetMakerCheckerQuery, MakerCheckerDto>
{

    private readonly IGenericRepository<MakerChecker> _repository;
    private readonly IMapper _mapper;

    public GetMakerCheckerQueryHandler(IGenericRepository<MakerChecker> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MakerCheckerDto> Handle(GetMakerCheckerQuery request, CancellationToken cancellationToken)
    {
        var makerChecker = await _repository.GetAll()
                                         .FirstOrDefaultAsync(s => s.Id == request.Id
                                                                && s.RecordStatus == RecordStatus.Active
                                                                , cancellationToken: cancellationToken);

        return _mapper.Map<MakerCheckerDto>(makerChecker);
    }
}