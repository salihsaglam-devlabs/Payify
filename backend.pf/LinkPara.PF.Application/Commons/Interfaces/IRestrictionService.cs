using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IRestrictionService
{
    Task IsUserAuthorizedAsync(Guid merchantId);
    Task RestrictMerchantTypes(List<MerchantType> merchantTypes);
}