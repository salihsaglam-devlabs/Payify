using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface IFraudService
{
    Task<FraudCheckResponse> Resume(ResumeRequest request);
}
