using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.CardBins.Command.DeleteCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetAllCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinById;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.GetBinInformation;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class CardBinService : ICardBinService
{
    private readonly ILogger<CardBinService> _logger;
    private readonly IGenericRepository<CardBin> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly ICacheService _cacheService;
    private readonly IMerchantService _merchantService;
    private readonly ICardTokenService _cardTokenService;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public CardBinService(ILogger<CardBinService> logger,
        IGenericRepository<CardBin> repository,
        IMapper mapper,
        IGenericRepository<Bank> bankRepository,
        ICacheService cacheService,
        IMerchantService merchantService,
        ICardTokenService cardTokenService,
        IResponseCodeService errorCodeService,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<Merchant> merchantRepository)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _bankRepository = bankRepository;
        _cacheService = cacheService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _merchantService = merchantService;
        _cardTokenService = cardTokenService;
        _errorCodeService = errorCodeService;
        _merchantRepository = merchantRepository;
    }

    public async Task DeleteAsync(DeleteCardBinCommand request)
    {
        var cardBin = await _repository.GetByIdAsync(request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (cardBin is null)
        {
            throw new NotFoundException(nameof(CardBin), request.Id);
        }

        try
        {
            cardBin.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(cardBin);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteCardBin",
                SourceApplication = "PF",
                Resource = "CardBin",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"Id", request.Id.ToString() },
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"CardBinDeleteError : {exception}");
        }
    }

    public async Task<PaginatedList<CardBinDto>> GetListAsync(GetAllCardBinQuery request)
    {
        var cardBinList = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            cardBinList = cardBinList
                .Where(b => b.BinNumber.Contains(request.Q));
        }

        if (request.RecordStatus is not null)
        {
            cardBinList = cardBinList
                .Where(b => b.RecordStatus == request.RecordStatus);
        }

        if (request.CardType is not null)
        {
            cardBinList = cardBinList
                .Where(b => b.CardType == request.CardType);
        }

        if (request.CardBrand is not null)
        {
            cardBinList = cardBinList
                .Where(b => b.CardBrand == request.CardBrand);
        }

        if (request.CardNetwork is not null)
        {
            cardBinList = cardBinList
                .Where(b => b.CardNetwork == request.CardNetwork);
        }

        if (request.CardSubType is not null)
        {
            cardBinList = cardBinList
                .Where(b => b.CardSubType == request.CardSubType);
        }

        return await cardBinList.Include(b => b.Bank).OrderBy(b => b.Bank.Name)
            .PaginatedListWithMappingAsync<CardBin, CardBinDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveCardBinCommand request)
    {
        var bank = await _bankRepository.GetAll()
            .FirstOrDefaultAsync(b => b.Code == request.BankCode);

        if (bank is null)
        {
            throw new NotFoundException(nameof(Bank), request.BankCode);
        }

        var activeBin = await _repository.GetAll().FirstOrDefaultAsync(
            b => b.BinNumber == request.BinNumber);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            if (activeBin is not null)
            {
                if (activeBin.RecordStatus == RecordStatus.Active)
                {
                    throw new DuplicateRecordException();
                }
                else
                {
                    activeBin = _mapper.Map(request, activeBin);
                    activeBin.RecordStatus = RecordStatus.Active;

                    await _repository.UpdateAsync(activeBin);

                    await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = true,
                            LogDate = DateTime.Now,
                            Operation = "UpdateCardBin",
                            SourceApplication = "PF",
                            Resource = "CardBin",
                            UserId = parseUserId,
                            Details = new Dictionary<string, string>
                            {
                        {"Id", activeBin.Id.ToString() },
                        {"BinNumber", request.BinNumber },
                        {"BankCode", request.BankCode.ToString() },
                        {"CountryName", request.CountryName },
                            }
                        });
                }
            }
            else
            {
                var cardBin = _mapper.Map<CardBin>(request);
                cardBin.CreateDate = DateTime.Now;
                cardBin.CreatedBy = parseUserId.ToString();
                await _repository.AddAsync(cardBin);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SaveCardBin",
                        SourceApplication = "PF",
                        Resource = "CardBin",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                        {"BinNumber", request.BinNumber },
                        {"BankCode", request.BankCode.ToString() },
                        {"CountryName", request.CountryName },
                        {"CardType", request.CardType.Value.ToString() },
                        }
                    });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"CardBinCreateError : {exception}");
            throw;
        }
    }
    public async Task UpdateAsync(UpdateCardBinCommand request)
    {
        var cardBin = await _repository.GetAll().FirstOrDefaultAsync(
            b => b.Id == request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (cardBin is null)
        {
            throw new NotFoundException(nameof(cardBin));
        }

        var activeBin = await _repository.GetAll().FirstOrDefaultAsync(
            b => b.Id != request.Id
                 && b.BinNumber == request.BinNumber);

        if (activeBin is not null)
        {
            throw new DuplicateRecordException();
        }

        var bank = await _bankRepository.GetAll()
            .FirstOrDefaultAsync(b => b.Code == request.BankCode);

        if (bank is null)
        {
            throw new NotFoundException(nameof(bank), request.BankCode);
        }

        try
        {
            cardBin = _mapper.Map(request, cardBin);

            await _repository.UpdateAsync(cardBin);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateCardBin",
                    SourceApplication = "PF",
                    Resource = "CardBin",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString() },
                        {"BinNumber", request.BinNumber },
                        {"BankCode", request.BankCode.ToString() },
                        {"CountryName", request.CountryName },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"CardBinUpdateError : {exception}");
            throw;
        }
    }

    public async Task<CardBinDto> GetByIdAsync(GetCardBinByIdQuery request)
    {
        var cardBin = await _repository.GetAll().Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (cardBin is null)
        {
            throw new NotFoundException(nameof(CardBin), request.Id);
        }

        return _mapper.Map<CardBinDto>(cardBin);
    }

    public async Task<CardBinDto> GetByNumberAsync(string binNumber)
    {
        var cardBin = new CardBin();

        if (binNumber.Length > 6)
        {
            var eightBinNumber = binNumber[..8];

            cardBin = await _cacheService.GetOrCreateAsync(eightBinNumber,
                async () =>
                {
                    return await _repository
                        .GetAll()
                        .Include(b => b.Bank)
                        .FirstOrDefaultAsync(b => b.BinNumber == eightBinNumber
                                                  && b.RecordStatus == RecordStatus.Active);
                });
        }

        if (cardBin == null || string.IsNullOrEmpty(cardBin.BinNumber))
        {
            var sixBinNumber = binNumber[..6];

            cardBin = await _cacheService.GetOrCreateAsync(sixBinNumber,
            async () =>
            {
                return await _repository
                    .GetAll()
                    .Include(b => b.Bank)
                    .FirstOrDefaultAsync(b => b.BinNumber == sixBinNumber
                                              && b.RecordStatus == RecordStatus.Active);
            });
        }

        return _mapper.Map<CardBinDto>(cardBin);
    }

    public async Task<GetBinInformationResponse> GetBinInformationAsync(GetBinInformationCommand request)
    {
        var merchant = await _merchantService.GetByIdAsync(request.MerchantId);

        var parentMerchantFinancialTransaction = true;
        if (merchant?.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }
        var validationResponse = await PreValidate(request, merchant, parentMerchantFinancialTransaction);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"Bin Validation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            return new GetBinInformationResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ErrorMessage = validationResponse.Message,
                ConversationId = request.ConversationId
            };
        }

        var binNumber = request.BinNumber;

        if (!string.IsNullOrEmpty(request.CardToken))
        {
            var cardToken = await _cardTokenService.GetByToken(request.CardToken);

            if (cardToken is not null)
            {
                try
                {
                    var cardInfo = await _cardTokenService.GetCardDetailsAsync(cardToken);
                    binNumber = CardHelper.GetBinNumber(cardInfo.CardNumber);
                }
                catch(Exception e)
                {
                    _logger.LogError($"InvalidCardInfoException: {e}" );
                    var invalidCardResponse =
                        await GetValidationResponseAsync(ApiErrorCode.InvalidCardInfo, request.LanguageCode);

                    return PopulateErrorResponse(invalidCardResponse, request.ConversationId, binNumber);
                }
            }
        }

        var bin = await GetByNumberAsync(binNumber);

        if (bin is not null)
        {
            return PopulateBinInformation(bin, request.ConversationId);
        }

        var notFoundResponse = await GetValidationResponseAsync(ApiErrorCode.CardBinNotFound, request.LanguageCode);

        return PopulateErrorResponse(notFoundResponse, request.ConversationId, binNumber);
    }

    private async Task<ValidationResponse> PreValidate(GetBinInformationCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction)
    {
        if (merchant is null)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(request.CardToken))
        {
            var cardToken = await _cardTokenService.GetByToken(request.CardToken);

            if (cardToken is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidToken, request.LanguageCode);
            }

            if (DateTime.Now.CompareTo(cardToken.ExpiryDate) >= 0)
            {
                return await GetValidationResponseAsync(ApiErrorCode.CardTokenExpired, request.LanguageCode);
            }
        }

        return new ValidationResponse { IsValid = true };
    }

    private async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
    {
        var merchantResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ValidationResponse
        {
            Code = errorCode,
            IsValid = false,
            Message = merchantResponse.DisplayMessage
        };
    }

    private GetBinInformationResponse PopulateBinInformation(CardBinDto bin, string conversationId)
    {
        return new GetBinInformationResponse
        {
            CardBrand = bin.CardBrand,
            BankCode = bin.Bank.Code,
            BankName = bin.Bank.Name,
            BinNumber = bin.BinNumber,
            CardNetwork = bin.CardNetwork,
            CardType = bin.CardType,
            CardSubType = bin.CardSubType,
            IsVirtual = bin.IsVirtual,
            ConversationId = conversationId,
            ErrorCode = string.Empty,
            ErrorMessage = string.Empty,
            IsSucceed = true
        };
    }

    private GetBinInformationResponse PopulateErrorResponse(ValidationResponse validationResponse,
        string conversationId, string binNumber)
    {
        _logger.LogError($"Bin Validation failed with code : {validationResponse.Code}, " +
                         $"Message: {validationResponse.Message}");

        return new GetBinInformationResponse
        {
            IsSucceed = true,
            ErrorCode = validationResponse.Code,
            ErrorMessage = validationResponse.Message,
            ConversationId = conversationId,
            BankCode = 0,
            BankName = string.Empty,
            BinNumber = binNumber,
            CardBrand = CardBrand.Undefined,
            CardNetwork = CardNetwork.Unknown,
            CardType = CardType.Unknown,
            IsVirtual = false
        };
    }
}