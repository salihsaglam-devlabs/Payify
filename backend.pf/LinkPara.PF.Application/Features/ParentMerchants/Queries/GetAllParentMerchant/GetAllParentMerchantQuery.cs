using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.ParentMerchants.Queries.GetAllParentMerchant;

public class GetAllParentMerchantQuery : SearchQueryParams, IRequest<PaginatedList<MerchantDto>>
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetAllParentMerchantQueryHandler : IRequestHandler<GetAllParentMerchantQuery, PaginatedList<MerchantDto>>
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly IMapper _mapper;

    public GetAllParentMerchantQueryHandler(IGenericRepository<Merchant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<MerchantDto>> Handle(GetAllParentMerchantQuery request, CancellationToken cancellationToken)
    {
        var merchantList = _repository.GetAll()
            .Include(b => b.Customer)
            .ThenInclude(c => c.AuthorizedPerson)
            .Include(b => b.MerchantBlockageList)
            .Include(m => m.MerchantUsers)
            .Include(m => m.MerchantBankAccounts)
            .Include(m => m.MerchantWallets)
            .AsQueryable();


        if (request.MainSubMerchantId is not null)
        {
            merchantList = merchantList
                    .Where(b => b.Id == request.MainSubMerchantId);
        }
        if (!string.IsNullOrEmpty(request.Q))
        {
            merchantList = merchantList.Where(b => b.Name.Contains(request.Q));
        }

        if (request.CreateDateStart is not null)
        {
            merchantList = merchantList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            merchantList = merchantList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.MerchantStatus is not null)
        {
            merchantList = merchantList
                    .Where(b => b.MerchantStatus == request.MerchantStatus);
        }

        if (request.IntegrationMode is not null)
        {
            var mode = request.IntegrationMode.ToString();
            merchantList = merchantList
                    .Where(b => ((string)(object)b.IntegrationMode).Contains(mode));
        }

        if (request.CityCode is not null)
        {
            merchantList = merchantList
                    .Where(b => b.Customer.City == request.CityCode);
        }

        if (request.MerchantType is not null)
        {
            merchantList = merchantList
                    .Where(b => b.MerchantType == request.MerchantType);
        }

        if (request.ParentMerchantIdList != null && request.ParentMerchantIdList.Any())
        {
            merchantList = merchantList
                    .Where(m => request.ParentMerchantIdList.Contains(m.ParentMerchantId.Value));
        }

        return await merchantList
            .PaginatedListWithMappingAsync<Merchant, MerchantDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
