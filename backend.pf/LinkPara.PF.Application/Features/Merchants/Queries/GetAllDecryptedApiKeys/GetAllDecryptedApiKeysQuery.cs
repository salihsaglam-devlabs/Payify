using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.Security;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetAllDecryptedApiKeys;

public class GetAllDecryptedApiKeysQuery : IRequest<List<MerchantApiKeyDto>>
{
}

public class GetAllDecryptedApiKeysQueryHandler : IRequestHandler<GetAllDecryptedApiKeysQuery,List<MerchantApiKeyDto>>
{
    private readonly IGenericRepository<MerchantApiKey> _repository;
    private readonly IVaultClient _vaultClient;
    private readonly IEncryptionService _encryptionService;
    
    public GetAllDecryptedApiKeysQueryHandler(
        IGenericRepository<MerchantApiKey> repository,
        IVaultClient vaultClient, 
        IEncryptionService encryptionService)
    {
        _repository = repository;
        _vaultClient = vaultClient;
        _encryptionService = encryptionService;
    }

    public async Task<List<MerchantApiKeyDto>> Handle(GetAllDecryptedApiKeysQuery request, CancellationToken cancellationToken)
    {
        var apiKeys = await _repository
            .GetAll()
            .Include(s => s.Merchant)
            .ToListAsync(cancellationToken);
        
        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");

        var merchantApiKeyDtoList = apiKeys.Select(s => new MerchantApiKeyDto
        {
            Id = s.Id,
            MerchantId = s.MerchantId,
            MerchantNumber = s.Merchant.Number,
            PublicKey = s.PublicKey,
            PrivateKey = _encryptionService.Decrypt(s.PrivateKeyEncrypted, keyConstant),
            PrivateKeyEncrypted = s.PrivateKeyEncrypted
        }).ToList();
        
        return merchantApiKeyDtoList;
    }
}