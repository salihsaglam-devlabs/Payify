using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Reconciliation.Persistence
{
    public class ReconciliationReview : AuditEntity
    {
        public Guid Id { get; set; }

        public Guid FileLineId { get; set; }
        public virtual IngestionFileLine IngestionFileLine { get; set; }

        public Guid GroupId { get; set; }

        public Guid EvaluationId { get; set; }
        public virtual ReconciliationEvaluation Evaluation { get; set; }

        public Guid OperationId { get; set; }
        public virtual ReconciliationOperation Operation { get; set; }

        public Guid? ReviewerId { get; set; }

        public ReviewDecision Decision { get; set; } = ReviewDecision.Pending;

        public string Comment { get; set; }

        public DateTime? DecisionAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public ReviewExpirationAction ExpirationAction { get; set; } = ReviewExpirationAction.Cancel;

        public ReviewExpirationFlowAction ExpirationFlowAction { get; set; } = ReviewExpirationFlowAction.Continue;
    }
}
