using LinkPara.Card.Application.Commons.Models.PaycoreModels;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPinBlockService
{
    Task<PaycoreResponse> GenerateEncryptedPinBlock(string clearPin, string clearCardNumber);
    Task<PaycoreResponse> DecryptEncryptedPinBlock(string encryptedBlock, string clearCardNumber);
}