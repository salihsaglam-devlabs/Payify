using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateBalance;
public class UpdateBalanceCommand : IRequest<UpdateBalanceResponse>
{
    public TransactionType TransactionType { get; set; }
    public TransactionDirection TransactionDirection { get; set; }
    public string Channel { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Utid { get; set; }
    public string RefUtid { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public bool IsBalanceControl { get; set; }
}
public class UpdateBalanceCommandHandler : IRequestHandler<UpdateBalanceCommand, UpdateBalanceResponse>
{
    private readonly IUpdateBalanceService _balanceService;
    private readonly IContextProvider _contextProvider;
    public UpdateBalanceCommandHandler(
        IUpdateBalanceService balanceService,
        IContextProvider contextProvider)
    {
        _balanceService = balanceService;
        _contextProvider = contextProvider;
    }
    public async Task<UpdateBalanceResponse> Handle(UpdateBalanceCommand command, CancellationToken cancellationToken)
    {
        if (_contextProvider.CurrentContext.Gateway != Gateway.BoaApiGateway.ToString())
        {
            throw new ForbiddenAccessException();
        }
        switch (command.TransactionType)
        {
            case TransactionType.Sale:
            case TransactionType.Withdraw:
                {
                    return await _balanceService.MoneyOutAsync(command);
                }
            case TransactionType.Deposit:
                {
                    return await _balanceService.MoneyInAsync(command);
                }
            case TransactionType.Return:
            case TransactionType.Cancel:
                {
                    return await _balanceService.ReturnAsync(command);
                }
            case TransactionType.Maintenance:
                {
                    return await _balanceService.MaintenanceAsync(command);
                }
            default:
                throw new InvalidOperationException($"Unexpected TransactionType : {command.TransactionType}");
        }
    }
}
