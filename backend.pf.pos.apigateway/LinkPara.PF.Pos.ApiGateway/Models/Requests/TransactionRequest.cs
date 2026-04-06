namespace LinkPara.PF.Pos.ApiGateway.Models.Requests;

public class TransactionRequest
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public long Date { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public int Amount { get; set; }
    public int PointAmount { get; set; }
    public int Installment { get; set; }
    public string MaskedCardNo { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string AcquirerResponseCode { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string PosEntryMode { get; set; }
    public string PinEntryInfo { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
}

public class TransactionMerchantRequest : TransactionRequest
{
    public TransactionMerchantRequest(TransactionRequest request)
    {
        PaymentId = request.PaymentId;
        BatchId = request.BatchId;
        Date = request.Date;
        Type = request.Type;
        Status = request.Status;
        Currency = request.Currency;
        MerchantId = request.MerchantId;
        TerminalId = request.TerminalId;
        Amount = request.Amount;
        PointAmount = request.PointAmount;
        Installment = request.Installment;
        MaskedCardNo = request.MaskedCardNo;
        BinNumber = request.BinNumber;
        ProvisionNo = request.ProvisionNo;
        AcquirerResponseCode = request.AcquirerResponseCode;
        InstitutionId = request.InstitutionId;
        Vendor = request.Vendor;
        Rrn = request.Rrn;
        Stan = request.Stan;
        PosEntryMode = request.PosEntryMode;
        PinEntryInfo = request.PinEntryInfo;
        BankRef = request.BankRef;
        OriginalRef = request.OriginalRef;
    }

    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}