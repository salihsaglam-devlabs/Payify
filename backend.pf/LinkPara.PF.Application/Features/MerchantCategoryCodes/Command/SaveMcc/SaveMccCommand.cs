using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;

public class SaveMccCommand :IRequest, IMapFrom<Mcc>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxIndividualInstallmentCount { get; set; }
    public int MaxCorporateInstallmentCount { get; set; }
    public string Description { get; set; }
}

public class SaveMccCommandHandler : IRequestHandler<SaveMccCommand>
{
    private readonly IMccService _mccService;

    public SaveMccCommandHandler(IMccService mccService)
    {
        _mccService = mccService;
    }

    public async Task<Unit> Handle(SaveMccCommand request, CancellationToken cancellationToken)
    {
        await _mccService.SaveAsync(request);

        return Unit.Value;  
    }
}
