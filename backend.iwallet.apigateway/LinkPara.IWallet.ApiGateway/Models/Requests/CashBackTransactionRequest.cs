using System.ComponentModel.DataAnnotations;

namespace LinkPara.IWallet.ApiGateway.Models.Requests
{
    public class CashBackTransactionRequest
    {
        public int oid { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; }
        public string status { get; set; }
        public decimal? vat_rate { get; set; }
        public decimal comsn_rate { get; set; }
        public int order_id { get; set; }
        public int card_id { get; set; }
        public int wallet_id { get; set; }
        public int? merchant_id { get; set; }
        public string? merchant_name { get; set; }
        public int? merchant_branch_id { get; set; }
        public string? merchant_branch_name { get; set; }
        public int pos_id { get; set; }
        public int customer_id { get; set; }
        public int customer_branch_id { get; set; }
        public string created_at { get; set; }
        public decimal comsn_amount { get; set; }
        public string? load_type { get; set; }
        public int? qr_code { get; set; }
        public string? ext_wallet_id { get; set; }
        public Guid? process_guid { get; set; }
    }
}
