using System;
using LinkPara.SharedModels.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;

namespace LinkPara.Card.Domain.Entities.Reconciliation
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
