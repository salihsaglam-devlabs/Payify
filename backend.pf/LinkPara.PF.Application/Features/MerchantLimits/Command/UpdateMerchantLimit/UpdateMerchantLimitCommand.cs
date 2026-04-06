using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit
{
    public class UpdateMerchantLimitCommand : IRequest, IMapFrom<MerchantLimit>
    {
        public Guid Id { get; set; }
        public TransactionLimitType TransactionLimitType { get; set; }
        public Period Period { get; set; }
        public LimitType LimitType { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public int? MaxPiece { get; set; }
        public decimal? MaxAmount { get; set; }
        public Guid MerchantId { get; set; }
        public string Currency { get; set; }
    }
    public class UpdateMerchantLimitCommandHandler : IRequestHandler<UpdateMerchantLimitCommand>
    {
        private readonly IMerchantLimitService _merchantLimitService;

        public UpdateMerchantLimitCommandHandler(IMerchantLimitService merchantLimitService)
        {
            _merchantLimitService = merchantLimitService;
        }
        public async Task<Unit> Handle(UpdateMerchantLimitCommand request, CancellationToken cancellationToken)
        {
            await _merchantLimitService.UpdateAsync(request);

            return Unit.Value;
        }
    }
}
