using AutoMapper;
using LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Utils;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class WithdrawRequestsController : ApiControllerBase
{
    private readonly IWithdrawRequestHttpClient _withdrawHttpClient;
    private readonly IMapper _mapper;

    public WithdrawRequestsController(
        IWithdrawRequestHttpClient withdrawHttpClient, IMapper mapper)
    {
        _withdrawHttpClient = withdrawHttpClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns Withdraw Details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "WithdrawRequest:Read")]
    public async Task<ActionResult<WithdrawRequestDto>> GetByIdAsync(Guid id)
    {
        return await _withdrawHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns Withdraw Detail List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "WithdrawRequest:ReadAll")]
    public async Task<PaginatedList<WithdrawRequestAdminDto>> GetWithdrawRequestListAsync(
        [FromQuery] GetWithdrawRequestListRequest request)
    {
        return await _withdrawHttpClient.GetWithdrawRequestListAsync(request);
    }

    /// <summary>
    /// Returns withdraw request as Excel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("excel")]
    [Authorize(Policy = "WithdrawRequest:ReadAll")]
    public async Task<IActionResult> GetWithdrawRequestExcelAsync(
        [FromQuery] GetWithdrawRequestListRequest request)
    {
        var response = await _withdrawHttpClient.GetWithdrawRequestListAsync(request);
        var excelExportModel = _mapper.Map<List<WithdrawRequestExcelExportModel>>(response.Items);
        var excel = Excel.Instance.CreateExcelDocument(excelExportModel);
        return File(excel, "application/vnd.ms-excel", $"withdraw-requests-{DateTime.Now.ToShortDateString()}.xlsx");
    }
}