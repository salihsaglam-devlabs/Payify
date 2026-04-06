namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models
{
    public class SaveParameterTemplateValueDto
    {
        public string GroupCode { get; set; }
        public string ParameterCode { get; set; }
        public string ParameterValue { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateValue { get; set; }
    }
}
