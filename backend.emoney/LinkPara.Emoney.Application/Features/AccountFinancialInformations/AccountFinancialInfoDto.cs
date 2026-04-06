using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.AccountFinancialInformations;

public class AccountFinancialInfoDto : IMapFrom<AccountFinancialInformation>
{
    public string IncomeSource { get; set; }
    public string IncomeInformation { get; set; }
    public string MonthlyTransactionVolume { get; set; }
    public string MonthlyTransactionCount { get; set; }
}
