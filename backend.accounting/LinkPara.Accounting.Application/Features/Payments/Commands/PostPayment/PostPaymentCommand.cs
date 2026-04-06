using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;

public class PostPaymentCommand : IRequest
{
    public string ReferenceId { get; set; }
    public OperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public int BankCode { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
}

public class PostPaymentCommandHandler : IRequestHandler<PostPaymentCommand>
{
    private readonly IPaymentService _paymentService;

    public PostPaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Unit> Handle(PostPaymentCommand request, CancellationToken cancellationToken)
    {
        await _paymentService.PostPaymentAsync(request);
        
        return Unit.Value;
    }
}