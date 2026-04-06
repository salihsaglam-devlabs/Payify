namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class UpdateParameterRequest
    {
        public Guid Id { get; set; }
        public string ParameterValue { get; set; }
        public List<UpdateParameterTemplateValueDto> ParameterTemplateValueList { get; set; }        
    }
}
