using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.DueProfiles.Queries.GetFilterDueProfile
{
    public class GetFilterDueProfileQuery : SearchQueryParams, IRequest<PaginatedList<DueProfileDto>>
    {
        public string Title { get; set; }
        public DueType? DueType { get; set; }
        public decimal? AmountBiggerThan { get; set; }
        public decimal? AmountSmallerThan { get; set; }
        public int? Currency { get; set; }
        public TimeInterval? OccurenceInterval { get; set; }
        public bool? IsDefault { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
    public class GetFilterDueProfileQueryHandler : IRequestHandler<GetFilterDueProfileQuery, PaginatedList<DueProfileDto>>
    {
        private readonly IDueProfileService _dueProfileService;

        public GetFilterDueProfileQueryHandler(IDueProfileService dueProfileService)
        {
            _dueProfileService = dueProfileService;
        }
        public async Task<PaginatedList<DueProfileDto>> Handle(GetFilterDueProfileQuery request, CancellationToken cancellationToken)
        {
            return await _dueProfileService.GetFilterListAsync(request);
        }
    }
}
