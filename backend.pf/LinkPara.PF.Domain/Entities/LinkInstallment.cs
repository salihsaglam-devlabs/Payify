using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Domain.Entities
{
    public class LinkInstallment : AuditEntity
    {
        public Guid LinkId { get; set; }
        public int Installment { get; set; }
        public Link Link { get; set; }
    }
}
