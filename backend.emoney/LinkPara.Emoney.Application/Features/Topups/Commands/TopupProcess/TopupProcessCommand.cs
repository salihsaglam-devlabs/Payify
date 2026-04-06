using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;

public class TopupProcessCommand : IRequest<TopupProcessResponse>
{
    public TopupProcessBaseRequest BaseRequest { get; set; }
}

public class TopupProcessCommandHandler : IRequestHandler<TopupProcessCommand, TopupProcessResponse>
{
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly ILimitService _limitService;
    private readonly IContextProvider _contextProvider;
    private readonly ITopupService _topupService;
    private readonly IPaymentProviderServiceFactory _paymentProviderServiceFactory;
    private readonly string _paymentProviderType;
    private readonly IMasterpassService _masterpassService;
    private readonly IVaultClient _vaultClient;

    public TopupProcessCommandHandler(IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IGenericRepository<Wallet> walletRepository,
        ILimitService limitService,
        ITopupService topupService,
        IContextProvider contextProvider,
        IPaymentProviderServiceFactory paymentProviderServiceFactory,
        IVaultClient vaultClient,
        IMasterpassService masterpassService)
    {
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _walletRepository = walletRepository;
        _limitService = limitService;
        _topupService = topupService;
        _contextProvider = contextProvider;
        _vaultClient = vaultClient;
        _paymentProviderServiceFactory = paymentProviderServiceFactory;
        _paymentProviderType = _vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
        _masterpassService = masterpassService;
    }

    public async Task<TopupProcessResponse> Handle(TopupProcessCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository
                       .GetAll()
                       .Include(w => w.Account)
                       .ThenInclude(w => w.AccountUsers)
                       .Where(w => w.Account.AccountUsers.Any(a => a.UserId == request.BaseRequest.UserId)
                              && w.WalletNumber.Equals(request.BaseRequest.WalletNumber)
                              && w.RecordStatus == RecordStatus.Active)
                       .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Wallet), wallet);
        }

        var validateLimit = await ValidateLimitsAsync(wallet, request);

        if (!validateLimit)
        {
            throw new AccountLimitInsufficientException();
        }

        var cardTopupRequest = await _cardTopupRequestRepository.GetByIdAsync(request.BaseRequest.CardTopupRequestId);

        if (cardTopupRequest == null)
        {
            throw new NotFoundException(nameof(CardTopupRequest), cardTopupRequest);
        }

        await UpdateCardTopupRequestAsync(cardTopupRequest, wallet, request.BaseRequest.Description);

        var paymentProviderService = await _paymentProviderServiceFactory.GetPaymentProviderServiceAsync(_paymentProviderType);

        return await request.BaseRequest.ProcessAsync(wallet, cardTopupRequest, _topupService, paymentProviderService, _masterpassService);
    }

    private async Task<bool> ValidateLimitsAsync(Wallet wallet, TopupProcessCommand command)
    {
        var response = await _limitService.IsLimitExceededAsync(new LimitControlRequest
        {
            Amount = command.BaseRequest.Amount,
            CurrencyCode = command.BaseRequest.Currency,
            LimitOperationType = LimitOperationType.Deposit,
            AccountId = wallet.AccountId,
            WalletNumber = wallet.WalletNumber
        });

        return !response.IsLimitExceeded;
    }

    private async Task UpdateCardTopupRequestAsync(CardTopupRequest request, Wallet wallet, string description)
    {
        if (request != null)
        {
            request.WalletId = wallet.Id;
            request.WalletNumber = wallet.WalletNumber;
            request.Name = wallet.Account.Name;
            request.UpdateDate = DateTime.Now;
            request.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
            request.Description = description;

            await _cardTopupRequestRepository.UpdateAsync(request);
        }
    }
}