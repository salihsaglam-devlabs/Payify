using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.UploadMerchantLogo;

public class UploadMerchantLogoCommand : IRequest
{
    public MerchantLogoDto MerchantLogo { get; set; }
}

public class UploadMerchantLogoCommandHandler : IRequestHandler<UploadMerchantLogoCommand>
{
    private readonly IGenericRepository<MerchantLogo> _merchantLogoRepository;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<UploadMerchantLogoCommandHandler> _logger;

    public UploadMerchantLogoCommandHandler(
        IGenericRepository<MerchantLogo> merchantLogoRepository, 
        IContextProvider contextProvider,
        ILogger<UploadMerchantLogoCommandHandler> logger)
    {
        _merchantLogoRepository = merchantLogoRepository;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(UploadMerchantLogoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var merchantLogo = await _merchantLogoRepository.GetAll()
                .Where(x => 
                    x.RecordStatus == RecordStatus.Active && 
                    x.MerchantId == command.MerchantLogo.MerchantId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        
            if (merchantLogo is not null)
            {
                merchantLogo.RecordStatus = RecordStatus.Passive;
                merchantLogo.LastModifiedBy = parseUserId.ToString();
                merchantLogo.UpdateDate = DateTime.Now;

                await _merchantLogoRepository.UpdateAsync(merchantLogo);
            }

            await _merchantLogoRepository.AddAsync(new MerchantLogo
            {
                MerchantId = command.MerchantLogo.MerchantId,
                Bytes = command.MerchantLogo.Bytes,
                FileName = command.MerchantLogo.FileName,
                ContentType = command.MerchantLogo.ContentType,
                CreatedBy = parseUserId.ToString()
            });
        
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "UploadMerchantLogoError : {Exception}", exception);
            throw;
        }
    }
}