using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Description).IsRequired();
        builder.Property(s => s.Summary).IsRequired();
        builder.Property(s => s.Image).IsRequired();
        builder.Property(s => s.Type).HasMaxLength(300).IsRequired();

        builder
         .HasOne(s => s.Publisher)
         .WithMany()
         .HasForeignKey(s => s.PublisherId)
         .HasPrincipalKey(s => s.Id)
         .OnDelete(DeleteBehavior.Restrict);
    }
}