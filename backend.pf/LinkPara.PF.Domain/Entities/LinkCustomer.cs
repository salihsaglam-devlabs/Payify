

using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class LinkCustomer :AuditEntity
{
    public Guid LinkTransactionId { get; set; }
    public string Name{ get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Note { get; set; }

}
