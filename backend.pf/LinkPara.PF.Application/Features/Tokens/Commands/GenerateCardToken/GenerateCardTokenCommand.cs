using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.Security;
using LinkPara.SharedModels.Persistence;
using MassTransit.Initializers;
using MediatR;

namespace LinkPara.PF.Application.Features.Tokens.Commands.GenerateCardToken;

public class GenerateCardTokenCommand : IRequest<CardTokenDto>
{
    public string CardNumber { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string Cvv { get; set; }
    public Guid MerchantId { get; set; }
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
}

public class GenerateCardTokenCommandHandler : IRequestHandler<GenerateCardTokenCommand, CardTokenDto>
{
    private const int CardTokenLength = 32;

    private readonly ISecureKeyGenerator _keyGenerator;
    private readonly IGenericRepository<CardToken> _tokenRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IDataEncryptionKeyService _dataEncryptionKeyService;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<Merchant> _merchantRepository;

    public GenerateCardTokenCommandHandler(IGenericRepository<CardToken> tokenRepository,
        ISecureKeyGenerator keyGenerator,
        IEncryptionService encryptionService,
        IDataEncryptionKeyService dataEncryptionKeyService,
        IAuditLogService auditLogService,
        IVaultClient vaultClient,
        IGenericRepository<Merchant> merchantRepository)
    {
        _tokenRepository = tokenRepository;
        _keyGenerator = keyGenerator;
        _encryptionService = encryptionService;
        _dataEncryptionKeyService = dataEncryptionKeyService;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _merchantRepository = merchantRepository;
    }

    public async Task<CardTokenDto> Handle(GenerateCardTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenExpiryInMinutes = _vaultClient.GetSecretValue<string>("PFSecrets", "CardTokenConfig", "TokenExpiryInMinutes");

        var token = _keyGenerator.Generate(CardTokenLength);

        var dek = await _dataEncryptionKeyService.GetDataEncryptionKeyAsync();
        
        var cardNumber = request.CardNumber.Replace(" ", "");
        var expireMonth = request.ExpireMonth.Replace(" ", "");
        var expireYear = request.ExpireYear.Replace(" ", "");

        var cardNumberEncrypted = _encryptionService.Encrypt(cardNumber, dek);
        var expireDateEncrypted = _encryptionService.Encrypt($"{expireMonth}/{expireYear}", dek);
        string cvvEncrypted = null;
        if (string.IsNullOrEmpty(request.Cvv))
        {
            var merchantCvvApprove = await _merchantRepository.GetByIdAsync(request.MerchantId).Select(b => b.IsCvvPaymentAllowed);
            if (merchantCvvApprove)
            {
                return new CardTokenDto
                {
                    CardToken = null,
                    Signature = request.Signature,
                    ConversationId = request.ConversationId
                };
            }
        }
        else
        {
            if (request.Cvv.Replace(" ", "").Length < 3)
            {
                return new CardTokenDto
                {
                    CardToken = null,
                    Signature = request.Signature,
                    ConversationId = request.ConversationId
                };
            }
            cvvEncrypted = _encryptionService.Encrypt(request.Cvv.Replace(" ", ""), dek);
        }

        await _tokenRepository.AddAsync(new CardToken
        {
            Token = token,
            ExpiryDate = DateTime.Now.AddMinutes(Convert.ToDouble(tokenExpiryInMinutes)),
            MerchantId = request.MerchantId,
            CreateDate = DateTime.Now,
            CreatedBy = request.MerchantId.ToString(),
            RecordStatus = RecordStatus.Active,
            CardNumberEncrypted = cardNumberEncrypted,
            ExpireDateEncrypted = expireDateEncrypted,
            CvvEncrypted = cvvEncrypted,
        });

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "GenerateCardToken",
                SourceApplication = "PF",
                Resource = "CardToken",
                Details = new Dictionary<string, string>
                {
                    {"Token", token},
                    {"MerchantId", request.MerchantId.ToString()}
                }
            });

        return new CardTokenDto
        {
            CardToken = token,
            Signature = request.Signature,
            ConversationId = request.ConversationId
        };
    }
}