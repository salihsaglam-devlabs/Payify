using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Partners.Commands.CreatePartner;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.Emoney.Infrastructure.Services;

public class PartnerService : IPartnerService
{
    private const int PublicKeyLength = 16;
    private const int PrivateKeyLength = 32;

    private readonly ILogger<PartnerService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IEmailSender _emailSender;
    private readonly IVaultClient _vaultClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEncryptionService _encryptionService;
    private readonly ISecureKeyGenerator _secureKeyGenerator;

    public PartnerService(ILogger<PartnerService> logger,
        IContextProvider contextProvider,
        IEmailSender emailSender,
        IVaultClient vaultClient,
        IServiceScopeFactory scopeFactory,
        IEncryptionService encryptionService,
        ISecureKeyGenerator secureKeyGenerator)
    {
        _logger = logger;
        _contextProvider = contextProvider;
        _emailSender = emailSender;
        _vaultClient = vaultClient;
        _scopeFactory = scopeFactory;
        _encryptionService = encryptionService;
        _secureKeyGenerator = secureKeyGenerator;
    }

    public async Task CreatePartnerAsync(CreatePartnerCommand request)
    {
        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant",
            "WalletGatewayEncryptionKey");

        var publicKey = _secureKeyGenerator.Generate(PublicKeyLength);
        var privateKey = _secureKeyGenerator.Generate(PrivateKeyLength);
        var encryptPrivateKey = _encryptionService.Encrypt(privateKey, keyConstant);
        var partnerNumber = string.Empty;

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var duplicateCheck = await dbContext.Partner
            .AnyAsync(s =>
                s.Email == request.Email.ToLowerInvariant() &&
                s.RecordStatus == RecordStatus.Active);

        if (duplicateCheck)
        {
            throw new DuplicateRecordException();
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var createdBy = _contextProvider.CurrentContext.UserId.ToString();

                var partnerCounter = new PartnerCounter
                {
                    CreatedBy = createdBy,
                    RecordStatus = RecordStatus.Active,
                };

                dbContext.Add(partnerCounter);

                await dbContext.SaveChangesAsync();

                partnerNumber = $"{(partnerCounter.Index).ToString().PadLeft(8, '0')}";

                var apiKey = new ApiKey
                {
                    CreatedBy = createdBy,
                    RecordStatus = RecordStatus.Active,
                    PublicKey = publicKey,
                    PrivateKey = encryptPrivateKey,
                    Partner = new Partner
                    {
                        Email = request.Email.ToLowerInvariant(),
                        CreatedBy = createdBy,
                        Name = request.Name,
                        PartnerNumber = partnerNumber,
                        RecordStatus = RecordStatus.Active,
                        PhoneNumber = string.Concat(request.PhoneCode, request.PhoneNumber)
                    }
                };

                dbContext.Add(apiKey);

                await dbContext.SaveChangesAsync();

                scope.Complete();

                await SendMailAsync(request.Email, publicKey, privateKey, partnerNumber);
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreatePartnerError - {exception}");
            }
        });
    }

    private async Task SendMailAsync(string email, string publicKey, string privateKey, string partnerNumber)
    {
        await _emailSender.SendEmailAsync(new SendEmail
        {
            ToEmail = email,
            TemplateName = "PartnerApiKeys",
            DynamicTemplateData = new Dictionary<string, string>
                {
                    {"publickey", publicKey },
                    {"privatekey", privateKey },
                    {"partnernumber", partnerNumber },
                }
        });
    }
}
