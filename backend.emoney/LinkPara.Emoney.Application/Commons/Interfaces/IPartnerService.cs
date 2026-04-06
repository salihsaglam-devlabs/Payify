using LinkPara.Emoney.Application.Features.Partners.Commands.CreatePartner;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPartnerService
{
    Task CreatePartnerAsync(CreatePartnerCommand request);
}
