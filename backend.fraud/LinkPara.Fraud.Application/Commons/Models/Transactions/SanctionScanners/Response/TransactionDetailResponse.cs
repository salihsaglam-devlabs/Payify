using LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Request;
using LinkPara.HttpProviders.Fraud.Models;

namespace LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Response;

public class TransactionDetailResponse : BaseResponse
{
    public object ExtraInfo { get; set; }
    public ExecuteTransactionApiRequest Result { get; set; }
}
