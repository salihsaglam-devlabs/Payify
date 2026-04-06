using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Inquire;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class InquireService : IInquireService
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IResponseCodeService _errorCodeService;

    public InquireService(IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<Currency> currencyRepository,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IResponseCodeService errorCodeService)
    {
        _merchantTransactionRepository = merchantTransactionRepository;
        _currencyRepository = currencyRepository;
        _acquireBankRepository = acquireBankRepository;
        _errorCodeService = errorCodeService;
    }

    public async Task<InquireResponse> InquireAsync(InquireCommand request)
    {
        MerchantTransaction merchantTransaction;

        if(!string.IsNullOrEmpty(request.PaymentConversationId) && !string.IsNullOrEmpty(request.OrderId))
        {
            merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s =>
                (s.ConversationId == request.PaymentConversationId && s.OrderId == request.OrderId) &&
                s.MerchantId == request.MerchantId);
        }
        else
        {
            merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s =>
                (s.ConversationId == request.PaymentConversationId || s.OrderId == request.OrderId) &&
                s.MerchantId == request.MerchantId);
        }

        if (merchantTransaction is null)
        {
            var error = await GetValidationResponseAsync(ErrorCode.NotFound, request.LanguageCode);

            return new InquireResponse
            {
                IsSucceed = false,
                ErrorCode = error.Code,
                ErrorMessage = error.Message,
                ConversationId = request.ConversationId
            };
        }

        var currency = await _currencyRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Number == merchantTransaction.Currency);

        var acquireBank = await _acquireBankRepository.GetAll()
            .FirstOrDefaultAsync(s => s.BankCode == merchantTransaction.IssuerBankCode);

        var cardNetwork = acquireBank?.CardNetwork ?? CardNetwork.Unknown;

        var provisionList = new List<ProvisionModel>();

        if (merchantTransaction.IsReverse)
        {
            provisionList.Add(new ProvisionModel
            {
                Amount = merchantTransaction.Amount,
                TransactionStatus = TransactionStatus.Success,
                TransactionType = TransactionType.Reverse,
                TransactionDate = merchantTransaction.ReverseDate.ToString(DateFormat)
            });
        }

        if (merchantTransaction.IsReturn)
        {
            var returnTransactions = await _merchantTransactionRepository.GetAll()
                .Where(s => s.ReturnedTransactionId == merchantTransaction.Id.ToString())
                .OrderBy(s => s.ReturnDate)
                .Select(s => new ProvisionModel
                {
                    Amount = s.Amount,
                    TransactionDate = s.CreateDate.ToString(DateFormat),
                    TransactionStatus = s.TransactionStatus,
                    TransactionType = s.TransactionType
                }).ToListAsync();

            provisionList.AddRange(returnTransactions);
        }

        if (merchantTransaction.IsPreClose && merchantTransaction.PreCloseTransactionId is not null)
        {
            var postAuth = await _merchantTransactionRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(merchantTransaction.PreCloseTransactionId));

            provisionList.Add(new ProvisionModel
            {
                Amount = postAuth.Amount,
                TransactionStatus = postAuth.TransactionStatus,
                TransactionType = postAuth.TransactionType,
                TransactionDate = merchantTransaction.PreCloseDate.ToString(DateFormat)
            });
        }

        provisionList.Add(new ProvisionModel
        {
            Amount = merchantTransaction.Amount,
            TransactionDate = merchantTransaction.CreateDate.ToString(DateFormat),
            TransactionStatus = merchantTransaction.TransactionStatus,
            TransactionType = merchantTransaction.TransactionType
        });

        var response = new InquireResponse
        {
            ConversationId = request.ConversationId,
            Amount = merchantTransaction.Amount,
            BinNumber = merchantTransaction.BinNumber,
            Currency = currency?.Code ?? string.Empty,
            PaymentConversationId = merchantTransaction.ConversationId,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            PointAmount = merchantTransaction.PointAmount,
            InstallmentCount = merchantTransaction.InstallmentCount,
            Is3ds = merchantTransaction.Is3ds,
            TransactionStatus = merchantTransaction.TransactionStatus,
            TransactionType = merchantTransaction.TransactionType,
            CardBrand = string.IsNullOrEmpty(merchantTransaction.CardNumber)
            ? CardBrand.Undefined
            : CardHelper.GetCardBrand(merchantTransaction.CardNumber),
            CardNumber = merchantTransaction.CardNumber,
            CardType = merchantTransaction.CardType,
            CardNetwork = cardNetwork,
            IsSucceed = true,
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty,
            OrderId = merchantTransaction.OrderId,
            ProvisionList = provisionList,
            ReturnStatus = merchantTransaction.ReturnStatus
        };

        return response;
    }

    private async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
    {
        var merchantResponse = await _errorCodeService.GetMerchantResponseCodeAsync(errorCode, languageCode);

        return new ValidationResponse
        {
            Code = errorCode,
            IsValid = false,
            Message = merchantResponse.DisplayMessage
        };
    }

}