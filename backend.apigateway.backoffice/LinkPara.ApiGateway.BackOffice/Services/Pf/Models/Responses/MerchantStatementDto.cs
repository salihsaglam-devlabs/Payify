using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantStatementDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string MailAddress { get; set; }
    public DateTime StatementStartDate { get; set; }
    public DateTime StatementEndDate { get; set; }
    public int StatementMonth { get; set; }
    public int StatementYear { get; set; }
    public string ExcelPath { get; set; }
    public string PdfPath { get; set; }
    public string FileName { get; set; }
    public MerchantStatementStatus StatementStatus { get; set; }
    public MerchantStatementType StatementType { get; set; }
    public string ReceiptNumber { get; set; }
    public DateTime CreateDate { get; set; }
}

