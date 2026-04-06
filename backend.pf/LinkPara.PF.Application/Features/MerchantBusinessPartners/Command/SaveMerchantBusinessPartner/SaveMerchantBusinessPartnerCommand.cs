using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.SaveMerchantBusinessPartner
{
    public class SaveMerchantBusinessPartnerCommand : IRequest, IMapFrom<MerchantBusinessPartner>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid MerchantId { get; set; }
    }
    public class SaveMerchantBusinessPartnerCommandHandler : IRequestHandler<SaveMerchantBusinessPartnerCommand>
    {
        private readonly IMerchantBusinessPartnerService _merchantBusinessPartnerService;

        public SaveMerchantBusinessPartnerCommandHandler(IMerchantBusinessPartnerService merchantBusinessPartnerService)
        {
            _merchantBusinessPartnerService = merchantBusinessPartnerService;
        }
        public async Task<Unit> Handle(SaveMerchantBusinessPartnerCommand request, CancellationToken cancellationToken)
        {
            await _merchantBusinessPartnerService.SaveAsync(request);

            return Unit.Value;
        }
    }
}