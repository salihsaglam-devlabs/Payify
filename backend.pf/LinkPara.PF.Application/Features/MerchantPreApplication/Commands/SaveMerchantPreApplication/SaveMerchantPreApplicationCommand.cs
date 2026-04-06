using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.SaveMerchantPreApplication;

public class SaveMerchantPreApplicationCommand : IRequest<MerchantPreApplicationCreateResponse>, IMapFrom<Domain.Entities.MerchantPreApplication>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public string Website { get; set; }
    public bool ConsentConfirmation { get; set; }
    public bool KvkkConfirmation { get; set; }
}

public class CreatePendingPosApplicationCommandHandler : IRequestHandler<SaveMerchantPreApplicationCommand, MerchantPreApplicationCreateResponse>
{
    private readonly IMerchantPreApplicationService _merchantPreApplicationService;

    public CreatePendingPosApplicationCommandHandler(IMerchantPreApplicationService merchantPreApplicationService)
    {
        _merchantPreApplicationService = merchantPreApplicationService;
    }

    public async Task<MerchantPreApplicationCreateResponse> Handle(SaveMerchantPreApplicationCommand request, CancellationToken cancellationToken)
    {
       return await _merchantPreApplicationService.SaveAsync(request);
    }
}

