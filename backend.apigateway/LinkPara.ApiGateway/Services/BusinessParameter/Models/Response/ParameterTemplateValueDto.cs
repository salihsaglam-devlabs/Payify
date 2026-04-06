namespace LinkPara.ApiGateway.Services.BusinessParameter.Models.Response
{
    public class ParameterTemplateValueDto
    {
        public Guid Id { get; set; }
        public string GroupCode { get; set; }
        public string ParameterCode { get; set; }
        public string ParameterValue { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateValue { get; set; }
    }
}
