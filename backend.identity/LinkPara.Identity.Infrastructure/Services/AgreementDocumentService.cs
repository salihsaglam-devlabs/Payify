using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Services
{
    public class AgreementDocumentService : IAgreementDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly  ILogger<AgreementDocumentService> _logger;

        public AgreementDocumentService(
            ApplicationDbContext applicationDbContext,
            ILogger<AgreementDocumentService> logger)
        {
            _context = applicationDbContext;
            _logger = logger;
        }

        public async Task UpdateAgreementDocument(AgreementDocument entity)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _context.AgreementDocument.Update(entity);
                    await _context.SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception exception)
                {
                    _logger.LogError($"AgreementDocumentUpdate Error: {exception}");
                }
            });
        }
    }
}
