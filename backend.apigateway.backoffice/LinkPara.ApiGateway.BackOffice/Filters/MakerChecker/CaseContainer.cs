using Elasticsearch.Net;
using LinkPara.ApiGateway.BackOffice.Commons.Models.MakerCheckerModels;
using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.Cache;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Filters.MakerChecker;


public interface ICaseContainer
{
    Task<bool> IsApprovalNeededAsync(CaseCheckerRequest request);
    CaseDto GetCase(CaseCheckerRequest request);
    Task LoadAllCasesCacheAsync();
}

public class CaseContainer : ICaseContainer
{
    private readonly IApprovalHttpClient _approvalHttpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CaseContainer> _logger;

    public CaseContainer(IApprovalHttpClient approvalHttpClient, 
        ICacheService cacheService,
        ILogger<CaseContainer> logger)
    {
        _approvalHttpClient = approvalHttpClient;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> IsApprovalNeededAsync(CaseCheckerRequest request)
    {
        var cacheResult = GetCase(request);

        if (cacheResult == default)
        {
            await LoadAllCasesCacheAsync();
        }

        cacheResult = GetCase(request);

        return cacheResult is not null;
    }

    public CaseDto GetCase(CaseCheckerRequest request)
    {
        var method = request.Action.ToUpper();
        var baseUrlPath = $"{request.Path}_{method}";
        var actionNamePath = $"{request.ControllerName}/{request.ActionName}_{method}";

        var baseUrlCacheresult = _cacheService.Get<CaseDto>(baseUrlPath);
        var cacheResult = baseUrlCacheresult == default ? _cacheService.Get<CaseDto>(actionNamePath) : baseUrlCacheresult;

        return cacheResult;
    }

    public async Task LoadAllCasesCacheAsync()
    {
        try
        {
            await ClearCacheAsync();

            var cases = await _approvalHttpClient.GetActiveCasesAsync();
            if (cases == null)
            {
                return;
            }
            foreach (var item in cases.Items)
            {
                    _cacheService.Add(item.CaseType == CaseType.BaseUrl 
                                                ? $"{item.BaseUrl}_{item.Action.ToString().ToUpper()}"
                                                : $"{item.Resource}/{item.ActionName}_{item.Action.ToString().ToUpper()}"
                                                , item);   
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ErrorOnLoadAllCasesCacheAsync detail: \n{ex}");
            throw;
        }
    }

    private async Task ClearCacheAsync()
    {
        var cases = await _approvalHttpClient.GetAllCasesAsync(new GetFilterCaseRequest
        {
            RecordStatus = RecordStatus.Passive,
            Size = Int32.MaxValue
        });

        foreach (var item in cases.Items)
        {
            await _cacheService.RemoveAsync(item.CaseType == CaseType.BaseUrl
                                                ? $"{item.BaseUrl}_{item.Action.ToString().ToUpper()}"
                                                : $"{item.Resource}/{item.ActionName}_{item.Action.ToString().ToUpper()}");
        }
    }
}