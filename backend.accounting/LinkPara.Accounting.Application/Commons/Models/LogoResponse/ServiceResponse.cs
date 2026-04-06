
namespace LinkPara.Accounting.Application.Commons.Models.LogoResponse;

public class ServiceResponse
{
    public bool isSuccess { get; set; }
    public string strMesage { get; set; }
    public LinkList[] linkList { get; set; }
}
