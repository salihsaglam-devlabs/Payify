using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Domain.Entities;

public class ParameterTemplateValue : AuditEntity, ITrackChange 
{
    public string GroupCode { get; set; }
    public string ParameterCode { get; set; }
    public string TemplateCode { get; set; }
    public string TemplateValue { get; set; }
    public ParameterGroup ParameterGroup { get; set; }
    public Parameter Parameter { get; set; }
}