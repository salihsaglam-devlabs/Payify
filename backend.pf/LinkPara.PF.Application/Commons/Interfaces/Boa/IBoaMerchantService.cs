using LinkPara.PF.Application.Features.Boa.Merchants;
using LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;

namespace LinkPara.PF.Application.Commons.Interfaces.Boa;

public interface IBoaMerchantService
{
    Task<CreateBoaMerchantResponse> CreateBoaMerchantAsync(CreateBoaMerchantCommand command);
    Task<BoaMerchantDto> GetMerchantByNumberAsync(string merchantNumber);
}