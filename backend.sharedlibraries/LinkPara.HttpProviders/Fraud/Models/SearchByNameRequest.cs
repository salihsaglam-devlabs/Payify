using LinkPara.HttpProviders.Fraud.Models.Enums;

namespace LinkPara.HttpProviders.Fraud.Models;

public class SearchByNameRequest
{
    public string Name { get; set; }
    public string BirthYear { get; set; }
    public SearchType SearchType { get; set; }
}
