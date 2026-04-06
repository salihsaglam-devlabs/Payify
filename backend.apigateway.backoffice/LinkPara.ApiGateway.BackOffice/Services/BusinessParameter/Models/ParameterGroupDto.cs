using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class ParameterGroupDto
    {
        public Guid Id { get; set; }
        public string GroupCode { get; set; }
        public string Explanation { get; set; }
        public string RecordStatus { get; set; }
    }
}
