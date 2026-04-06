using LinkPara.CampaignManagement.Application.Commons.Exceptions;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ChargeByIWallet;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Commands.ReverseCharge;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Pagination;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges;
using LinkPara.CampaignManagement.Application.Features.IWalletCharges.Queries.GetCharges;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using LinkPara.MappingExtensions.Mapping;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletChargeService : IIWalletChargeService
{
    private readonly IGenericRepository<IWalletCharge> _repository;
    private readonly IGenericRepository<IWalletQrCode> _qrCodeRepository;
    private readonly ILogger<IWalletChargeService> _logger;
    private readonly IEmoneyService _eMoneyService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<IWalletChargeTransaction> _chargeTransactionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccountingService _accountingService;
    private readonly IMapper _mapper;

    public IWalletChargeService(IGenericRepository<IWalletCharge> repository,
        IGenericRepository<IWalletQrCode> qrCodeRepository,
        ILogger<IWalletChargeService> logger,
        IApplicationUserService applicationUserService,
        IEmoneyService eMoneyService,
        IGenericRepository<IWalletChargeTransaction> chargeTransactionRepository,
        IHttpContextAccessor httpContextAccessor,
        IAccountingService accountingService,
        IMapper mapper)
    {
        _repository = repository;
        _qrCodeRepository = qrCodeRepository;
        _logger = logger;
        _applicationUserService = applicationUserService;
        _eMoneyService = eMoneyService;
        _chargeTransactionRepository = chargeTransactionRepository;
        _httpContextAccessor = httpContextAccessor;
        _accountingService = accountingService;
        _mapper = mapper;
    }

    public async Task<Guid> SaveChargeAsync(ChargeByIWalletCommand request)
    {
        var qrCode = await _qrCodeRepository.GetAll().Where(x => x.QrCode == request.QrCode).SingleOrDefaultAsync();

        if (qrCode is null)
        {
            throw new NotFoundException(nameof(qrCode));
        }

        var charge = new IWalletCharge
        {
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            TerminalId = request.TerminalId,
            QrCode = request.QrCode,
            TerminalName = request.TerminalName,
            WalletId = request.WalletId,
            ChargeStatus = ChargeStatus.Pending,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            UserId = qrCode.UserId,
            WalletNumber = qrCode.WalletNumber,
            IWalletQrCodeId = qrCode.Id
        };

        await _repository.AddAsync(charge);

        await ProvisionPreviewAsync(charge, qrCode);

        return charge.Id;
    }

    private async Task ProvisionPreviewAsync(IWalletCharge charge, IWalletQrCode walletQrCode)
    {
        var provisionPreviewRequest = new ProvisionPreviewRequest
        {
            Amount = charge.Amount,
            CurrencyCode = "TRY",
            WalletNumber = walletQrCode.WalletNumber,
            UserId = walletQrCode.UserId
        };

        ProvisionPreviewResponse provisionPreview;

        try
        {
            provisionPreview = await _eMoneyService.PreviewProvisionAsync(provisionPreviewRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError("ProvisionPreviewException : {Exception}", exception);

            charge.ChargeStatus = ChargeStatus.Error;
            charge.ExceptionMessage = "ProvisionPreviewException";
            await _repository.UpdateAsync(charge);
            throw;
        }

        if (!provisionPreview.IsSuccess)
        {
            charge.ChargeStatus = ChargeStatus.Error;
            charge.ExceptionMessage = "ProvisionPreviewIsNotSuccessfull";
            await _repository.UpdateAsync(charge);

            _logger.LogError("ProvisionPreviewUnSuccess Message: {ErrorMessage}", provisionPreview.ErrorMessage);
            throw new ProvisionPreviewErrorException();
        }
    }

    public async Task ReverseChargeAsync(ReverseChargeCommand request)
    {
        var salesChargeTransaction = await _chargeTransactionRepository
            .GetAll()
            .Where(x => x.IWalletChargeId == request.ProcessGuid
                     && x.ChargeTransactionType == ChargeTransactionType.Sales)
            .SingleOrDefaultAsync();

        var charge = await _repository
            .GetAll()
            .Where(x => x.Id == request.ProcessGuid)
            .SingleOrDefaultAsync();

        if (salesChargeTransaction is null)
        {
            throw new NotFoundException(nameof(salesChargeTransaction), request.ProcessGuid);
        }

        if (charge is null)
        {
            throw new NotFoundException(nameof(charge), request.ProcessGuid);
        }

        var provisionReturnRequest = PrepareProvisionReturnRequest(request, charge, salesChargeTransaction);

        ProvisionResponse response = await _eMoneyService.ReturnProvisionAsync(provisionReturnRequest);

        await _accountingService.PostAccountingPaymentAsync(new AccountingPayment
        {
            AccountingTransactionType = AccountingTransactionType.Emoney,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            Amount = request.ReversedAmount,
            CurrencyCode = "TRY",
            OperationType = OperationType.ReturnCampaignPayment,
            Destination = $"WA-{charge.WalletNumber}",
            TransactionDate = DateTime.Now,
            UserId = charge.UserId
        });


        CancelProvisionCashbackRequest cancelProvisionCashbackRequest = PrepareCancelProvisionRequest(request, salesChargeTransaction, charge);

        await _eMoneyService.CancelProvisionCashbackAsync(cancelProvisionCashbackRequest);

        await _accountingService.PostAccountingPaymentAsync(new AccountingPayment
        {
            AccountingTransactionType = AccountingTransactionType.Emoney,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            Amount = request.CashBackAmount,
            CurrencyCode = "TRY",
            OperationType = OperationType.ReturnCampaignCashback,
            Source = $"WA-{charge.WalletNumber}",
            TransactionDate = DateTime.Now,
            UserId = charge.UserId
        });

        await SaveReverseChargeTransactionsAsync(charge, request, response);
    }

    private static CancelProvisionCashbackRequest PrepareCancelProvisionRequest(ReverseChargeCommand request, IWalletChargeTransaction salesChargeTransaction, IWalletCharge charge)
    {
        return new CancelProvisionCashbackRequest
        {
            Amount = request.CashBackAmount,
            UserId = charge.UserId,
            WalletNumber = charge.WalletNumber,
            ProvisionReference = salesChargeTransaction.ProvisionReferenceNumber
        };
    }

    private ProvisionReturnRequest PrepareProvisionReturnRequest(ReverseChargeCommand request, IWalletCharge charge, IWalletChargeTransaction salesChargeTransaction)
    {
        var clientIpAddress = _httpContextAccessor.HttpContext?.Request?.Headers["ClientIpAddress"].ToString();

        var provisionReturnRequest = new ProvisionReturnRequest
        {
            Amount = request.ReversedAmount,
            ClientIpAddress = string.IsNullOrEmpty(clientIpAddress) ? "IpNotFound" : clientIpAddress,
            ConversationId = Guid.NewGuid().ToString(),
            ProvisionSource = ProvisionSource.IWallet,
            UserId = charge.UserId,
            WalletNumber = charge.WalletNumber,
            CurrencyCode = "TRY",
            ProvisionReferenceNumber = salesChargeTransaction.ProvisionReferenceNumber
        };

        return provisionReturnRequest;
    }

    private async Task SaveReverseChargeTransactionsAsync(IWalletCharge charge, ReverseChargeCommand request, ProvisionResponse provision)
    {
        await _chargeTransactionRepository.AddAsync(
            new IWalletChargeTransaction
            {
                Amount = request.ReversedAmount,
                ChargeTransactionType = ChargeTransactionType.Reverse,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                IWalletChargeId = charge.Id,
                ProvisionConversationId = provision.ConversationId,
                ProvisionReferenceNumber = provision.ReferenceNumber,
            }
        );
        await _chargeTransactionRepository.AddAsync(
            new IWalletChargeTransaction
            {
                Amount = request.CashBackAmount,
                ChargeTransactionType = ChargeTransactionType.ReverseCashback,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                IWalletChargeId = charge.Id
            }
        );
    }

    public async Task<PaginatedList<ChargeTransactionDto>> GetChargeTransactionsAsync(GetChargeTransactionsSearchQuery request)
    {
        var transactions = _chargeTransactionRepository.GetAll()
            .Include(x => x.IWalletCharge)
                .ThenInclude(x => x.IWalletQrCode)
                    .ThenInclude(x => x.IWalletCard)
            .Include(x => x.IWalletCharge);

        if (request.TransactionDateEnd is not null)
        {
            transactions = transactions
                .Where(x => x.IWalletCharge.CreateDate <= request.TransactionDateEnd)
                .Include(x => x.IWalletCharge);
        }

        if (request.TransactionDateStart is not null)
        {
            transactions = transactions
                .Where(x => x.IWalletCharge.CreateDate >= request.TransactionDateStart)
                .Include(x => x.IWalletCharge);
        }

        if (!string.IsNullOrEmpty(request.FullName))
        {
            transactions = transactions
                .Where(x => x.IWalletCharge.IWalletQrCode.IWalletCard.FullName.Contains(request.FullName))
                .Include(x => x.IWalletCharge)
                   .ThenInclude(x => x.IWalletQrCode)
                       .ThenInclude(x => x.IWalletCard)
                .Include(x => x.IWalletCharge);
        }

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            transactions = transactions
                .Where(x => x.IWalletCharge.WalletNumber.Contains(request.WalletNumber))
                .Include(x => x.IWalletCharge);
        }

        if (request.ChargeTransactionType is not null)
        {
            transactions = transactions
                .Where(x => x.ChargeTransactionType == request.ChargeTransactionType)
                .Include(x => x.IWalletCharge);
        }

        if (request.SourceCampaignType is not null)
        {
            transactions = transactions
                .Where(x => x.IWalletCharge.SourceCampaignType == request.SourceCampaignType)
                .Include(x => x.IWalletCharge);
        }
        var result = await transactions.OrderByDescending(b => b.IWalletCharge.CreateDate)
            .PaginatedListWithMappingAsync<IWalletChargeTransaction, ChargeTransactionDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        return result;

    }
}