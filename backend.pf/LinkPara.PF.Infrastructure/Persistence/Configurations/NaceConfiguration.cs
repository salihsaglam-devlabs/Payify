using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class NaceConfiguration : IEntityTypeConfiguration<Nace>
{
    public void Configure(EntityTypeBuilder<Nace> builder)
    {
        builder.Property(n => n.SectorCode).HasMaxLength(2).IsRequired();
        builder.Property(n => n.SectorDescription).HasMaxLength(300).IsRequired();
        builder.Property(n => n.ProfessionCode).HasMaxLength(10).IsRequired();
        builder.Property(n => n.ProfessionDescription).HasMaxLength(300).IsRequired();
        builder.Property(n => n.Code).HasMaxLength(10).IsRequired();
        builder.Property(n => n.Description).HasMaxLength(800).IsRequired();
    }
}