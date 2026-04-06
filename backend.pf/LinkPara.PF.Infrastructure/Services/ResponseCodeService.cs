using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class ResponseCodeService : IResponseCodeService
{
    private readonly ILogger<ResponseCodeService> _logger;
    private readonly IGenericRepository<BankResponseCode> _bankResponseCodeRepository;
    private readonly IGenericRepository<MerchantResponseCode> _merchantResponseCodeRepository;
    private readonly IGenericRepository<ApiResponseCode> _apiResponseCodeRepository;
    private readonly IApplicationUserService _applicationUserService;

    private const string GenericErrorCode = "99";
    private const string GenericErrorMessage = "Unapproved";

    public ResponseCodeService(IGenericRepository<BankResponseCode> bankResponseCodeRepository,
        IGenericRepository<MerchantResponseCode> merchantResponseCodeRepository,
        ILogger<ResponseCodeService> logger,
        IApplicationUserService applicationUserService,
        IGenericRepository<ApiResponseCode> apiResponseCodeRepository)
    {
        _bankResponseCodeRepository = bankResponseCodeRepository;
        _merchantResponseCodeRepository = merchantResponseCodeRepository;
        _logger = logger;
        _applicationUserService = applicationUserService;
        _apiResponseCodeRepository = apiResponseCodeRepository;
    }

    public async Task<MerchantResponseCodeDto> GetMerchantResponseCodeAsync(string merchantResponseCode, string languageCode)
    {
        var merchantResponse =  await _merchantResponseCodeRepository.GetAll()
            .FirstOrDefaultAsync(w => w.ResponseCode == merchantResponseCode);

        if (merchantResponse is not null)
        {
            return new MerchantResponseCodeDto
            {
                Description = merchantResponse.Description,
                ResponseCode = merchantResponse.ResponseCode,
                DisplayMessage = (languageCode.ToLower() == "tr")
                    ? merchantResponse.DisplayMessageTr
                    : merchantResponse.DisplayMessageEn
            };
        }
        
        return GetGenericResponseCode(languageCode, false);
    }

    public async Task<MerchantResponseCodeDto> GetApiResponseCode(string code, string languageCode)
    {
        var apiResponse = await _apiResponseCodeRepository.GetAll()
            .Include(i => i.MerchantResponseCode)
            .FirstOrDefaultAsync(b => b.ResponseCode == code);

        if (apiResponse is not null)
        {
            if (apiResponse.MerchantResponseCode is not null)
            {
                return new MerchantResponseCodeDto
                {
                    ResponseCode = apiResponse.MerchantResponseCode.ResponseCode,
                    Description = apiResponse.MerchantResponseCode.Description,
                    DisplayMessage = languageCode.ToLower() == "tr"
                        ? apiResponse.MerchantResponseCode.DisplayMessageTr
                        : apiResponse.MerchantResponseCode.DisplayMessageEn
                };
            }

            _logger.LogError("Unmapped api response code! : ResponseCode : {code}",
                 code);
        }

        return GetGenericResponseCode(languageCode, false);
    }
    public async Task<MerchantResponseCodeDto> GetMerchantResponseCodeByBankCodeAsync(int bankCode,
        string bankResponseCode, string errorMessage, string languageCode)
    {
        var bankError = await _bankResponseCodeRepository.GetAll()
            .Include(i => i.MerchantResponseCode)
            .FirstOrDefaultAsync(w => w.ResponseCode == bankResponseCode
                                       && w.BankCode == bankCode);

        if (bankError is not null)
        {
            if (bankError.MerchantResponseCode is not null)
            {
                return new MerchantResponseCodeDto
                {
                    ResponseCode = bankError.MerchantResponseCode.ResponseCode,
                    Description = bankError.MerchantResponseCode.Description,
                    DisplayMessage = languageCode.ToLower() == "tr"
                        ? bankError.MerchantResponseCode.DisplayMessageTr
                        : bankError.MerchantResponseCode.DisplayMessageEn,
                    ProcessTimeoutManagement = bankError.ProcessTimeoutManagement
                };
            }
            
            _logger.LogError("Unmapped bank response code! : BankCode : {bank}, ResponseCode : {code}",
                bankCode, bankResponseCode);

            return GetGenericResponseCode(languageCode, bankError.ProcessTimeoutManagement);
        }

        await _bankResponseCodeRepository.AddAsync(new BankResponseCode
        {
            BankCode = bankCode,
            Description = errorMessage,
            ResponseCode = bankResponseCode,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            ProcessTimeoutManagement = false
        });

        return GetGenericResponseCode(languageCode, false);
    }
    
    private static MerchantResponseCodeDto GetGenericResponseCode(string languageCode,
        bool processTimeoutManagement)
    {
        return new MerchantResponseCodeDto
        {
            ResponseCode = GenericErrorCode,
            Description = GenericErrorMessage,
            DisplayMessage = languageCode.ToLower() == "tr"
                ? "BeklenmedikHata"
                : "UnexpectedErrorOccured",
            ProcessTimeoutManagement = processTimeoutManagement
        };
    }
}