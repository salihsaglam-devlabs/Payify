using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantContents;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppDetailsByTrackingId;

public class GetHppDetailsByTrackingIdQuery : IRequest<HostedPaymentPageResponse>
{
    public string TrackingId { get; set; }
}

public class GetHppDetailsByTrackingIdQueryHandler : IRequestHandler<GetHppDetailsByTrackingIdQuery, HostedPaymentPageResponse>
{
    private readonly IGenericRepository<HostedPayment> _hostedPaymentRepository;
    private readonly IGenericRepository<MerchantLogo> _merchantLogoRepository;
    private readonly IGenericRepository<MerchantContent> _merchantContentRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;
    private readonly IHostedPaymentService _hostedPaymentService;

    public GetHppDetailsByTrackingIdQueryHandler(
        IGenericRepository<HostedPayment> hostedPaymentRepository,
        IGenericRepository<MerchantLogo> merchantLogoRepository,
        IGenericRepository<MerchantContent> merchantContentRepository,
        IApplicationUserService applicationUserService,
        IMapper mapper,
        IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentRepository = hostedPaymentRepository;
        _merchantLogoRepository = merchantLogoRepository;
        _merchantContentRepository = merchantContentRepository;
        _applicationUserService = applicationUserService;
        _mapper = mapper;
        _hostedPaymentService = hostedPaymentService;
    }

    public async Task<HostedPaymentPageResponse> Handle(GetHppDetailsByTrackingIdQuery request,
        CancellationToken cancellationToken)
    {
        var hpp = await _hostedPaymentRepository.GetAll()
            .Include(s => s.Installments)
            .FirstOrDefaultAsync(s => s.TrackingId == request.TrackingId
                                      && s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (hpp is null)
        {
            throw new NotFoundException(nameof(hpp), request.TrackingId);
        }

        if (hpp.ExpiryDate < DateTime.Now)
        {
            hpp.UpdateDate = DateTime.Now;
            hpp.RecordStatus = RecordStatus.Passive;
            hpp.HppStatus = ChannelStatus.Expired;
            hpp.HppPaymentStatus = ChannelPaymentStatus.Expired;
            hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            
            await _hostedPaymentRepository.UpdateAsync(hpp);

            await _hostedPaymentService.TriggerHppWebhookAsync(hpp.TrackingId);

            throw new NotFoundException(nameof(hpp), request.TrackingId);
        }

        var merchantLogo = await _merchantLogoRepository.GetAll()
            .Where(l =>
                l.RecordStatus == RecordStatus.Active &&
                l.MerchantId == hpp.MerchantId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        var merchantContent = await _merchantContentRepository.GetAll()
            .Include(lc => 
                lc.Contents.Where(a => a.RecordStatus == RecordStatus.Active))
            .Where(x => 
                x.MerchantId == hpp.MerchantId && 
                x.RecordStatus == RecordStatus.Active && 
                x.ContentSource == MerchantContentSource.Hpp)
            .ToListAsync(cancellationToken: cancellationToken);

        var hostedPaymentPageResponse = new HostedPaymentPageResponse
        {
            HostedPayment = _mapper.Map<HostedPaymentDto>(hpp),
            MerchantLogo = _mapper.Map<MerchantLogoDto>(merchantLogo),
            MerchantContents = _mapper.Map<List<MerchantContentDto>>(merchantContent)
        };
        
        return hostedPaymentPageResponse;
    }
}