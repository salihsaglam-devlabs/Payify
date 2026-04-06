using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Features.Payments.Commands.PointInquiry;
using LinkPara.PF.Application.Features.Tokens;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class PointInquiryService : IPointInquiryService
{
    private const string GenericErrorCode = "99";

    private readonly ILogger<PointInquiryService> _logger;
    private readonly ICardBinService _binService;
    private readonly ICurrencyService _currencyService;
    private readonly ICardTokenService _cardTokenService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IGenericRepository<MerchantVpos> _merchantVpos;
    private readonly PfDbContext _dbContext;
    private readonly IResponseCodeService _errorCodeService;
    public PointInquiryService(ICardBinService binService,
        ILogger<PointInquiryService> logger,
        ICurrencyService currencyService,
        ICardTokenService cardTokenService,
        IGenericRepository<Vpos> vposRepository,
        VposServiceFactory vposServiceFactory,
        IGenericRepository<MerchantVpos> merchantVpos,
        PfDbContext dbContext,
        IResponseCodeService errorCodeService)
    {
        _binService = binService;
        _logger = logger;
        _currencyService = currencyService;
        _cardTokenService = cardTokenService;
        _vposRepository = vposRepository;
        _vposServiceFactory = vposServiceFactory;
        _merchantVpos = merchantVpos;
        _dbContext = dbContext;
        _errorCodeService = errorCodeService;
    }
    public async Task<PointInquiryResponse> PointInquiryAsync(PointInquiryCommand request)
    {
        var cardToken = await _cardTokenService.GetByToken(request.CardToken);
        var card = await GetCardDetailsAsync(cardToken);
        var bin = await _binService.GetByNumberAsync(card.CardNumber);

        if (bin is null)
        {
            var errorCode = ApiErrorCode.CardBinNotFound;
            var apiResponse = await _errorCodeService.GetApiResponseCode(errorCode, request.LanguageCode);
            
            return new PointInquiryResponse
            {
                IsSucceed = false,
                ErrorCode = apiResponse.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage
            };
        }
        var merchantVposList = new List<MerchantVpos>();
        merchantVposList = _dbContext.MerchantVpos
            .Include(b => b.Vpos)
            .Where(b => b.MerchantId == request.MerchantId
                && b.RecordStatus == RecordStatus.Active
                && b.Vpos.VposStatus == VposStatus.Active
                && b.Vpos.AcquireBank.BankCode == bin.BankCode)
            .ToList();

        if (!merchantVposList.Any())
        {
            var loyalty = _dbContext.CardLoyalty
                  .Where(b => b.Name == bin.CardNetwork.ToString()
                      && b.RecordStatus == RecordStatus.Active)
                  .ToList();

            merchantVposList = _dbContext.MerchantVpos
                .Include(b => b.Vpos)
                .Where(b => b.MerchantId == request.MerchantId
                    && b.RecordStatus == RecordStatus.Active
                    && b.Vpos.VposStatus == VposStatus.Active
                    && loyalty.Select(x => x.BankCode).ToList().Contains(b.Vpos.AcquireBank.BankCode))
                .ToList();
        }

        if (!merchantVposList.Any())
        {
            var errorCode = ApiErrorCode.PointInquiryNotFound;

            var apiResponse = await _errorCodeService.GetApiResponseCode(errorCode, request.LanguageCode);
            
            return new PointInquiryResponse
            {
                IsSucceed = false,
                ErrorCode = apiResponse.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage
            };
        }

        var vpos = await _vposRepository.GetAll()
            .Include(s => s.AcquireBank)
            .Include(s => s.VposBankApiInfos)
            .ThenInclude(s => s.Key)
            .FirstOrDefaultAsync(b => b.Id == merchantVposList.FirstOrDefault().VposId);

        var subMerchant = await GetSubMerchantCode(vpos.Id, request.MerchantId);
        var currency = await _currencyService.GetByCodeAsync(request.Currency);
        var bankService = _vposServiceFactory.GetVposServices(vpos, request.MerchantId);

        try
        {
            var pointResponse = await bankService.PointInquiry(new PosPointInquiryRequest
            {
                CardNumber = card.CardNumber,
                Cvv2 = card.Cvv,
                ExpireMonth = card.ExpireMonth,
                ExpireYear = card.ExpireYear,
                OrderNumber = Guid.NewGuid().ToString(),
                Currency = currency.Number,
                LanguageCode = request.LanguageCode,
                SubMerchantCode = subMerchant.SubMerchantCode,
                SubMerchantTerminalNo = subMerchant.TerminalNo,
                ServiceProviderPspMerchantId = subMerchant.ServiceProviderPspMerchantId,
                ClientIp = request.ClientIpAddress
            });
            
            if (pointResponse.IsSuccess == false)
            {
                return new PointInquiryResponse
                {
                    IsSucceed = false,
                    ErrorCode = pointResponse.ResponseCode,
                    ErrorMessage = pointResponse.ResponseMessage,
                    AvailablePoint = pointResponse.AvailablePoint
                };
            }

            return new PointInquiryResponse
            {
                IsSucceed = true,
                AvailablePoint = pointResponse.AvailablePoint
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Point Inquiry Error - {exception}");
            
            return new PointInquiryResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError"
            };
        }
    }
    private async Task<CardInfoDto> GetCardDetailsAsync(CardToken token)
    {
        return await _cardTokenService.GetCardDetailsAsync(token);
    }
    private async Task<MerchantVpos> GetSubMerchantCode(Guid vposId, Guid merchantId)
    {
        var subMerchant = await _merchantVpos
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.VposId == vposId &&
                                      s.MerchantId == merchantId);

        return subMerchant ?? new MerchantVpos();
    }
}
