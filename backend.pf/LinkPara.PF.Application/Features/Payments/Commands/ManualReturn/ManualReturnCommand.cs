using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;

public class ManualReturnCommand : IRequest
{
    public Guid MerchantTransactionId { get; set; }
    public decimal Amount { get; set; }
    public List<MerchantDocumentDto> Files { get; set; }
}

public class ManualReturnCommandHandler : IRequestHandler<ManualReturnCommand>
{
    private readonly IReturnService _returnService;

    public ManualReturnCommandHandler(IReturnService returnService)
    {
        _returnService = returnService;
    }

    public async Task<Unit> Handle(ManualReturnCommand request, CancellationToken cancellationToken)
    {
        await _returnService.ManualReturnAsync(request);
        return Unit.Value;
    }
}