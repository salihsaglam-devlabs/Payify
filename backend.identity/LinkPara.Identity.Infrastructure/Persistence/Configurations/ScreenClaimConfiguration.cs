using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class ScreenClaimConfiguration : IEntityTypeConfiguration<ScreenClaim>
    {
        public void Configure(EntityTypeBuilder<ScreenClaim> builder)
        {

        }
    }
}