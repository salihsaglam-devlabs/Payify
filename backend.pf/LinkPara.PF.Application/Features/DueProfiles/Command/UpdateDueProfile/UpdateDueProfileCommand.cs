using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.DueProfiles.Command.UpdateDueProfile
{
    public class UpdateDueProfileCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public TimeInterval OccurenceInterval { get; set; }
    }
    public class UpdateDueProfileCommandHandler : IRequestHandler<UpdateDueProfileCommand>
    {
        private readonly IDueProfileService _dueProfileService;

        public UpdateDueProfileCommandHandler(IDueProfileService dueProfileService)
        {
            _dueProfileService = dueProfileService;
        }
        public async Task<Unit> Handle(UpdateDueProfileCommand request, CancellationToken cancellationToken)
        {
            await _dueProfileService.UpdateAsync(request);
            return Unit.Value;
        }
    }

}
