

using LinkPara.HttpProviders.Receipt.Models;

namespace LinkPara.HttpProviders.Receipt;

public interface IReceiptService
{
    Task<ReceiptDto> GetReceiptByIdAsync(GetReceiptByIdRequest request);
    Task<CreateReceiptResponse> CreateReceiptAsync(CreateReceiptRequest request);
}