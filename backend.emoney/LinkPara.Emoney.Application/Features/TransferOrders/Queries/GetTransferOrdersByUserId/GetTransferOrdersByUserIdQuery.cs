using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Queries.GetTransferOrdersByUserId;

public class GetTransferOrdersByUserIdQuery : SearchQueryParams, IRequest<PaginatedList<TransferOrderDto>>
{
    public ReceiverAccountType? ReceiverAccountType { get; set; }
    public TransferOrderStatus? TransferOrderStatus { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType? SenderUserType { get; set; }
    public DateTime? TransferDateStart { get; set; }
    public DateTime? TransferDateEnd { get; set; }
    public Guid? UserId { get; set; }
}

public class GetTransferOrdersByUserIdQueryHandler : IRequestHandler<GetTransferOrdersByUserIdQuery, PaginatedList<TransferOrderDto>>
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<TransferOrder> _repository;

    public GetTransferOrdersByUserIdQueryHandler(IMapper mapper,
        IGenericRepository<TransferOrder> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<PaginatedList<TransferOrderDto>> Handle(GetTransferOrdersByUserIdQuery query, CancellationToken cancellationToken)
    {
        var transferOrders = _repository.GetAll().Include(b => b.Currency)
            .Where(b=>b.RecordStatus == RecordStatus.Active);

        if (query.UserId is not null)
        {
            transferOrders = transferOrders.Where(b => b.UserId.Equals(query.UserId));
        }

        if (!string.IsNullOrEmpty(query.Q))
        {
            transferOrders = transferOrders.Where(b => b.ReceiverNameSurname.ToLower().Contains(query.Q.ToLower()));
        }

        if (!string.IsNullOrEmpty(query.ReceiverAccountValue))
        {
            transferOrders = transferOrders.Where(b => b.ReceiverAccountValue.Contains(query.ReceiverAccountValue));
        }

        if (!string.IsNullOrEmpty(query.ReceiverNameSurname))
        {
            transferOrders = transferOrders.Where(b => b.ReceiverNameSurname.ToLower()
                                           .Contains(query.ReceiverNameSurname.ToLower()));
        }

        if (!string.IsNullOrEmpty(query.SenderWalletNumber))
        {
            transferOrders = transferOrders.Where(b => b.SenderWalletNumber.Contains(query.SenderWalletNumber));
        }

        if (query.ReceiverAccountType is not null)
        {
            transferOrders = transferOrders.Where(b => b.ReceiverAccountType == query.ReceiverAccountType);
        }

        if (query.TransferOrderStatus is not null)
        {
            transferOrders = transferOrders.Where(b => b.TransferOrderStatus == query.TransferOrderStatus);
        }

        if (query.TransferDateStart is not null)
        {
            transferOrders = transferOrders.Where(b => b.TransferDate >= query.TransferDateStart);
        }

        if (query.TransferDateEnd is not null)
        {
            transferOrders = transferOrders.Where(b => b.TransferDate <= query.TransferDateEnd);
        }

        if (!string.IsNullOrEmpty(query.SenderNameSurname))
        {
            transferOrders = transferOrders.Where(b => b.SenderNameSurname.ToLower()
                                           .Contains(query.SenderNameSurname.ToLower()));
        }

        if (query.SenderUserType is not null)
        {
            transferOrders = transferOrders.Where(b => b.SenderUserType == query.SenderUserType);
        }

        return await transferOrders.OrderByDescending(b=>b.TransferDate)
            .PaginatedListWithMappingAsync<TransferOrder,TransferOrderDto>(_mapper, query.Page, query.Size, query.OrderBy, query.SortBy);
    }
}