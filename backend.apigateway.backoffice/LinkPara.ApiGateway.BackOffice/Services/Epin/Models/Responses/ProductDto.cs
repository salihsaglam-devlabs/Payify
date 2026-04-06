namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public string Equivalent { get; set; }
    public string Vat { get; set; }
    public Guid PublisherId { get; set; }
    public Guid BrandId { get; set; }
}
