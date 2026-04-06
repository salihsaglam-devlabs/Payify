using System;
using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Reconciliation
{
    public class ReconciliationOperationExecution : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid GroupId { get; set; }

        public Guid EvaluationId { get; set; }
        public virtual ReconciliationEvaluation Evaluation { get; set; }

        public Guid OperationId { get; set; }
        public virtual ReconciliationOperation Operation { get; set; }

        public int AttemptNumber { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public ExecutionStatus Status { get; set; }

        public string RequestPayload { get; set; }

        public string ResponsePayload { get; set; }

        public string ResultCode { get; set; }

        public string ResultMessage { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
