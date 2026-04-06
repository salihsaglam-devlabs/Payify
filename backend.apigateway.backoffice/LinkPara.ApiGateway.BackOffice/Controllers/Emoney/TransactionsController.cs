using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using Microsoft.AspNetCore.Authorization;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;
using LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels;
using LinkPara.ApiGateway.BackOffice.Utils;
using AutoMapper;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class TransactionsController : ApiControllerBase
{
    private readonly ITransactionHttpClient _transactionHttpClient;
    private readonly IMapper _mapper;

    public TransactionsController(ITransactionHttpClient transactionHttpClient,
        IMapper mapper)
    {
        _transactionHttpClient = transactionHttpClient;
        _mapper = mapper;
    }

    [HttpGet("{Id}")]
    [Authorize(Policy = "Transaction:Read")]
    public async Task<TransactionAdminDto> GetTransactionAsync(Guid Id)
    {
        return await _transactionHttpClient.GetAdminTransactionAsync(Id);
    }

    [HttpGet("")]
    [Authorize(Policy = "Transaction:ReadAll")]
    public async Task<PaginatedList<TransactionAdminDto>> GetTransactionsAsync([FromQuery] GetTransactionsRequest request)
    {
        return await _transactionHttpClient.GetAdminTransactionsAsync(request);
    }

    /// <summary>
    /// Export transactions as Excel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("excel")]
    [Authorize(Policy = "Transaction:ReadAll")]
    public async Task<IActionResult> GetTransactionsExcelExportAsync(
        [FromQuery] GetTransactionsRequest request)
    {
        var response = await _transactionHttpClient.GetAdminTransactionsAsync(request);
        var excelExportModel = _mapper.Map<List<TransactionAdminExcelExportModel>>(response.Items);
        var excel = Excel.Instance.CreateExcelDocument(excelExportModel);
        return File(excel, "application/vnd.ms-excel", $"transactions-{DateTime.Now.ToShortDateString()}.xlsx");
    }
}