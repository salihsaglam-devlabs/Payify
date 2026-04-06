using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceById;

public class GetPostingBalanceByIdQuery : SearchQueryParams, IRequest<PostingBalanceDto>
{
    public Guid Id { get; set; }
}

public class GetPostingBalanceByIdQueryHandler : IRequestHandler<GetPostingBalanceByIdQuery, PostingBalanceDto>
{
    private readonly IPostingBalanceService _postingBalanceService;

    public GetPostingBalanceByIdQueryHandler(IPostingBalanceService postingBalanceService)
    {
        _postingBalanceService = postingBalanceService;
    }
    public async Task<PostingBalanceDto> Handle(GetPostingBalanceByIdQuery request, CancellationToken cancellationToken)
    {
        return await _postingBalanceService.GetByIdAsync(request.Id);
    }
}
