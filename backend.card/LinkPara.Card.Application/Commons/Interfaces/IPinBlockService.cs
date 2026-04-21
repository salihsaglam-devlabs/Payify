using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPinBlockService
{
    Task<EncDecPinblockResponse> GenerateEncryptedPinBlock(string clearPin, string clearCardNumber);
    Task<EncDecPinblockResponse> DecryptEncryptedPinBlock(string encryptedBlock, string clearCardNumber);
}