using LinkPara.PF.Application.Commons.Models.MerchantStatement;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Features.MerchantStatements;
using LinkPara.PF.Application.Features.Statements.Queries.GetMerchantStatementDetail;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantStatementService
{
    Task<MerchantStatement> CreateMerchantStatementExcelFileAsync(Merchant merchant, MerchantStatement merchantStatement, StatementDetails statementDetails);
    Task<MerchantStatement> CreateMerchantStatementPdfFileAsync(Merchant merchant, MerchantStatement merchantStatement, StatementDetails statementDetails);
    Task<PaginatedList<MerchantStatementDto>> GetPaginatedMerchantStatementsAsync(GetMerchantStatementDetailQuery request);
    Task<StatementDetails> GetStatementDetailsAsync(Merchant merchant, DateTime startDate, DateTime endDate);
}
