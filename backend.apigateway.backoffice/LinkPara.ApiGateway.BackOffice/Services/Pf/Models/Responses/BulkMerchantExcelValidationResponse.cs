namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class BulkMerchantExcelValidationResponse
{
    public bool IsValid { get; init; }
    public List<ExcelRowValidationError> Errors { get; set; }
}

public sealed class ExcelRowValidationError
{
    public int RowIndex { get; init; }
    public List<ExcelColumnValidationError> Errors { get; set; }
    
}

public sealed class ExcelColumnValidationError
{
    public string ColumnName { get; init; }
    public List<string> ErrorMessages { get; init; }
}