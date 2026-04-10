using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation.Persistence
{
    public class ReconciliationEvaluation : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid GroupId { get; set; }

        public EvaluationStatus Status { get; set; }

        public string Message { get; set; }

        public int CreatedOperationCount { get; set; }
    }
}
