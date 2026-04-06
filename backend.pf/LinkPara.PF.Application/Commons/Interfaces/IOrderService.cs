using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IOrderService
{
    public Task<string> GenerateOrderNumberAsync(int bankCode);
}
