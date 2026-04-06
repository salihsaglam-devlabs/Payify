using MediatR;
using LinkPara.Emoney.Application.Commons.Interfaces;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Commands.ApproveCompanyPool;

public class ApproveCompanyPoolCommand : IRequest
{
    public Guid CompanyPoolId { get; set; }
    public bool IsApprove { get; set; }
    public string RejectReason { get; set; }
    public Guid UserId { get; set; }
}

public class ApproveCompanyPoolCommandHandler : IRequestHandler<ApproveCompanyPoolCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public ApproveCompanyPoolCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(ApproveCompanyPoolCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.ActionCompanyPoolAsync(request);
        return Unit.Value;
    }
}