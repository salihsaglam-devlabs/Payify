using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequestById;

public class GetRequestByIdQuery : IRequest<RequestDto>
{
    public Guid Id { get; set; }
}

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, RequestDto>
{
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IMapper _mapper;
    public GetRequestByIdQueryHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<RequestDto> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var approvalRequest = await _requestRepository
            .GetAll()
            .Where(s => s.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (approvalRequest is null)
        {
            throw new NotFoundException(nameof(Request), request.Id);
        }

        return _mapper.Map<RequestDto>(approvalRequest);
    }
}
