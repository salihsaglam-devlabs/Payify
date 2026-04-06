namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class BulkImportMerchantResponse
{
    public List<ImportRecord> ImportRecords { get; set; }
}

public class ImportRecord
{
    public bool IsSuccess { get; set; }
    public int RowIndex { get; set; }
    public string ErrorMessage { get; set; }
}