using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupCancel;

public class TopupCancelCommand : IRequest<TopupCancelResponse>
{
    public TopupCancelBaseRequest BaseRequest { get; set; }
}

public class TopupCancelCommandHandler : IRequestHandler<TopupCancelCommand, TopupCancelResponse>
{
    private readonly ITopupService _topupService;
    private readonly IWalletService _walletService;
    private readonly IPaymentProviderServiceFactory _paymentProviderServiceFactory;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly string _paymentProviderType;
    private readonly IMasterpassService _masterpassService;

    public TopupCancelCommandHandler(ITopupService topupService,
        IWalletService walletService,
        IPaymentProviderServiceFactory paymentProviderServiceFactory,
        IVaultClient vaultClient,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IGenericRepository<Wallet> walletRepository,
        IMasterpassService masterpassService)
    {
        _topupService = topupService;
        _walletService = walletService;
        _paymentProviderServiceFactory = paymentProviderServiceFactory;
        _paymentProviderType = vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _walletRepository = walletRepository;
        _masterpassService = masterpassService;
    }

    public async Task<TopupCancelResponse> Handle(TopupCancelCommand request, CancellationToken cancellationToken)
    {
        var cardTopupRequest = await _cardTopupRequestRepository.GetByIdAsync(request.BaseRequest.CardTopupRequestId);
        
        if (cardTopupRequest == null)
        {
            throw new NotFoundException(nameof(CardTopupRequest), cardTopupRequest);
        }

        var amount = Math.Round(cardTopupRequest.Amount + cardTopupRequest.Fee + cardTopupRequest.CommissionTotal + cardTopupRequest.BsmvTotal, 2);

        bool isCredit = cardTopupRequest.CardType == CardType.Credit || cardTopupRequest.CardType == CardType.Unknown;

        var isBalanceSufficient = await _walletService.IsBalanceSufficientAsync(cardTopupRequest.WalletNumber, amount, isCredit);

        if (!isBalanceSufficient)
        {
            throw new InsufficientBalanceException();
        }

        var wallet = await _walletRepository
                         .GetAll()
                         .Include(w => w.Account)
                         .ThenInclude(w => w.AccountUsers)
                         .Where(w => w.WalletNumber.Equals(cardTopupRequest.WalletNumber) && w.RecordStatus == RecordStatus.Active)
                         .FirstOrDefaultAsync(cancellationToken);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Wallet), wallet);
        }

        var paymentProvider = await _paymentProviderServiceFactory.GetPaymentProviderServiceAsync(_paymentProviderType);

        var response = await request.BaseRequest.TopupCancelAsync(cardTopupRequest, wallet, request.BaseRequest.Description, amount, paymentProvider, _masterpassService,_topupService);

        return response;
    }
}