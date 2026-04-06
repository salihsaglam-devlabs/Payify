using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantStatements.Queries.DownloadMerchantStatement;

public class DownloadMerchantStatementQuery : IRequest<IActionResult>
{
    public Guid Id { get; set; }
    public MerchantStatementType StatementType { get; set; }
}

public class DownloadMerchantStatementQueryHandler : IRequestHandler<DownloadMerchantStatementQuery,IActionResult>
{
    private readonly IGenericRepository<MerchantStatement> _merchantStatementRepository;

    public DownloadMerchantStatementQueryHandler(IGenericRepository<MerchantStatement> merchantStatementRepository)
    {
        _merchantStatementRepository = merchantStatementRepository;
    }
    public async Task<IActionResult> Handle(DownloadMerchantStatementQuery request, CancellationToken cancellationToken)
    {
        var statement = await _merchantStatementRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (statement is null)
        {
            throw new NotFoundException(nameof(MerchantStatement), request.Id);
        }

        var filePath = request.StatementType switch
        {
            MerchantStatementType.Excel => statement.ExcelPath,
            MerchantStatementType.PDF => statement.PdfPath,
            _ => statement.ExcelPath
        };
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = request.StatementType == MerchantStatementType.Excel
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";

        return new FileStreamResult(fileStream, contentType)
        {
            FileDownloadName = Path.GetFileName(filePath)
        };
    }
}
