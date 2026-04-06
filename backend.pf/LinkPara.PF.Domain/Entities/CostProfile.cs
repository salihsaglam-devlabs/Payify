using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CostProfile : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public PosType PosType { get; set; }
    public Guid? VposId { get; set; }
    public Vpos Vpos { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public PhysicalPos.PhysicalPos PhysicalPos { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public List<CostProfileItem> CostProfileItems { get; set; }
}
