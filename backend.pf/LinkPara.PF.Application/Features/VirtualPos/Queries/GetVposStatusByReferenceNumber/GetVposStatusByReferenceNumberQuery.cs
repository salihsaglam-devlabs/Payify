using AutoMapper;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.IKS.Models.Response;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LinkPara.PF.Application.Features.VirtualPos.Queries.GetVposStatusByReferenceNumber;

public class GetVposStatusByReferenceNumberQuery : IRequest<MerchantVposDto>
{
    public string BkmReferenceNumber { get; set; }
}

public class GetVposStatusByReferenceNumberQueryHandler : IRequestHandler<GetVposStatusByReferenceNumberQuery, MerchantVposDto>
{
    private readonly IMapper _mapper;
    private readonly IIKSService _iKSService;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly ILogger<GetVposStatusByReferenceNumberQuery> _logger;
    public GetVposStatusByReferenceNumberQueryHandler(IMapper mapper,
        IIKSService iKSService,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        ILogger<GetVposStatusByReferenceNumberQuery> logger)
    {
        _mapper = mapper;
        _iKSService = iKSService;
        _merchantVposRepository = merchantVposRepository;
        _logger = logger;
    }
    public async Task<MerchantVposDto?> Handle(GetVposStatusByReferenceNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var iksResponse = await _iKSService.GetTerminalStatusQueryAsync(
                new IKSGetTerminalStatusRequest
                {
                    ReferenceCode = request.BkmReferenceNumber
                });

            var merchantVpos = await _merchantVposRepository.GetAll()
                .FirstOrDefaultAsync(b => b.BkmReferenceNumber == request.BkmReferenceNumber, cancellationToken);

            if (iksResponse.IsSuccess && iksResponse.Data is not null && (!string.IsNullOrWhiteSpace(iksResponse.Data.ResponseCode)
                 || !string.IsNullOrWhiteSpace(iksResponse.Data.StatusCode)
                 || !string.IsNullOrWhiteSpace(iksResponse.Data.TerminalId)))
            {
                if (merchantVpos is not null)   
                {
                    if (!string.IsNullOrWhiteSpace(iksResponse.Data.TerminalId))
                    {
                        merchantVpos.SubMerchantCode = iksResponse.Data.TerminalId;
                        merchantVpos.ServiceProviderPspMerchantId = iksResponse.Data.ServiceProviderPspMerchantId;
                        merchantVpos.TerminalStatus = TerminalStatus.Active;
                    }

                    if (iksResponse.Data.ResponseCode != null && iksResponse.Data.ResponseCode != "00" && iksResponse.Data.ResponseCode != "200")
                    {
                        merchantVpos.TerminalStatus = TerminalStatus.Reject;
                        merchantVpos.RecordStatus = RecordStatus.Passive;
                    }

                    if (iksResponse.Data.StatusCode != "0")
                    {
                        merchantVpos.TerminalStatus = TerminalStatus.Passive;
                        merchantVpos.RecordStatus = RecordStatus.Passive;
                    }

                    await _merchantVposRepository.UpdateAsync(merchantVpos);
                }
                else
                {
                    _logger.LogWarning("MerchantVpos is not found. ReferenceNumber: {ReferenceNumber}", request.BkmReferenceNumber);
                }
            }
            else
            {
                _logger.LogError("IKS Terminal query failed. ReferenceNumber: {ReferenceNumber}, Error: {Error}",
                    request.BkmReferenceNumber, iksResponse.Error);
            }

            return merchantVpos is not null
                ? _mapper.Map<MerchantVposDto>(merchantVpos)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetVposStatusByReferenceNumberQuery failed. ReferenceNumber: {ReferenceNumber}",
                request.BkmReferenceNumber);
            return null;
        }
    }
}

