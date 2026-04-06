using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllDeviceApiKeys;

public class GetAllDeviceApiKeysQuery : IRequest<List<DeviceApiKeyDecryptedDto>>
{
    public Guid MerchantId { get; set; }
}

public class GetAllDeviceApiKeysQueryHandler : IRequestHandler<GetAllDeviceApiKeysQuery, List<DeviceApiKeyDecryptedDto>>
{
    private readonly IGenericRepository<MerchantPhysicalDevice> _physicalDeviceRepository;
    private readonly IVaultClient _vaultClient;
    private readonly IEncryptionService _encryptionService;

    public GetAllDeviceApiKeysQueryHandler(
        IGenericRepository<MerchantPhysicalDevice> physicalDeviceRepository, IVaultClient vaultClient, IEncryptionService encryptionService)
    {
        _physicalDeviceRepository = physicalDeviceRepository;
        _vaultClient = vaultClient;
        _encryptionService = encryptionService;
    }
    
    public async Task<List<DeviceApiKeyDecryptedDto>> Handle(GetAllDeviceApiKeysQuery request, CancellationToken cancellationToken)
    {
        var physicalDevices = await _physicalDeviceRepository.GetAll()
            .Include(s => s.Merchant)
            .Include(s => s.DeviceInventory)
            .Include(s => s.DeviceApiKeys)
            .Where(s => s.MerchantId == request.MerchantId).ToListAsync(cancellationToken: cancellationToken);
        if (physicalDevices.Count == 0)
        {
            throw new NotFoundException(nameof(MerchantPhysicalDevice), request.MerchantId);
        }
        
        var keyConstant =
            _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");

        return physicalDevices.Select(physicalDevice => new DeviceApiKeyDecryptedDto
        {
            MerchantId = physicalDevice.MerchantId,
            MerchantNumber = physicalDevice.Merchant.Number,
            SerialNumber = physicalDevice.DeviceInventory.SerialNo,
            PublicKey = physicalDevice.DeviceApiKeys.FirstOrDefault()!.PublicKey,
            PrivateKeyEncrypted = physicalDevice.DeviceApiKeys.FirstOrDefault()!.PrivateKeyEncrypted,
            PrivateKey = _encryptionService.Decrypt(physicalDevice.DeviceApiKeys.FirstOrDefault()!.PrivateKeyEncrypted, keyConstant)
        }).ToList();
    }
}