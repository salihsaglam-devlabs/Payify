using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPostingBatchFactory
{
    Task TriggerBatchAsync(PostingBatchStatus batchStatus);
}