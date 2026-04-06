using MediatR;
using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LinkPara.PF.Application.Features.MerchantContents;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Features.LinkPayments.Queries.GetLinkByUrlPath;

public class GetLinkByUrlPathQuery : IRequest<LinkPaymentPageResponse>
{
    public string LinkCode { get; set; }
}

public class GetLinkByUrlPathQueryHandler : IRequestHandler<GetLinkByUrlPathQuery, LinkPaymentPageResponse>
{
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IGenericRepository<MerchantContent> _merchantContentRepository;
    private readonly IGenericRepository<MerchantLogo> _merchantLogoRepository;
    private readonly IMapper _mapper;
    public GetLinkByUrlPathQueryHandler(IGenericRepository<Link> linkRepository,
        IGenericRepository<MerchantContent> merchantContentRepository,
        IGenericRepository<MerchantLogo> merchantLogoRepository,
        IMapper mapper)
    {
        _linkRepository = linkRepository;
        _merchantContentRepository = merchantContentRepository; 
        _merchantLogoRepository = merchantLogoRepository;
        _mapper = mapper;
    }
    public async Task<LinkPaymentPageResponse> Handle(GetLinkByUrlPathQuery request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetAll()
            .Include(s => s.LinkInstallments)
            .FirstOrDefaultAsync(s => s.LinkCode == request.LinkCode
            && s.RecordStatus == RecordStatus.Active, cancellationToken);

        if (link is null)
        {
            throw new NotFoundException(nameof(link), request.LinkCode);
        }

        if (link.ExpiryDate < DateTime.Now)
        {
            link.UpdateDate = DateTime.Now;
            link.RecordStatus = RecordStatus.Passive;
            link.LinkStatus = ChannelStatus.Expired;

            await _linkRepository.UpdateAsync(link);

            throw new NotFoundException(nameof(link), request.LinkCode);
        }

        var linkContent = await _merchantContentRepository.GetAll()
            .Include(lc => 
                lc.Contents.Where(a => a.RecordStatus == RecordStatus.Active))
            .Where(x => 
                x.MerchantId == link.MerchantId && 
                x.RecordStatus == RecordStatus.Active && 
                x.ContentSource == MerchantContentSource.Link)
            .ToListAsync(cancellationToken);

        var linkLogo = await _merchantLogoRepository.GetAll()
            .Where(l =>
                l.RecordStatus == RecordStatus.Active &&
                l.MerchantId == link.MerchantId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var linkPaymentPageResponse = new LinkPaymentPageResponse
        {
            Link = _mapper.Map<LinkDto>(link),
            LinkContents = _mapper.Map<List<MerchantContentDto>>(linkContent),
            LinkLogo = _mapper.Map<MerchantLogoDto>(linkLogo),
        };
        return linkPaymentPageResponse;
    }
}
