using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation.Persistence
{
    public class ReconciliationOperation : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid EvaluationId { get; set; }
        public virtual ReconciliationEvaluation Evaluation { get; set; }

        public Guid GroupId { get; set; }

        public int SequenceNumber { get; set; }

        public int? ParentSequenceNumber { get; set; }

        public string Code { get; set; }

        public string Note { get; set; }

        public string Payload { get; set; }

        public bool IsManual { get; set; }

        public string Branch { get; set; }

        public OperationStatus Status { get; set; }

        public string LeaseOwner { get; set; }

        public DateTime? LeaseExpiresAt { get; set; }

        public int RetryCount { get; set; }

        public int MaxRetries { get; set; } = 5;

        public DateTime? NextAttemptAt { get; set; }

        public string IdempotencyKey { get; set; }

        public string LastError { get; set; }
    }
}
