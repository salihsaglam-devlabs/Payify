using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface IAccountFinancialInformationService
{
    Task CreateAccountFinancialInfoAsync(CreateAccountFinancialInfoRequest request);
}
