using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class ApiResponseCodeConfiguration : IEntityTypeConfiguration<ApiResponseCode>
{
    public void Configure(EntityTypeBuilder<ApiResponseCode> builder)
    {
        builder.Property(b => b.ResponseCode).IsRequired().HasMaxLength(10);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(256);
    }
}
