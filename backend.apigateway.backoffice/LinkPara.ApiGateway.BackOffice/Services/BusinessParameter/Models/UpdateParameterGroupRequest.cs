using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class UpdateParameterGroupRequest
    {
        public Guid Id { get; set; }
        public string GroupCode { get; set; }
        public string Explanation { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
