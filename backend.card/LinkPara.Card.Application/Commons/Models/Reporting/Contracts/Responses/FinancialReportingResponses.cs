using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;

public class GetDailyTransactionVolumeResponse : ReconciliationResponseBase
{
    public PaginatedList<DailyTransactionVolumeDto> Data { get; set; }
}

public class GetMccRevenueConcentrationResponse : ReconciliationResponseBase
{
    public PaginatedList<MccRevenueConcentrationDto> Data { get; set; }
}

public class GetMerchantRiskHotspotsResponse : ReconciliationResponseBase
{
    public PaginatedList<MerchantRiskHotspotDto> Data { get; set; }
}

public class GetCountryCrossBorderExposureResponse : ReconciliationResponseBase
{
    public PaginatedList<CountryCrossBorderExposureDto> Data { get; set; }
}

public class GetResponseCodeDeclineHealthResponse : ReconciliationResponseBase
{
    public PaginatedList<ResponseCodeDeclineHealthDto> Data { get; set; }
}

public class GetSettlementLagAnalysisResponse : ReconciliationResponseBase
{
    public PaginatedList<SettlementLagAnalysisDto> Data { get; set; }
}

public class GetCurrencyFxDriftResponse : ReconciliationResponseBase
{
    public PaginatedList<CurrencyFxDriftDto> Data { get; set; }
}

public class GetInstallmentPortfolioSummaryResponse : ReconciliationResponseBase
{
    public PaginatedList<InstallmentPortfolioSummaryDto> Data { get; set; }
}

public class GetLoyaltyPointsEconomyResponse : ReconciliationResponseBase
{
    public PaginatedList<LoyaltyPointsEconomyDto> Data { get; set; }
}

public class GetClearingDisputeSummaryResponse : ReconciliationResponseBase
{
    public PaginatedList<ClearingDisputeSummaryDto> Data { get; set; }
}

public class GetClearingIoImbalanceResponse : ReconciliationResponseBase
{
    public PaginatedList<ClearingIoImbalanceDto> Data { get; set; }
}

public class GetHighValueUnmatchedTransactionsResponse : ReconciliationResponseBase
{
    public PaginatedList<HighValueUnmatchedTransactionDto> Data { get; set; }
}

