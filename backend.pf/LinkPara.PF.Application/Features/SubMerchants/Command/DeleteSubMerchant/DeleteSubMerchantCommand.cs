using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.DeleteSubMerchant;

public class DeleteSubMerchantCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSubMerchantCommandHandler : IRequestHandler<DeleteSubMerchantCommand>
{
    private readonly ISubMerchantService _subMerchantService;

    public DeleteSubMerchantCommandHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }

    public async Task<Unit> Handle(DeleteSubMerchantCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantService.DeleteAsync(request);

        return Unit.Value;
    }
}