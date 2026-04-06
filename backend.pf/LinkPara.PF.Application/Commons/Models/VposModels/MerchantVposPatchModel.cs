using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.VposModels
{
    public class MerchantVposPatchModel
    {       
        public string VposId { get; set; }        
        public string SubMerchantCode { get; set; }
        public string TerminalNo { get; set; }
        public string Password { get; set; }
        public int Priority { get; set; }
        public string RecordStatus { get; set; }
        public string CreatedBy { get; set; }
    }
}
