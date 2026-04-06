using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPostingBatch<in TModel> where TModel : class
{
    Task ExecuteBatchAsync(PostingBatchStatus batchStatus);
}