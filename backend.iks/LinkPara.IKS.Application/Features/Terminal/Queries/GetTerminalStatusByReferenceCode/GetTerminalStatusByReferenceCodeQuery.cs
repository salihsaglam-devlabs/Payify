using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LinkPara.IKS.Application.Features.Terminal.Queries.GetTerminalStatusByReferenceCode;

public class GetTerminalStatusByReferenceCodeQuery : IRequest<IKSResponse<TerminalResponse>>
{
    public string ReferenceCode { get; set; }
}

public class GetTerminalStatusByReferenceCodeQueryHandler : IRequestHandler<GetTerminalStatusByReferenceCodeQuery, IKSResponse<TerminalResponse>>
{
    private readonly IIKSService _iksService;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<IksTerminal> _iksTerminalRepository;
    private readonly IGenericRepository<IksTerminalHistory> _iksTerminalHistoryRepository;
    private readonly IContextProvider _contextProvider;

    public GetTerminalStatusByReferenceCodeQueryHandler(IIKSService iksService,
        IMapper mapper,
        IGenericRepository<IksTerminal> iksTerminalRepository,
        IGenericRepository<IksTerminalHistory> iksTerminalHistoryRepository,
        IContextProvider contextProvider)
    {
        _iksService = iksService;
        _mapper = mapper;
        _iksTerminalRepository = iksTerminalRepository;
        _iksTerminalHistoryRepository = iksTerminalHistoryRepository;
        _contextProvider = contextProvider;
    }

    public async Task<IKSResponse<TerminalResponse>> Handle(GetTerminalStatusByReferenceCodeQuery request, CancellationToken cancellationToken)
    {
        var iksTerminal = await _iksTerminalRepository.GetAll()
            .Where(s => s.ReferenceCode == request.ReferenceCode && s.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (iksTerminal is null)
        {
            throw new NotFoundException(nameof(IksTerminal), request.ReferenceCode);
        }

        var oldTerminalId = iksTerminal.TerminalId;
        var oldStatusCode = iksTerminal.StatusCode;
        var oldResponseCode = iksTerminal.ResponseCode;

        var result = await _iksService.GetTerminalStatusQueryAsync(new GetTerminalStatusRequest{ReferenceCode = request.ReferenceCode});
        var data = result?.Data?.terminals.FirstOrDefault();

        if (data is not null)
        {
            iksTerminal = _iksService.UpdateIksTerminalFields(iksTerminal, data);
            await _iksTerminalRepository.UpdateAsync(iksTerminal);
                       

            if (data.terminalId != oldTerminalId || data.statusCode != oldStatusCode)
            {
                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                var terminalHistories = new List<IksTerminalHistory>();

                if (data.terminalId != oldTerminalId)
                {
                    terminalHistories.Add(new IksTerminalHistory
                    {
                        MerchantId = iksTerminal.MerchantId,
                        VposId = iksTerminal.VposId,
                        PhysicalPosId = iksTerminal.PhysicalPosId,
                        OldData = oldTerminalId ?? string.Empty,
                        NewData = data.terminalId ?? string.Empty,
                        ChangedField = "TerminalId",
                        ReferenceCode = data.referenceCode,
                        ResponseCode = data.responseCode,
                        ResponseCodeExplanation = data.responseCodeExplanation,
                        CreatedBy = parseUserId.ToString(),
                        QueryDate= DateTime.Now,
                        TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                    });
                }

                if (data.statusCode != oldStatusCode)
                {
                    terminalHistories.Add(new IksTerminalHistory
                    {
                        MerchantId = iksTerminal.MerchantId,
                        VposId = iksTerminal.VposId,
                        PhysicalPosId = iksTerminal.PhysicalPosId,
                        OldData = oldStatusCode?.ToString() ?? string.Empty,
                        NewData = data.statusCode?.ToString() ?? string.Empty,
                        ChangedField = "StatusCode",
                        ReferenceCode = data.referenceCode,
                        ResponseCode = data.responseCode,
                        ResponseCodeExplanation = data.responseCodeExplanation,
                        CreatedBy = parseUserId.ToString(),
                        QueryDate = DateTime.Now,
                        TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                    });
                }

                if (data.responseCode != oldResponseCode)
                {
                    terminalHistories.Add(new IksTerminalHistory
                    {
                        MerchantId = iksTerminal.MerchantId,
                        VposId = iksTerminal.VposId,
                        PhysicalPosId = iksTerminal.PhysicalPosId,
                        OldData = oldResponseCode?.ToString() ?? string.Empty,
                        NewData = data.responseCode?.ToString() ?? string.Empty,
                        ChangedField = "ResponseCode",
                        ReferenceCode = data.referenceCode,
                        ResponseCode = data.responseCode,
                        ResponseCodeExplanation = data.responseCodeExplanation,
                        CreatedBy = parseUserId.ToString(),
                        QueryDate = DateTime.Now,
                        TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                    });
                }

                if (terminalHistories.Any())
                {
                    await _iksTerminalHistoryRepository.AddRangeAsync(terminalHistories);
                }
            }
        }

        return new IKSResponse<TerminalResponse>
        {
            Error = result?.Error,
            Data = new TerminalResponse
            {
                GlobalMerchantId = iksTerminal.GlobalMerchantId,
                PspMerchantId = iksTerminal.PspMerchantId,
                TerminalId = iksTerminal.TerminalId,
                ReferenceCode = iksTerminal.ReferenceCode,
                StatusCode = iksTerminal.StatusCode,
                ResponseCode = iksTerminal.ResponseCode,
                ResponseCodeExplanation = iksTerminal.ResponseCodeExplanation,
                ServiceProviderPspMerchantId = iksTerminal.ServiceProviderPspMerchantId
            }
        };
    }
}