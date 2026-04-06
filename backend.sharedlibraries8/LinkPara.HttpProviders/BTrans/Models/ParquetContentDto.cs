namespace LinkPara.HttpProviders.BTrans.Models;

public class ParquetContentDto : DocumentDto
{
    public List<Dictionary<string, string>> Items { get; set; }
}