using System;
using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Reconciliation
{
    public class ReconciliationOperation : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid EvaluationId { get; set; }
        public virtual ReconciliationEvaluation Evaluation { get; set; }

        public Guid GroupId { get; set; }

        public int SequenceIndex { get; set; }

        public int? ParentSequenceIndex { get; set; }

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
