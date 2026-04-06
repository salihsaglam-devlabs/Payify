using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class DeleteParameterGroupRequest
    {
        public Guid Id { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
