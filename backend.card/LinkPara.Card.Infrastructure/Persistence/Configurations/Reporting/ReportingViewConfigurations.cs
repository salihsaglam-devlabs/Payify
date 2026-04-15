using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations.Reporting;


public class ReconciliationTransactionViewConfiguration : IEntityTypeConfiguration<ReconciliationTransactionDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationTransactionDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_reconciliation_matched_pair", "reporting");
    }
}

public class ReconciliationSummaryDailyViewConfiguration : IEntityTypeConfiguration<ReconciliationSummaryDailyDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationSummaryDailyDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_reconciliation_summary_daily", "reporting");
    }
}

public class ReconciliationSummaryByNetworkViewConfiguration : IEntityTypeConfiguration<ReconciliationSummaryByNetworkDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationSummaryByNetworkDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_reconciliation_summary_by_network", "reporting");
    }
}

public class ReconciliationSummaryByFileViewConfiguration : IEntityTypeConfiguration<ReconciliationSummaryByFileDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationSummaryByFileDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_reconciliation_summary_by_file", "reporting");
    }
}

public class ReconciliationSummaryOverallViewConfiguration : IEntityTypeConfiguration<ReconciliationSummaryOverallDto>
{
    public void Configure(EntityTypeBuilder<ReconciliationSummaryOverallDto> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_reconciliation_summary_overall", "reporting");
    }
}

