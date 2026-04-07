using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPinBlockService
{
    Task<SetCardBinResponse> GenerateEncryptedPinBlock(string clearPin, string clearCardNumber);
    Task<SetCardBinResponse> DecryptEncryptedPinBlock(string encryptedBlock, string clearCardNumber);
}