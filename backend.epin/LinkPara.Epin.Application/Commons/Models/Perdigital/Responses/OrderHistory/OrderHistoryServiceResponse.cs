
namespace LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.OrderHistory;

public class OrderHistoryServiceResponse : BaseServiceResponse
{
    public List<OrderHistoryServiceDto> order_history { get; set; }
}

public class OrderHistoryServiceDto
{
    public OrderHistoryProductsServiceDto Products { get; set; }
    public decimal Total { get; set; }
    public int order { get; set; }
    public string date { get; set; }
    public DateTime TransactionDate => DateTime.Parse(date);
}
public class OrderHistoryProductsServiceDto
{
    public List<OrderHistoryProductServiceDto> Product { get; set; }
}
public class PinDto
{
    public string Pin { get; set; }
}

public class OrderHistoryProductServiceDto
{
    public List<PinDto> Pins { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public int Product { get; set; }
    public decimal Discount { get; set; }
    public decimal unit_price { get; set; }
    public decimal Vat { get; set; }

}
