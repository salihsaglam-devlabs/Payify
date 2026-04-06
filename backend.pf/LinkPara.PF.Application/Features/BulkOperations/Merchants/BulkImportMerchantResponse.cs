namespace LinkPara.PF.Application.Features.BulkOperations.Merchants;

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