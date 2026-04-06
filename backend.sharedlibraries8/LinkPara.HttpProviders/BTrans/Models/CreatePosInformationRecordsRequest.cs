using LinkPara.SharedModels.BusModels.Commands.BTrans;

namespace LinkPara.HttpProviders.BTrans.Models;

public class CreatePosInformationRecordsRequest
{
    public PosInformationReportList PosInformationReports { get; set; }
}