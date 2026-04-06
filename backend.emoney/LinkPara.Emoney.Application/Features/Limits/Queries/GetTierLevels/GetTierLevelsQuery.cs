using AutoMapper;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevels;

public class GetTierLevelsQuery : IRequest<List<TierLevelDto>>
{
    public string CurrencyCode { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public bool IncludeCustoms { get; set; }
}

public class GetTierLevelsQueryHandler : IRequestHandler<GetTierLevelsQuery, List<TierLevelDto>>
{
    private readonly ITierLevelService _tierLevelService;

    public GetTierLevelsQueryHandler(ITierLevelService tierLevelService)
    {
        _tierLevelService = tierLevelService;
    }

    public async Task<List<TierLevelDto>> Handle(GetTierLevelsQuery request, CancellationToken cancellationToken)
    {
        return await _tierLevelService.GetTierLevelsQueryAsync(request);
    }
}