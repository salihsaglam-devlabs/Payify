using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.DeleteMerchantIntegrator;

public class DeleteMerchantIntegratorCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteMerchantIntegratorCommandHandler : IRequestHandler<DeleteMerchantIntegratorCommand>
{
    private readonly IMerchantIntegratorService _merchantIntegratorService;

    public DeleteMerchantIntegratorCommandHandler(IMerchantIntegratorService merchantIntegratorService)
    {
        _merchantIntegratorService = merchantIntegratorService;
    }
    public async Task<Unit> Handle(DeleteMerchantIntegratorCommand request, CancellationToken cancellationToken)
    {
        await _merchantIntegratorService.DeleteAsync(request);

        return Unit.Value;
    }
}   
