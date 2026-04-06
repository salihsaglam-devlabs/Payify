using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.BusinessParameter.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.BusinessParameter.Domain.Entities;

public class ParameterTemplate : AuditEntity, ITrackChange  
{
    public string GroupCode { get; set; }
    public string TemplateCode { get; set; }
    public ParameterDataType DataType { get; set; }
    public int DataLength { get; set; }
    public string Explanation { get; set; }
    public ParameterGroup ParameterGroup { get; set; }
}