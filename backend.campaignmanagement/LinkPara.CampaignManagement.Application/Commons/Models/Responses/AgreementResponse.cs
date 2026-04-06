
namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class AgreementResponse : BaseResponse
{
    public List<Agreement>  Data { get; set; }
}

public class Agreement
{
    public string name { get; set; }
    public string short_name { get; set; }
    public string version { get; set; }
    public string pdf_file { get; set; }
    public string html_file { get; set; }
}
