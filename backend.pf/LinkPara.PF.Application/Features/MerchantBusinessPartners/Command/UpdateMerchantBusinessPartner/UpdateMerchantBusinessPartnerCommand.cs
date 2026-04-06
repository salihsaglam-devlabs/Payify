using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner
{
    public class UpdateMerchantBusinessPartnerCommand : IRequest, IMapFrom<MerchantBusinessPartner>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid MerchantId { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }

    public class UpdateMerchantBusinessPartnerCommandHandler : IRequestHandler<UpdateMerchantBusinessPartnerCommand>
    {
        private readonly IMerchantBusinessPartnerService _merchantBusinessPartnerService;

        public UpdateMerchantBusinessPartnerCommandHandler(IMerchantBusinessPartnerService merchantBusinessPartnerService)
        {
            _merchantBusinessPartnerService = merchantBusinessPartnerService;
        }

        public async Task<Unit> Handle(UpdateMerchantBusinessPartnerCommand request, CancellationToken cancellationToken)
        {
            await _merchantBusinessPartnerService.UpdateAsync(request);

            return Unit.Value;
        }
    }
}
