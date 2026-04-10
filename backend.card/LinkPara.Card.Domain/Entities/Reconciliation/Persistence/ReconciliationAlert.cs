using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation.Persistence
{
    public class ReconciliationAlert : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid GroupId { get; set; }

        public Guid EvaluationId { get; set; }
        public virtual ReconciliationEvaluation Evaluation { get; set; }

        public Guid OperationId { get; set; }
        public virtual ReconciliationOperation Operation { get; set; }

        public string Severity { get; set; }

        public string AlertType { get; set; }

        public string Message { get; set; }
        
        public AlertStatus AlertStatus { get; set; }
    }
}
