using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantTransactions.Command;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;
using LinkPara.PF.Domain.Entities;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class OrderNumberGeneratorService : IOrderNumberGeneratorService
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly ISecureRandomGenerator _randomGenerator;
    public OrderNumberGeneratorService(IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<Link> linkRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        ISecureRandomGenerator randomGenerator)
    {
        _merchantRepository = merchantRepository;
        _linkRepository = linkRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _randomGenerator = randomGenerator;
    }
    public async Task<OrderNumberResponse> GenerateAsync(GenerateOrderNumberCommand request)
    {

        return request.IsLinkOrderId ? await GenerateForLinkPaymentAsync(request)
            : await GenerateForManuelPaymentAsync(request);

    }

    public async Task<string> GenerateForBankTransactionAsync(int bankCode, string merchantNumber)
    {
        bool any;
        string orderNumber;
        do
        {
            var orderNumberInitial = string.Concat(bankCode, merchantNumber);
            if (orderNumberInitial.Length > 10)
                orderNumberInitial = orderNumberInitial[..10];
            orderNumber = string.Concat(orderNumberInitial, GenerateRandomNumber(20 - orderNumberInitial.Length));
            any = await _bankTransactionRepository.GetAll().AnyAsync(s => s.OrderId == orderNumber);
        }
        while (any);
        return orderNumber;
    }
    
    public async Task<string> GenerateForPhysicalPosTransactionAsync(int bankCode, string merchantNumber, string merchantName)
    {
        while (true)
        {
            var orderNumber = GenerateOrderId(bankCode, merchantNumber, merchantName);
            var exists = await _merchantTransactionRepository
                .GetAll()
                .AsNoTracking()
                .AnyAsync(x => x.ConversationId == orderNumber);
            if (!exists)
                return orderNumber;
        }
    }
    
    private static string GenerateOrderId(int bankCode, string merchantNumber, string merchantName)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = RandomNumberGenerator.GetInt32(int.MaxValue);
        var raw = $"{bankCode}|{merchantNumber}|{merchantName}|{timestamp}|{random}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        Span<char> digits = stackalloc char[20];
        var digitIndex = 0;
        var hashIndex = 0;
        while (digitIndex < 20)
        {
            if (hashIndex >= hash.Length)
            {
                hash = SHA256.HashData(hash);
                hashIndex = 0;
            }
            var b = hash[hashIndex++];
            if (b < 250)
            {
                digits[digitIndex++] = (char)('0' + (b % 10));
            }
        }
        return new string(digits);
    }
    
    private async Task<OrderNumberResponse> GenerateForManuelPaymentAsync(GenerateOrderNumberCommand request)
    {
        var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), request.MerchantId);
        }

        var any = false;
        var orderNumberResponse = new OrderNumberResponse();
        var random = new Random();
        if (request.OrderNumber is null)
        {
            do
            {
                orderNumberResponse.OrderNumber = string.Concat(CleanMerchantName(merchant.Name.Trim()), random.NextInt64().ToString("D19"));

                any = _merchantTransactionRepository.GetAll().Any(s => s.ConversationId == orderNumberResponse.OrderNumber);

                orderNumberResponse.IsSuccess = true;
            }
            while (any);
        }
        else
        {
            var orderNumberLength = request.OrderNumber.Length;
            orderNumberResponse.OrderNumber = request.OrderNumber;

            if (orderNumberLength != 24)
            {
                orderNumberResponse.OrderNumber = string.Concat(request.OrderNumber.Trim(),
                    GenerateRandomNumber(24 - orderNumberLength));
            }

            orderNumberResponse.IsSuccess = !_merchantTransactionRepository.GetAll()
                                .Any(s => s.ConversationId == orderNumberResponse.OrderNumber);

        }
        return orderNumberResponse;
    }

    private async Task<OrderNumberResponse> GenerateForLinkPaymentAsync(GenerateOrderNumberCommand request)
    {
        var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), request.MerchantId);
        }

        var any = false;
        var orderNumberResponse = new OrderNumberResponse();
        var random = new Random();

        if (request.OrderNumber is null)
        {
            do
            {
                orderNumberResponse.OrderNumber = string.Concat(CleanMerchantName(merchant.Name.Trim()), GenerateRandomNumber(5));

                any = _linkRepository.GetAll().Any(s => s.OrderId.Length >= 10 && s.OrderId.Substring(0, 10)
                == orderNumberResponse.OrderNumber);

                orderNumberResponse.IsSuccess = true;
            }
            while (any);
        }
        else
        {
            var orderNumberLength = request.OrderNumber.Length;
            orderNumberResponse.OrderNumber = request.OrderNumber;

            if (orderNumberLength != 24)
            {
                orderNumberResponse.OrderNumber = string.Concat(request.OrderNumber.Trim(),
                    GenerateRandomNumber(24 - orderNumberLength));
            }

            orderNumberResponse.IsSuccess = !_linkRepository.GetAll().Any(s => s.OrderId.Length >= 10 && s.OrderId.Substring(0, 10)
                        == orderNumberResponse.OrderNumber.Substring(0, 10));
        }
        return orderNumberResponse;
    }

    private string CleanMerchantName(string merchantName)
    {
        var normalized = merchantName
            .Replace("ı", "i")
            .Replace("İ", "I")
            .Replace("ğ", "g")
            .Replace("Ğ", "G")
            .Replace("ü", "u")
            .Replace("Ü", "U")
            .Replace("ö", "o")
            .Replace("Ö", "O")
            .Replace("ş", "s")
            .Replace("Ş", "S")
            .Replace("ç", "c")
            .Replace("Ç", "C");
        
        var trimMerchantName = normalized.Trim().Replace(" ", string.Empty);
        return new string(trimMerchantName
            .Where(char.IsLetterOrDigit)
            .Take(5)
            .ToArray());
    }
    private string GenerateRandomNumber(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        int minValue = 0;
        int maxValue = (int)Math.Pow(10, length) - 1;

        int randomNumber = Convert.ToInt32(_randomGenerator.GenerateSecureRandomNumber(minValue, maxValue));

        return randomNumber.ToString().Trim().PadLeft(length, '0');
    }
}
