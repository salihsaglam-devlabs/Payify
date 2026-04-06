using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Kkb.Domain.Entities;

public class AccountIban : AuditEntity
{
    public string IdentityNo { get; set; }
    public string Iban { get; set; }
}
