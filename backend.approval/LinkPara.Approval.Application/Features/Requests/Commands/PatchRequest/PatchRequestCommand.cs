using AutoMapper;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Requests.Commands.PatchRequest;

public class PatchRequestCommand : IRequest
{
    public Guid RequestId { get; set; }
    public JsonPatchDocument<PatchRequestDto> PatchRequestDto { get; set; }
}

public class UpdateErrorRequestCommandHandler : IRequestHandler<PatchRequestCommand>
{
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IMapper _mapper;

    public UpdateErrorRequestCommandHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(PatchRequestCommand request, CancellationToken cancellationToken)
    {

        var approvalRequest = await _requestRepository.GetByIdAsync(request.RequestId);

        if (approvalRequest is null)
        {
            throw new NotFoundException(nameof(Request), request.RequestId);
        }

        var requestPatchDto = _mapper.Map<PatchRequestDto>(approvalRequest);

        request.PatchRequestDto.ApplyTo(requestPatchDto);

        _mapper.Map(requestPatchDto, approvalRequest);

        await _requestRepository.UpdateAsync(approvalRequest);

        return Unit.Value;
    }
}
