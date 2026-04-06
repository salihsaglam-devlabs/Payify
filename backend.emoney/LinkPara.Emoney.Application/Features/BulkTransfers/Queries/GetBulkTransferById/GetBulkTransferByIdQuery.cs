using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Features.Wallets.Queries.TransferPreview;
using LinkPara.Emoney.Application.Features.Wallets.Queries.WithdrawPreview;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetBulkTransferById;

public class GetBulkTransferByIdQuery : IRequest<BulkTransferDto>
{
    public Guid Id { get; set; }
    public bool CalculateCommission { get; set; }
}

public class GetBulkTransferByIdQueryHandler : IRequestHandler<GetBulkTransferByIdQuery, BulkTransferDto>
{
    private readonly IGenericRepository<BulkTransfer> _repository;
    private readonly IMapper _mapper;
    private readonly IAccountService _accountService;
    private readonly IContextProvider _contextProvider;
    private readonly IPricingProfileService _pricingProfileService;
    private readonly ILogger<GetBulkTransferByIdQueryHandler> _logger;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IStringLocalizer _localizer;

    public GetBulkTransferByIdQueryHandler(IMapper mapper,
        IGenericRepository<BulkTransfer> repository,
        IAccountService accountService,
        IContextProvider contextProvider,
        ILogger<GetBulkTransferByIdQueryHandler> logger,
        IPricingProfileService pricingProfileService,
        IMoneyTransferService moneyTransferService,
        IGenericRepository<Wallet> walletRepository,
        IStringLocalizerFactory factory)
    {
        _mapper = mapper;
        _repository = repository;
        _accountService = accountService;
        _contextProvider = contextProvider;
        _logger = logger;
        _pricingProfileService = pricingProfileService;
        _moneyTransferService = moneyTransferService;
        _walletRepository = walletRepository;
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
    }

    public async Task<BulkTransferDto> Handle(GetBulkTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var createdUserId = _contextProvider.CurrentContext.UserId;

        var accountUser = await _accountService.GetCorporateAccountUserAsync(Guid.Parse(createdUserId));

        var bulkTransfer = await _repository
            .GetAll()
            .Where(x => x.Id == request.Id && accountUser.AccountId == x.AccountId)
            .Include(x => x.BulkTransferDetails)
            .FirstOrDefaultAsync();

        if (bulkTransfer is null)
        {
            throw new NotFoundException(nameof(BulkTransfer), request.Id);
        }

        var bulkTransferDetails = bulkTransfer.BulkTransferDetails;

        bulkTransferDetails.ForEach(bulkTransferDetail =>
        {

            if (bulkTransferDetail.BulkTransferDetailStatus == BulkTransferDetailStatus.Failed)
            {
                bulkTransferDetail.ExceptionMessage = string.IsNullOrWhiteSpace(bulkTransferDetail.ExceptionMessage)
                    ? _localizer.GetString("UnknownException").Value
                    : _localizer.GetString(bulkTransferDetail.ExceptionMessage).Value ?? _localizer.GetString("UnknownException").Value;
            }
        });

        var wallet = _walletRepository.GetAll()
            .FirstOrDefault(x => x.WalletNumber == bulkTransfer.SenderWalletNumber
                              && x.RecordStatus == RecordStatus.Active);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet), bulkTransfer.SenderWalletNumber);
        }

        if (request.CalculateCommission)
        {
            foreach (var bulkTransferDetail in bulkTransferDetails)
            {
                try
                {
                    if (bulkTransfer.BulkTransferType == BulkTransferType.Internal)
                    {
                        var pricingInfo = await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
                        {
                            TransferType = TransferType.Internal,
                            Amount = bulkTransferDetail.Amount,
                            CurrencyCode = wallet.CurrencyCode,
                            SenderWalletType = wallet.WalletType
                        });

                        bulkTransferDetail.CommissionAmount = pricingInfo.Fee + pricingInfo.CommissionAmount;
                        bulkTransferDetail.BsmvAmount = pricingInfo.BsmvTotal;
                    }
                    else if (bulkTransfer.BulkTransferType == BulkTransferType.Withdraw)
                    {
                        var transferBank = await _moneyTransferService.GetTransferBankAsync(new GetTransferBankRequest
                        {
                            Amount = bulkTransferDetail.Amount,
                            ReceiverIBAN = bulkTransferDetail.Receiver,
                            CurrencyCode = wallet.CurrencyCode,
                        });

                        var pricingInfo = await _pricingProfileService.CalculatePricingAsync(new CalculatePricingRequest
                        {
                            TransferType = transferBank.TransferType,
                            Amount = bulkTransferDetail.Amount,
                            BankCode = transferBank.TransferBankCode,
                            CurrencyCode = wallet.CurrencyCode,
                            SenderWalletType = wallet.WalletType
                        });

                        bulkTransferDetail.CommissionAmount = pricingInfo.Fee + pricingInfo.CommissionAmount;
                        bulkTransferDetail.BsmvAmount = pricingInfo.BsmvTotal;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Error On BulkTransferCalculatePricing Id: {bulkTransferDetail.Id} Exception:  {exception}");
                }
            }
        }

        return _mapper.Map<BulkTransferDto>(bulkTransfer);
    }
}