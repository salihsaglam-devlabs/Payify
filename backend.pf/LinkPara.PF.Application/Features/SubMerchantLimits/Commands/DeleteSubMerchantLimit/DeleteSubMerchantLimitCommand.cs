using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantLimits.Commands.DeleteSubMerchantLimit;

public class DeleteSubMerchantLimitCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSubMerchantCommandHandler : IRequestHandler<DeleteSubMerchantLimitCommand>
{
    private readonly ISubMerchantLimitService _subMerchantLimitService;

    public DeleteSubMerchantCommandHandler(ISubMerchantLimitService subMerchantLimitService)
    {
        _subMerchantLimitService = subMerchantLimitService;
    }

    public async Task<Unit> Handle(DeleteSubMerchantLimitCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantLimitService.DeleteAsync(request.Id);
        return Unit.Value;
    }
}