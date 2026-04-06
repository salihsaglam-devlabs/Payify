using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantDevices.Queries.GetMerchantDeviceApiKeys;

public class GetMerchantDeviceApiKeysQuery : IRequest<MerchantDeviceApiKeyDto>
{
    public string PublicKeyEncoded { get; set; }
}

public class GetMerchantDeviceApiKeysQueryHandler : IRequestHandler<GetMerchantDeviceApiKeysQuery, MerchantDeviceApiKeyDto>
{
    private readonly IGenericRepository<MerchantDeviceApiKey> _repository;
    private readonly IGenericRepository<MerchantPhysicalDevice> _physicalDeviceRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<DeviceInventory> _deviceInventoryRepository;
    
    public GetMerchantDeviceApiKeysQueryHandler(
        IGenericRepository<MerchantDeviceApiKey> repository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantPhysicalDevice> physicalDeviceRepository, 
        IGenericRepository<DeviceInventory> deviceInventoryRepository)
    {
        _repository = repository;
        _merchantRepository = merchantRepository;
        _physicalDeviceRepository = physicalDeviceRepository;
        _deviceInventoryRepository = deviceInventoryRepository;
    }

    public async Task<MerchantDeviceApiKeyDto> Handle(GetMerchantDeviceApiKeysQuery request, CancellationToken cancellationToken)
    {
        var encodedBytes = Convert.FromBase64String(request.PublicKeyEncoded);
        var publicKey = System.Text.Encoding.UTF8.GetString(encodedBytes);
               
        var apiKeys = await _repository.GetAll()
            .FirstOrDefaultAsync(w => w.PublicKey == publicKey.Trim()
                , cancellationToken: cancellationToken);
        if (apiKeys is null)
        {
            throw new NotFoundException(nameof(MerchantDeviceApiKey), request.PublicKeyEncoded);
        }
        
        var physicalDevice = await _physicalDeviceRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == apiKeys.MerchantPhysicalDeviceId, cancellationToken: cancellationToken);
        if (physicalDevice is null)
        {
            throw new NotFoundException(nameof(MerchantPhysicalDevice), apiKeys.MerchantPhysicalDeviceId);
        }
        
        var merchant = await _merchantRepository.GetByIdAsync(physicalDevice.MerchantId);
        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), physicalDevice.MerchantId);
        }
        
        var deviceInventory = await _deviceInventoryRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == physicalDevice.DeviceInventoryId, cancellationToken: cancellationToken);

        return new MerchantDeviceApiKeyDto
        {
            PublicKey = apiKeys.PublicKey,
            PrivateKeyEncrypted = apiKeys.PrivateKeyEncrypted,
            MerchantId = merchant.Id,
            MerchantNumber = merchant.Number,
            SerialNumber = deviceInventory.SerialNo,
        };
    }
}