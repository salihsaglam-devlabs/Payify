using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public enum SekerBankMethodType
{
    Inquiry,
    Payment,
    Cancellation,
    Reconciliation,
    Technical
}