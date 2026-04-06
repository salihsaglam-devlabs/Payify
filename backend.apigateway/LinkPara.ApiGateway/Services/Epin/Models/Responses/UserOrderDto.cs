namespace LinkPara.ApiGateway.Services.Epin.Models.Responses;

public class UserOrderDto
{
    public Guid Id { get; set; }
    public string Equivalent { get; set; }
    public DateTime CreateDate { get; set; }
    public string Pin { get; set; }
    public decimal UnitPrice { get; set; }
    public string Image { get; set; }
}
