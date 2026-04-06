using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;

public class UpdateMccCommand : IRequest, IMapFrom<Mcc>
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxIndividualInstallmentCount { get; set; }
    public int MaxCorporateInstallmentCount { get; set; }
    public string Description { get; set; }
}

public class UpdateMccCommandHandler : IRequestHandler<UpdateMccCommand>
{
    private readonly IMccService _mccService;

    public UpdateMccCommandHandler(IMccService mccService)
    {
        _mccService = mccService;
    }
    public async Task<Unit> Handle(UpdateMccCommand request, CancellationToken cancellationToken)
    {
        await _mccService.UpdateAsync(request);

        return Unit.Value;  
    }
}