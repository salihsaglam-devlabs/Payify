using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantLimits.Commands.SaveSubMerchantLimit;

public class SaveSubMerchantLimitCommand : IRequest, IMapFrom<SubMerchantLimit>
{
    public TransactionLimitType TransactionLimitType { get; set; }
    public Period Period { get; set; }
    public LimitType LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; }
    public Guid SubMerchantId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class SaveSubMerchantLimitCommandHandler : IRequestHandler<SaveSubMerchantLimitCommand>
{
    private readonly ISubMerchantLimitService _subMerchantLimitService;

    public SaveSubMerchantLimitCommandHandler(ISubMerchantLimitService subMerchantLimitService)
    {
        _subMerchantLimitService = subMerchantLimitService;
    }

    public async Task<Unit> Handle(SaveSubMerchantLimitCommand request, CancellationToken cancellationToken)
    {
       await _subMerchantLimitService.SaveAsync(request);
       return Unit.Value;
    }
}