namespace LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;

public class ProductServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Unit_Price { get; set; }
    public decimal Discount { get; set; }
    public string Equivalent { get; set; }
    public string Vat { get; set; }
    public int ExternalPublisherId { get; set; }
    public int ExternalBrandId { get; set; }
}

