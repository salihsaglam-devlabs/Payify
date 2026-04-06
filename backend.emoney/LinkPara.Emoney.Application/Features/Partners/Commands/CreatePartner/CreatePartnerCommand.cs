using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Partners.Commands.CreatePartner;

public class CreatePartnerCommand : IRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
}

public class CreatePartnerCommandHandler : IRequestHandler<CreatePartnerCommand>
{
    private readonly IPartnerService _partnerService;

    public CreatePartnerCommandHandler(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    public async Task<Unit> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        await _partnerService.CreatePartnerAsync(request);

        return Unit.Value;
    }
}
