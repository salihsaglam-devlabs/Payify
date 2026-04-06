using LinkPara.HttpProviders.BTrans.Models;

namespace LinkPara.HttpProviders.BTrans;

public interface IBTransPosInformationService
{
    Task CreatePosInformationRecordsAsync(CreatePosInformationRecordsRequest request);
    Task DeletePosInformationRecordAsync(DeletePosInformationRecordRequest request);
}