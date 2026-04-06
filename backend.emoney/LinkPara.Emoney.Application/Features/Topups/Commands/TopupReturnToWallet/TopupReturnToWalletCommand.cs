using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupReturnToWallet;

public class TopupReturnToWalletCommand : IRequest
{
    public Guid CardTopupRequestId { get; set; }
}

public class TopupReturnToWalletHandler : IRequestHandler<TopupReturnToWalletCommand>
{
    private readonly ITopupService _topupService;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IWalletService _walletService;
    private readonly IGenericRepository<Wallet> _walletRepository;

    public TopupReturnToWalletHandler(ITopupService topupService,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IWalletService walletService,
        IGenericRepository<Wallet> walletRepository)
    {
        _topupService = topupService;
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _walletService = walletService;
        _walletRepository = walletRepository;
    }

    public async Task<Unit> Handle(TopupReturnToWalletCommand request, CancellationToken cancellationToken)
    {
        var cardTopupRequest = await _cardTopupRequestRepository.GetByIdAsync(request.CardTopupRequestId);

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

        await _topupService.TopupReverseAsync(cardTopupRequest, CardTopupRequestStatus.Failed, wallet);

        return Unit.Value;
    }
}
