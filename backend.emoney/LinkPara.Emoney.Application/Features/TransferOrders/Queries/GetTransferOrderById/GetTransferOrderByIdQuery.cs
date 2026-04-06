using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Queries.GetTransferOrderById;

public class GetTransferOrderByIdQuery : IRequest<TransferOrderDto>
{
    public Guid Id { get; set; }
}

public class GetTransferOrderByIdQueryHandler : IRequestHandler<GetTransferOrderByIdQuery, TransferOrderDto>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<TransferOrder> _repository;

    public GetTransferOrderByIdQueryHandler(IMapper mapper,
        IGenericRepository<TransferOrder> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<TransferOrderDto> Handle(GetTransferOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var transferOrder = await _repository.GetAll().Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);

        if (transferOrder is null)
        {
            throw new NotFoundException(nameof(TransferOrder), query.Id);
        }

        return _mapper.Map<TransferOrder, TransferOrderDto>(transferOrder);
    }
}
