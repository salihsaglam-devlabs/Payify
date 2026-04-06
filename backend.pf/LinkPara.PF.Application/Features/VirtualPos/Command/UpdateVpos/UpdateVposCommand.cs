using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;

public class UpdateVposCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public SecurityType SecurityType { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public List<SaveBankApiInfoDto> VposBankApiInfos { get; set; }
}

public class UpdateVposCommandHandler : IRequestHandler<UpdateVposCommand>
{
    private readonly IVposService _vposService;

    public UpdateVposCommandHandler(IVposService vposService)
    {
        _vposService = vposService;
    }

    public async Task<Unit> Handle(UpdateVposCommand request, CancellationToken cancellationToken)
    {
        await _vposService.UpdateAsync(request);

        return Unit.Value;
    }
}
