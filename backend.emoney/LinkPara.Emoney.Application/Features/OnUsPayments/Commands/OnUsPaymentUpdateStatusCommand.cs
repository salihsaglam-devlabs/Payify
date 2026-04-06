using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.OnUsPayments.Commands;

public class OnUsPaymentUpdateStatusCommand : IRequest
{
    public Guid OnUsPaymentRequestId { get; set; }
    public OnUsPaymentStatus Status { get; set; }
}

public class OnUsPaymentUpdateStatusCommandHandler : IRequestHandler<OnUsPaymentUpdateStatusCommand>
{
    private readonly IOnUsPaymentService _onUsPaymentService;

    public OnUsPaymentUpdateStatusCommandHandler(IOnUsPaymentService onUsPaymentService)
    {
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<Unit> Handle(OnUsPaymentUpdateStatusCommand request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.OnUsPaymentUpdateStatusAsync(request);        
    }
}
