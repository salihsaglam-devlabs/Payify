using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit
{
    public class SaveMerchantLimitCommand : IRequest, IMapFrom<MerchantLimit>
    {
        public TransactionLimitType TransactionLimitType { get; set; }
        public Period Period { get; set; }
        public LimitType LimitType { get; set; }
        public int? MaxPiece { get; set; }
        public decimal? MaxAmount { get; set; }
        public Guid MerchantId { get; set; }
        public string Currency { get; set; }
    }

    public class SaveMerchantLimitCommandHandler : IRequestHandler<SaveMerchantLimitCommand>
    {
        private readonly IMerchantLimitService _merchantLimitService;

        public SaveMerchantLimitCommandHandler(IMerchantLimitService merchantLimitService)
        {
            _merchantLimitService = merchantLimitService;
        }
        public async Task<Unit> Handle(SaveMerchantLimitCommand request, CancellationToken cancellationToken)
        {
            await _merchantLimitService.SaveAsync(request);

            return Unit.Value;
        }
    }
}
