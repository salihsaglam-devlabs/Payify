using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Tokens;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class CardTokenService : ICardTokenService
{
    private readonly ILogger<CardTokenService> _logger;
    private readonly IGenericRepository<CardToken> _cardTokenRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IDataEncryptionKeyService _dataEncryptionKeyService;
    private readonly PfDbContext _dbContext;

    public CardTokenService(ILogger<CardTokenService> logger,
        IGenericRepository<CardToken> cardTokenRepository,
        IDataEncryptionKeyService dataEncryptionKeyService,
        IEncryptionService encryptionService,
        PfDbContext dbContext)
    {
        _logger = logger;
        _cardTokenRepository = cardTokenRepository;
        _dataEncryptionKeyService = dataEncryptionKeyService;
        _encryptionService = encryptionService;
        _dbContext = dbContext;
    }

    public async Task DeleteTokenAsync(CardToken token)
    {
        try
        {
            _dbContext.CardToken.Remove(token);

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError($"DeleteToken Error {exception}");
        }
    }

    public async Task<CardToken> GetByToken(string token)
    {
        var cardTokenInfo = await _cardTokenRepository.
           GetAll().
           FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
           s.Token == token);

        if (cardTokenInfo is null)
        {
            throw new NotFoundException(nameof(CardToken), token);
        }

        return cardTokenInfo;
    }

    public async Task<CardInfoDto> GetCardDetailsAsync(CardToken token)
    {
        try
        {
            var dek = await _dataEncryptionKeyService.GetDataEncryptionKeyAsync();

            var cardInfoDto = new CardInfoDto
            {
                CardNumber = _encryptionService.Decrypt(token.CardNumberEncrypted, dek),
                Cvv = token.CvvEncrypted is not null ? _encryptionService.Decrypt(token.CvvEncrypted, dek) : string.Empty
            };

            var expiryString = _encryptionService.Decrypt(token.ExpireDateEncrypted, dek);

            var parts = expiryString.Split("/", StringSplitOptions.RemoveEmptyEntries);
            cardInfoDto.ExpireMonth = parts[0];
            cardInfoDto.ExpireYear = parts[1];

            return cardInfoDto;
        }
        catch (Exception exception)
        {
            _logger.LogError($"CardToken - GetCardDetails error : {exception}");
            throw;
        }
    }
}
