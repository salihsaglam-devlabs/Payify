using LinkPara.SharedModels.Persistence;

namespace LinkPara.Content.Domain.Entities;

public class DataContainer : AuditEntity
{
   public string Key { get; set; }
   public string Value { get; set; }
}

