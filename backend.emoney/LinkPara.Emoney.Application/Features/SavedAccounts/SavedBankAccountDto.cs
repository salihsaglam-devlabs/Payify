using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Commons.Models.BankingModels.Configurations;
using LinkPara.Emoney.Application.Features.Banks;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.SavedAccounts;

public class SavedBankAccountDto : IMapFrom<SavedBankAccount>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tag { get; set; }
    public string Iban { get; set; }
    public string ReceiverName { get; set; }
    public Guid BankId { get; set; }
    public virtual BankDto Bank { get; set; }

    public static void Mapping(Profile profile)
    {
        profile.CreateMap<SavedBankAccount, SavedBankAccountDto>()
            .AfterMap((src, dest) => dest.Bank.LogoUrl = src.Bank.HasLogo
                                                         ? string.Format(BankConstInfo.BankLogoUrlPrefix, src.BankId)
                                                         : string.Empty);
    }
}