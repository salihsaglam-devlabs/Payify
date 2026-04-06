using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.Links
{
    public class LinkResponse
    {
        public Guid Id { get; set; }
        public bool IsSuccess { get; set; }
        public string LinkUrl { get; set; }
    }
}
