using AutoMapper;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.PF.Application.Features.MerchantPhysicalDevices;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetPhysicalPosStatusByReferenceNumber;

public class GetPhysicalPosStatusByReferenceNumberQuery : IRequest<MerchantPhysicalPosDto>
{
    public string BkmReferenceNumber { get; set; }
}

public class GetPhysicalPosStatusByReferenceNumberQueryHandler : IRequestHandler<GetPhysicalPosStatusByReferenceNumberQuery, MerchantPhysicalPosDto>
{
    private readonly IMapper _mapper;
    private readonly IIKSService _iKSService;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;
    private readonly ILogger<GetPhysicalPosStatusByReferenceNumberQuery> _logger;
    public GetPhysicalPosStatusByReferenceNumberQueryHandler(IMapper mapper,
        IIKSService iKSService,
        ILogger<GetPhysicalPosStatusByReferenceNumberQuery> logger,
        IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository)
    {
        _mapper = mapper;
        _iKSService = iKSService;
        _logger = logger;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
    }
    public async Task<MerchantPhysicalPosDto?> Handle(GetPhysicalPosStatusByReferenceNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var iksResponse = await _iKSService.GetTerminalStatusQueryAsync(
                new IKSGetTerminalStatusRequest
                {
                    ReferenceCode = request.BkmReferenceNumber
                });

            var merchantPhysicalPos = await _merchantPhysicalPosRepository.GetAll()
                .FirstOrDefaultAsync(b => b.BkmReferenceNumber == request.BkmReferenceNumber, cancellationToken);

            if (iksResponse.IsSuccess && iksResponse.Data is not null && (!string.IsNullOrWhiteSpace(iksResponse.Data.ResponseCode)
                 || !string.IsNullOrWhiteSpace(iksResponse.Data.StatusCode)
                 || !string.IsNullOrWhiteSpace(iksResponse.Data.TerminalId)))
            {
                if (merchantPhysicalPos is not null)
                {
                    if (!string.IsNullOrWhiteSpace(iksResponse.Data.TerminalId))
                    {
                        merchantPhysicalPos.PosTerminalId = iksResponse.Data.TerminalId;
                        merchantPhysicalPos.PosMerchantId = iksResponse.Data.ServiceProviderPspMerchantId;
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Active;
                    }

                    if (iksResponse.Data.ResponseCode != null && iksResponse.Data.ResponseCode != "00" && iksResponse.Data.ResponseCode != "200")
                    {
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Reject;
                        merchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                    }

                    if (iksResponse.Data.StatusCode != "0")
                    {
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Passive;
                        merchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                    }

                    await _merchantPhysicalPosRepository.UpdateAsync(merchantPhysicalPos);
                }
                else
                {
                    _logger.LogWarning("MerchantPhysicalPos is not found. ReferenceNumber: {ReferenceNumber}", request.BkmReferenceNumber);
                }
            }
            else
            {
                _logger.LogError("IKS Physical Terminal query failed. ReferenceNumber: {ReferenceNumber}, Error: {Error}",
                    request.BkmReferenceNumber, iksResponse.Error);
            }

            return merchantPhysicalPos is not null
                ? _mapper.Map<MerchantPhysicalPosDto>(merchantPhysicalPos)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPhysicalPosStatusByReferenceNumberQuery failed. ReferenceNumber: {ReferenceNumber}",
                request.BkmReferenceNumber);
            return null;
        }
    }
}
