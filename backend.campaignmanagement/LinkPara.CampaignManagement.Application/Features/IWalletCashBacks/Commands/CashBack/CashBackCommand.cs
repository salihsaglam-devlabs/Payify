using LinkPara.CampaignManagement.Application.Commons.Attributes;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Models;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCashbacks.Commands.CashBack;

public class CashBackCommand : IRequest
{
    public string hash_data { get; set; }
    public string merchant_name { get; set; }
    public List<CashbackTransaction> sales_transactions { get; set; }
    public CashbackTransaction reward_transactions { get; set; }
}

public class CashBackCommandHandler : IRequestHandler<CashBackCommand>
{
    private readonly IIWalletCashbackService _service;

    public CashBackCommandHandler(IIWalletCashbackService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(CashBackCommand request, CancellationToken cancellationToken)
    {
        await _service.SaveCashBackTransactionAsync(request);

        return Unit.Value;
    }
}