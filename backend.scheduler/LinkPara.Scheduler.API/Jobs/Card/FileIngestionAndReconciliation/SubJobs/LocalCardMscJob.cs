using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;
using LinkPara.SystemUser;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.SubJobs;

public class LocalCardMscJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalCardMscJob(IBus bus, IApplicationUserService applicationUserService)
        : base(bus, applicationUserService)
    {
    }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalCardMsc;
}