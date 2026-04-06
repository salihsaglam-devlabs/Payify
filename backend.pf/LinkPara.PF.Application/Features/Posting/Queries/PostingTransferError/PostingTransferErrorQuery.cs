using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Posting.Queries;

public class PostingTransferErrorQuery : SearchQueryParams, IRequest<PaginatedList<PostingTransferErrorDto>>
{
    public Guid? MerchantId { get; set; }
    public Guid? MerchantTransactionId { get; set; }
    public DateTime? PostingDateStart { get; set; }
    public DateTime? PostingDateEnd { get; set; }
    public DateTime? TransactionStartDate { get; set; }
    public DateTime? TransactionEndDate { get; set; }
    public TransferErrorCategory? TransferErrorCategory { get; set; }
    public string MerchantOrderId { get; set; }
}

public class PostingTransferErrorQueryHandler : IRequestHandler<PostingTransferErrorQuery, PaginatedList<PostingTransferErrorDto>>
{
    private readonly IGenericRepository<PostingTransferError> _postingTransferErrorRepository;

    public PostingTransferErrorQueryHandler(IGenericRepository<PostingTransferError> postingTransferErrorRepository)
    {
        _postingTransferErrorRepository = postingTransferErrorRepository;
    }

    public async Task<PaginatedList<PostingTransferErrorDto>> Handle(PostingTransferErrorQuery request, CancellationToken cancellationToken)
    {
        var errors = _postingTransferErrorRepository
            .GetAll()
            .Include(i => i.Merchant)
            .Include(i => i.MerchantTransaction)
            .Select(s => new PostingTransferErrorDto
            {
                PostingDate = s.PostingDate,
                MerchantId = s.MerchantId,
                MerchantName = s.Merchant.Name,
                MerchantTransactionId = s.MerchantTransactionId,
                TransactionDate = s.MerchantTransaction.TransactionDate,
                Amount = s.MerchantTransaction.Amount,
                Currency = s.MerchantTransaction.Currency,
                ErrorMessage = s.ErrorMessage,
                TransferErrorCategory = s.TransferErrorCategory,
                MerchantNumber = s.Merchant.Number,
                MerchantOrderId = s.MerchantTransaction.OrderId
            });

        if (request.PostingDateStart != null)
        {
            errors = errors.Where(w => w.PostingDate >= request.PostingDateStart);
        }

        if (request.PostingDateEnd != null)
        {
            errors = errors.Where(w => w.PostingDate <= request.PostingDateEnd);
        }

        if (request.TransactionStartDate != null)
        {
            errors = errors.Where(w => w.TransactionDate >= request.TransactionStartDate);
        }

        if (request.TransactionEndDate != null)
        {
            errors = errors.Where(w => w.TransactionDate <= request.TransactionEndDate);
        }

        if (request.TransferErrorCategory != null)
        {
            errors = errors.Where(w => w.TransferErrorCategory == request.TransferErrorCategory);
        }

        if (request.MerchantId != null)
        {
            errors = errors.Where(w => w.MerchantId == request.MerchantId);
        }

        if (request.MerchantTransactionId != null)
        {
            errors = errors.Where(w => w.MerchantTransactionId == request.MerchantTransactionId);
        }

        if (!string.IsNullOrWhiteSpace(request.MerchantOrderId))
        {
            errors = errors.Where(w => w.MerchantOrderId == request.MerchantOrderId);
        }

        return await errors
            .OrderByDescending(o => o.PostingDate)
            .PaginatedListAsync(request.Page, request.Size);
    }
}