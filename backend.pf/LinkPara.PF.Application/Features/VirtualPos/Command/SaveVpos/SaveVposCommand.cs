using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;

public class SaveVposCommand : IRequest
{
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public SecurityType SecurityType { get; set; }
    public VposType VposType { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public virtual List<SaveBankApiInfoDto> VposBankApiInfos { get; set; }
}

public class SaveVposCommandHandler : IRequestHandler<SaveVposCommand>
{
    private readonly IVposService _vposService;

    public SaveVposCommandHandler(IVposService vposService)
    {
        _vposService = vposService;
    }

    public async Task<Unit> Handle(SaveVposCommand request, CancellationToken cancellationToken)
    {
        await _vposService.SaveAsync(request);

        return Unit.Value;
    }
}
