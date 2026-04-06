using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.Links
{
    public class LinkRequirementResponse
    {
        public bool Is3dRequired { get; set; }
        public List<int> AvailableInstallmentCounts { get; set; }
    }
}
