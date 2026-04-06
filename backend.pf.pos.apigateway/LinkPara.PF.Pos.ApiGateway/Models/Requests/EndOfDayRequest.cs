namespace LinkPara.PF.Pos.ApiGateway.Models.Requests;

public class EndOfDayRequest
{
    public string BatchId { get; set; }
    public string BankId { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public int Date { get; set; }
    public int SaleCount { get; set; }
    public int VoidCount { get; set; }
    public int RefundCount { get; set; }
    public int InstallmentSaleCount { get; set; }
    public int SaleAmount { get; set; }
    public int VoidAmount { get; set; }
    public int RefundAmount { get; set; }
    public int InstallmentSaleAmount { get; set; }
    public string Currency { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
}

public class EndOfDayMerchantRequest : EndOfDayRequest
{
    public EndOfDayMerchantRequest(EndOfDayRequest request)
    {
        BatchId = request.BatchId;
        BankId = request.BankId;
        MerchantId = request.MerchantId;
        TerminalId = request.TerminalId;
        Date = request.Date;
        SaleCount = request.SaleCount;
        VoidCount = request.VoidCount;
        RefundCount = request.RefundCount;
        InstallmentSaleCount = request.InstallmentSaleCount;
        SaleAmount = request.SaleAmount;
        VoidAmount = request.VoidAmount;
        RefundAmount = request.RefundAmount;
        InstallmentSaleAmount = request.InstallmentSaleAmount;
        Currency = request.Currency;
        InstitutionId = request.InstitutionId;
        Vendor = request.Vendor;
    }

    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}