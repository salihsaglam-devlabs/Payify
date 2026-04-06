using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.SharedModels.Persistence;
using LinkPara.ApiGateway.BackOffice.Commons.Mappings;

namespace LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels
{
    public class TransactionAdminExcelExportModel : IMapFrom<TransactionAdminDto>
    {
        public string TransactionType { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionStatus { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal Amount { get; set; }
        public string Tag { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? ReceiverBankCode { get; set; }
        public string ReceiverName { get; set; }
        public int? SenderBankCode { get; set; }
        public string SenderName { get; set; }
    }
}
