using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantById;
using LinkPara.PF.Application.Features.Merchants;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkPara.PF.Application.Commons.Interfaces;

namespace LinkPara.PF.Application.Features.DueProfiles.Queries.GetDueProfileById
{
    public class GetDueProfileByIdQuery : IRequest<DueProfileDto>
    {
        public Guid Id { get; set; }
    }

    public class GetDueProfileByIdQueryHandler : IRequestHandler<GetDueProfileByIdQuery, DueProfileDto>
    {
        private readonly IDueProfileService _dueProfileService;

        public GetDueProfileByIdQueryHandler(IDueProfileService dueProfileService)
        {
            _dueProfileService = dueProfileService;
        }
        public async Task<DueProfileDto> Handle(GetDueProfileByIdQuery request, CancellationToken cancellationToken)
        {
            return await _dueProfileService.GetByIdAsync(request.Id);
        }
    }
}
