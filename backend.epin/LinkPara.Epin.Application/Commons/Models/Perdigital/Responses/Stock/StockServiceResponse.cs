namespace LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Stock;

public class StockServiceResponse : BaseServiceResponse
{
    public Stock Stock { get; set; }
}

public class Stock
{
    public int Product { get; set; }
    public int Quantity { get; set; }

}