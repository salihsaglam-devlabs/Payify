using LinkPara.Fraud.Domain.Enums;

namespace LinkPara.Fraud.Application.Features.Searchs;

public class SearchByNameDto
{
    public string Name { get; set; }
    public SearchType SearchType { get; set; }
}
