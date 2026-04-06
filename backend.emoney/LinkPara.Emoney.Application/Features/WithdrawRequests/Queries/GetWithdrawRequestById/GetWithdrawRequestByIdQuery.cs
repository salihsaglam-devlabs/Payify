using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestById;

public class GetWithdrawRequestByIdQuery : IRequest<WithdrawRequestDto>
{
    public Guid Id { get; set; }
}

public class GetWithdrawRequestByIdQueryHandler : IRequestHandler<GetWithdrawRequestByIdQuery, WithdrawRequestDto>
{
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<WithdrawRequest> _repository;
    private readonly IMapper _mapper;

    public GetWithdrawRequestByIdQueryHandler(IGenericRepository<WithdrawRequest> repository,
        IMapper mapper, IGenericRepository<Transaction> transactionRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _transactionRepository = transactionRepository;
    }

    public async Task<WithdrawRequestDto> Handle(GetWithdrawRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var withdrawRequest = await _repository.GetByIdAsync(request.Id);

        if (withdrawRequest is null)
        {
            throw new NotFoundException(nameof(WithdrawRequest), request.Id);
        }

        var transaction = _transactionRepository.GetAll()
            .Select(x => new
            {
                x.Id,
                x.WithdrawRequestId,
                x.TransactionDate,
                x.SenderName
            })
            .FirstOrDefault(s => s.WithdrawRequestId == request.Id);

        var relatedTransactions = _transactionRepository.GetAll()
            .Where(s => s.RelatedTransactionId == transaction.Id);

        var response = _mapper.Map<WithdrawRequestDto>(withdrawRequest);

        var bsmv = relatedTransactions.Select(x => new
        {
            x.Tag,
            x.Amount,
            x.TransactionType
        }).FirstOrDefault(s => s.TransactionType == TransactionType.Tax);

        var pricing = relatedTransactions.Select(x => new
        {
            x.Tag,
            x.Amount,
            x.TransactionType
        }).FirstOrDefault(s => s.TransactionType == TransactionType.Commission);

        response.Bsmv = bsmv?.Amount ?? 0;
        response.Pricing = pricing?.Amount ?? 0;
        response.TransactionDate = transaction?.TransactionDate;
        response.SenderName = transaction?.SenderName;

        return response;
    }
}
