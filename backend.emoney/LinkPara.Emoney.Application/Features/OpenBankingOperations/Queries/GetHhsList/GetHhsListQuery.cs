using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsList;
public class GetHhsListQuery : IRequest<HhsResultDto>
{

}

public class GetHhsListQueryHandler : IRequestHandler<GetHhsListQuery, HhsResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetHhsListQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<HhsResultDto> Handle(GetHhsListQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetHhsListAsync();
    }
}
