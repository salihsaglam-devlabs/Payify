namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.Models;

public class ParquetContentDto : BtransDocumentDto
{
    public List<Dictionary<string, string>> Items { get; set; }
}