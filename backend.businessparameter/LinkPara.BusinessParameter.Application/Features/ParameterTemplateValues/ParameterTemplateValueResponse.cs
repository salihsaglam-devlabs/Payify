using LinkPara.BusinessParameter.Application.Commons.Mappings;
using LinkPara.BusinessParameter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues
{
    public class ParameterTemplateValueResponse : IMapFrom<ParameterTemplateValue>
    {
        public Guid Id { get; set; }
        public string GroupCode { get; set; }
        public string ParameterCode { get; set; }
        public string ParameterValue { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateValue { get; set; }
    }
}
