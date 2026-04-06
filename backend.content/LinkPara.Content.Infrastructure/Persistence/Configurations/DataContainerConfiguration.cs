using LinkPara.Content.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace LinkPara.Content.Infrastructure.Persistence.Configurations;

public class DataContainerConfiguration : IEntityTypeConfiguration<DataContainer>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DataContainer> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(25).IsRequired();
        builder.HasAlternateKey(x => x.Key);
    }
}

