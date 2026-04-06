using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetAllMerchantHistory;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.PF.Application.Features.MerchantHistory;
using LinkPara.PF.Application.Features.MerchantHistory.Queries.GetFilterMerchantHistory;

namespace LinkPara.PF.Infrastructure.Services
{
    public class MerchantHistoryService : IMerchantHistoryService
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<MerchantHistory> _repository;

        public MerchantHistoryService(IMapper mapper,
            IGenericRepository<MerchantHistory> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<PaginatedList<MerchantHistoryDto>> GetAllMerchantHistoryAsync(GetAllMerchantHistoryQuery request)
        {
            return await _repository.GetAll().Include(x=>x.Merchant).Where(x => x.RecordStatus == RecordStatus.Active)
                .PaginatedListWithMappingAsync<MerchantHistory,MerchantHistoryDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }
        public async Task<PaginatedList<MerchantHistoryDto>> GetFilterListAsync(GetFilterMerchantHistoryQuery request)
        {
            var merchantHistoryList = _repository.GetAll().Include(b => b.Merchant).AsQueryable();

            if (!string.IsNullOrEmpty(request.MerchantName))
            {
                merchantHistoryList = merchantHistoryList.Where(b => b.Merchant.Name.Contains(request.MerchantName));
            }

            if (request.PermissionOperationType is not null)
            {
                merchantHistoryList = merchantHistoryList
                        .Where(b => b.PermissionOperationType == request.PermissionOperationType);
            }

            if (request.CreateDateStart is not null)
            {
                merchantHistoryList = merchantHistoryList.Where(b => b.CreateDate
                                   >= request.CreateDateStart);
            }

            if (request.CreateDateEnd is not null)
            {
                merchantHistoryList = merchantHistoryList.Where(b => b.CreateDate
                                   <= request.CreateDateEnd);
            }

            if (request.CreatedNameBy is not null)
            {
                merchantHistoryList = merchantHistoryList.Where(b => b.CreatedNameBy.ToLower().Contains(request.CreatedNameBy.ToLower()));
            }

            return await merchantHistoryList
               .PaginatedListWithMappingAsync<MerchantHistory,MerchantHistoryDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }
    }
}
