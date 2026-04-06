namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi;

public enum SanctionScannerInfo
{
    SearchByName,
    SearchByIdentity,
    ExecuteTransaction,
    TransactionExists,
    TransactionDetail,
    CancelTransaction,
    AddMemoToSearch,
    MonitoringEnable,
    MonitoringDisable
}
