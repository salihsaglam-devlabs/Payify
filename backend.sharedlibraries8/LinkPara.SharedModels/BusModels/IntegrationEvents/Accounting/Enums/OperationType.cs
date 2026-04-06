namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

public enum OperationType
{
    EmoneyTransfer,
    Withdraw,
    Deposit,
    BillPayment,
    BillPaymentCancellation,
    BuyPin,
    PfBalance,
    PfBankBalance,
    CancelPin,
    CampaignPayment,
    CampaignCashback,
    ReturnCampaignCashback,
    ReturnCampaignPayment,
    PfCustomerInvoice,
    PfPosBlockage,
    PfPosUnBlockage,
    PfPosBlockageReturn,
    PaymentWithWallet,
    Cashback,
    PfCardTopupBalance,
    PfDeductionBalance
}
