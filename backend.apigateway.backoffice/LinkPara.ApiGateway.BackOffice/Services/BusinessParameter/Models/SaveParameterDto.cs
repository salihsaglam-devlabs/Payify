namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class SaveParameterDto
    {
        public string GroupCode { get; set; }
        public string ParameterCode { get; set; }
        public string ParameterValue { get; set; }
        public List<ParameterTemplateValueRequest> ParameterTemplateValueList { get; set; }
    }
}
