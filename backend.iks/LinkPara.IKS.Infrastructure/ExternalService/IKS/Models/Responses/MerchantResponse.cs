

namespace LinkPara.IKS.Infrastructure.ExternalService.IKS.Models.Responses
{
    public class MerchantResponse
    {
        public int PspNo { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime LastStatusUpdateDate { get; set; }
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; }
    }
    public class AdditionalInfo
    {
        /// <summary>
        /// MIV: Merkezi İşyeri Veritabanı.
        //MTV: Merkezi Terminal Veritabanı.
        //FTS: Fesih Takip Sistemi.
        //ATM: ATM Veritabanı
        /// </summary>
        public string AppCode { get; set; }
        public int Code { get; set; }
        public string Description { get; set; }
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}
