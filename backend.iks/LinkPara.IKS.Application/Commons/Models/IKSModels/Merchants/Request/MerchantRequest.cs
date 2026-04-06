using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.IKS.Application.Commons.Mappings;
using LinkPara.IKS.Application.Features.Merchant.Command.SaveMerchant;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request
{
    public class MerchantRequest : IMapFrom<SaveMerchantCommand>
    {
        public Guid MerchantId { get; set; }
        public string PspMerchantId { get; set; }
        /// <summary>
        ///0: Açık işyeri.
        ///1: Kapalı işyeri.
        /// </summary>
        public string StatusCode { get; set; }
        public string TaxNo { get; set; }
        public string TradeName { get; set; }
        public string MerchantName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        /// <summary>
        /// Semt
        /// </summary>
        public string Neighborhood { get; set; }
        /// <summary>
        /// Plaka kodu
        /// </summary>
        public int LicenseTag { get; set; }
        public string CountryCode { get; set; }
        /// <summary>
        /// isyerinin bulundugu ulke numerik ISO kodu
        /// </summary>
        public int Mcc { get; set; }
        public string ManagerName { get; set; }
        public string AgreementDate { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// Isyerinin kendisinin bir Ödeme Hizmeti Sağlayıcısı olup olmadığı 
        //bilgisidir.
        //0: ÖHS işyeri değil.
        //1: ÖHS işyeri.
        /// </summary>
        public string PspFlag { get; set; }
        /// <summary>
        /// İşyerinin bayilik yapısı olup olmadığı bilgisidir.
        //0: Tekil işyeri.
        //1: Ana bayi işyeri.
        //2: Alt bayi işyeri.
        /// </summary>
        public string MainSellerFlag { get; set; }
        /// <summary>
        /// işyerinin bağlı olduğu ana bayi TCKN ya da Vergi Numarası 
        // bilgisidir.
        // Yurt içi işyerleri için TCKN ise 11 karakter, VKN ise 10 karakter
        // uzunluğunda geçerli bir değer olmalıdır.Kıbrıs ya da yurt dışı
        // işyerleri için 1-11 karakter uzunluğunda olmalıdır.
        // Ana Bayi Ayracı (mainSellerFlag) değerinin “2 - Alt bayi işyeri”
        //olarak iletilmesi halinde dolu gönderilmeli, aksi takdirde boş
        // gönderilmelidir. 
        /// </summary>
        public string MainSellerTaxNo { get; set; }
        public string ZipCode { get; set; }
        public string TaxOfficeName { get; set; }
        public string CommercialType { get; set; }
        public string EstablishmentDate { get; set; }
        public string BusinessModel { get; set; }
        public string BusinessActivity { get; set; }
        public int BranchCount { get; set; }
        public int EmployeeCount { get; set; }
        public string ManagerBirthDate { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public List<MerchantBusinessPartnerRequest> Partners { get; set; }
    }
}
