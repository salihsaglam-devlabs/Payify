
namespace LinkPara.HttpProviders.Receipt.Models
{
    public class CreateReceiptResponse
    {
        public long ReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
