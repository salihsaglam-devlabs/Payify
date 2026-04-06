using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IResponseCodeService
{
    Task<MerchantResponseCodeDto> GetMerchantResponseCodeAsync(string merchantResponseCode, string languageCode);
    Task<MerchantResponseCodeDto> GetApiResponseCode(string code, string languageCode);
    Task<MerchantResponseCodeDto> GetMerchantResponseCodeByBankCodeAsync(int bankCode, string bankResponseCode, string errorMessage, string languageCode);
}