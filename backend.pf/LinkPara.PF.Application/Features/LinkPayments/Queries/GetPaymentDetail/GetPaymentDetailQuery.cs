using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.LinkPayments.Queries.GetLinkByUrlPath;
using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.MerchantContents;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.LinkPayments.Queries.GetPaymentDetail;

public class GetPaymentDetailQuery : SearchQueryParams, IRequest<PaginatedList<LinkPaymentDetailResponse>>
{
    public string LinkCode { get; set; }
}

public class GetPaymentDetailQueryHandler : IRequestHandler<GetPaymentDetailQuery, PaginatedList<LinkPaymentDetailResponse>>
{
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly ILinkPaymentService _linkPaymentService;
    private readonly IRestrictionService _restrictionService;
    public GetPaymentDetailQueryHandler(IGenericRepository<Link> linkRepository,
        ILinkPaymentService linkPaymentService,
        IRestrictionService restrictionService)
    {
        _linkRepository = linkRepository;
        _linkPaymentService = linkPaymentService;
        _restrictionService = restrictionService;
    }
    public async Task<PaginatedList<LinkPaymentDetailResponse>> Handle(GetPaymentDetailQuery request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetAll()
            .FirstOrDefaultAsync(s => s.LinkCode == request.LinkCode, cancellationToken);

        if (link is null)
        {
            throw new NotFoundException(nameof(link), request.LinkCode);
        }

        await _restrictionService.IsUserAuthorizedAsync(link.MerchantId);
              
        return await _linkPaymentService.GetLinkPaymentDetails(request);
    }
}
