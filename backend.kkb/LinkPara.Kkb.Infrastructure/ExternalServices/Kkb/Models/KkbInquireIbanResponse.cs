
namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;


    public class IbanInquiryResult
    {
        public string OperationResult { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string AccountCurrency { get; set; }
        public List<string> TitleList { get; set; } = new List<string>();
    }

