using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.RepresentativeTransactions;

public class UpdateRepresentativeTransactionRequest
{
    public Guid Id { get; set; }
    public RepresentativeTransactionStatus RepresentativeTransactionStatus { get; set; }
}