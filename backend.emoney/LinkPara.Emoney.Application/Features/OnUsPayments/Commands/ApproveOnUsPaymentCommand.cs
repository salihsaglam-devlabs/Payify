using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OnUsPayments.Commands;

public class ApproveOnUsPaymentCommand : IRequest<ProvisionPreviewResponse>
{    
    public Guid OnUsPaymentRequestId { get; set; }
    public string SenderWalletNumber { get; set; }
    public bool IsVerifiedByUser { get; set; }
}

public class ApproveOnUsPaymentCommandHandler : IRequestHandler<ApproveOnUsPaymentCommand, ProvisionPreviewResponse>
{
    private readonly IOnUsPaymentService _onUsPaymentService;

    public ApproveOnUsPaymentCommandHandler(IOnUsPaymentService onUsPaymentService)
    {
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<ProvisionPreviewResponse> Handle(ApproveOnUsPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.ApproveOnUsPaymentAsync(request);        
    }
}
