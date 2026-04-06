namespace LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Order;

public class OrderServiceResponse : BaseServiceResponse
{
    public OrderServiceDto Order { get; set; }
}

public class OrderServiceDto
{
    public OrderProductsServiceDto Products { get; set; }
    public decimal Total { get; set; }
    public int Id { get; set; }
}
public class OrderProductsServiceDto
{
    public List<OrderProductServiceDto> Product { get; set; }
}
public class PinDto
{
    public string Pin { get; set; }
}

public class OrderProductServiceDto
{
    public List<PinDto> Pins { get; set; }
    public int Quantity { get; set; }
    public double Total { get; set; }
    public int Product { get; set; }
}
