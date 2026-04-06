

namespace LinkPara.SharedModels.Persistence
{
    public class EntityChangeLogModel
    {
        public DateTime LogDate { get; set; }
        public string ShemaName { get; set; }
        public string TableName { get; set; }
        public CrudOperationType CrudOperationType { get; set; }
        public string UserId { get; set; }
        public string ClientIpAddress { get; set; }
        public string ServiceName { get; set; }
        public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> OldValues { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> NewValues { get; set; } = new Dictionary<string, string>();
        public List<string> AffectedColumns { get; set; } = new List<string>();
        public string CorrelationId { get; set; }
    }
}
