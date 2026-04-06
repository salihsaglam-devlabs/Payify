using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Commons.Models.BankingModels.Configurations;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Banks;

public class BankDto : IMapFrom<Bank>
{
    public int Code { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }

    public static void Mapping(Profile profile)
    {
        profile.CreateMap<Bank, BankDto>()
            .ForMember(dest => dest.LogoUrl, opt
                => opt.MapFrom(s => s.HasLogo ? string.Format(BankConstInfo.BankLogoUrlPrefix, s.Id) : string.Empty));
    }
}