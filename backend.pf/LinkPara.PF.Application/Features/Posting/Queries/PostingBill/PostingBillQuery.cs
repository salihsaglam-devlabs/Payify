using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Posting.Queries;

public class PostingBillQuery : SearchQueryParams, IRequest<PaginatedList<PostingBillDto>>
{
    public Guid? MerchantId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? BillMonth { get; set; }
}

public class PostingBillQueryHandler : IRequestHandler<PostingBillQuery, PaginatedList<PostingBillDto>>
{
    private readonly IGenericRepository<PostingBill> _postingBillRepository;
    private readonly ICurrencyService _currencyService;

    public PostingBillQueryHandler(IGenericRepository<PostingBill> postingBillRepository, ICurrencyService currencyService)
    {
        _postingBillRepository = postingBillRepository;
        _currencyService = currencyService;
    }

    public async Task<PaginatedList<PostingBillDto>> Handle(PostingBillQuery request, CancellationToken cancellationToken)
    {
        var bills = _postingBillRepository
            .GetAll();

        if (request.MerchantId != null)
        {
            bills = bills.Where(b => b.MerchantId ==  request.MerchantId);
        }

        if (request.StartDate != null)
        {
            bills = bills.Where(b => b.BillDate >= request.StartDate);
        }

        if (request.EndDate != null)
        {
            bills = bills.Where(b => b.BillDate <= request.EndDate);
        }

        if (request.MaxAmount != null)
        {
            bills = bills.Where(b => b.TotalAmount <= request.MaxAmount);
        }

        if (request.MinAmount != null)
        {
            bills = bills.Where(b => b.TotalAmount >= request.MinAmount);
        }

        if (request.BillMonth != null)
        {
            bills = bills.Where(b => b.BillMonth == request.BillMonth);
        }

        var result = await bills
            .Include(b => b.Merchant)
            .Select(b => new PostingBillDto
            {
                MerchantId = b.MerchantId,
                MerchantName = b.Merchant.Name,
                BillAmount = b.TotalAmount,
                BillDate = b.BillDate,
                BillMonth = b.BillMonth,
                CurrencyCode = b.Currency,
                HasBsmv = true
            })
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);


        return await SetCurrencyNamesAsync(result);
    }

    private async Task<PaginatedList<PostingBillDto>> SetCurrencyNamesAsync(PaginatedList<PostingBillDto> bills)
    {
        foreach (var bill in bills.Items)
        {
            var billCurrency = await _currencyService.GetByNumberAsync(bill.CurrencyCode);

            bill.CurrencyName = billCurrency?.Name;
            bill.CurrencyIsoCode = billCurrency?.Code;
        }

        return bills;
    }
}