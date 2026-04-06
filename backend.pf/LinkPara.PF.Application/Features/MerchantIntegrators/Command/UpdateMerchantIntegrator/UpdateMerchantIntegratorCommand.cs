using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.UpdateMerchantIntegrator;

public class UpdateMerchantIntegratorCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal CommissionRate { get; set; }
}

public class UpdateMerchantIntegratorHandler : IRequestHandler<UpdateMerchantIntegratorCommand>
{
    private readonly IMerchantIntegratorService _merchantIntegratorService;

    public UpdateMerchantIntegratorHandler(IMerchantIntegratorService merchantIntegratorService)
    {
        _merchantIntegratorService = merchantIntegratorService;
    }
    public async Task<Unit> Handle(UpdateMerchantIntegratorCommand request, CancellationToken cancellationToken)
    {
        await _merchantIntegratorService.UpdateAsync(request);

        return Unit.Value;
    }
}
