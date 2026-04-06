using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantEmailConfiguration : IEntityTypeConfiguration<MerchantEmail>
{
    public void Configure(EntityTypeBuilder<MerchantEmail> builder)
    {
        builder.Property(b => b.Email).IsRequired().HasMaxLength(256);
    }
}
