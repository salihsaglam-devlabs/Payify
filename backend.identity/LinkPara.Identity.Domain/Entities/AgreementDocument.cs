using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class AgreementDocument : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public string LastVersion { get; set; }
    public string LanguageCode { get; set; }
    public List<AgreementDocumentVersion> Agreements { get; set; }
    public ProductType ProductType { get; set; }
}