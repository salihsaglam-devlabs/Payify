
using LinkPara.CampaignManagement.Application.Commons.Mappings;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;

namespace LinkPara.CampaignManagement.Application.Features.Agreements;

public class AgreementDto : IMapFrom<Agreement>
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Version { get; set; }
    public string PdfFile { get; set; }
    public string HtmlFile { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Agreement, AgreementDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.name))
            .ForMember(d => d.ShortName, opt => opt.MapFrom(s => s.short_name))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.version))
            .ForMember(d => d.PdfFile, opt => opt.MapFrom(s => s.pdf_file))
            .ForMember(d => d.HtmlFile, opt => opt.MapFrom(s => s.html_file));
    }

}
