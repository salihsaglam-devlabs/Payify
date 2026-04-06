using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.Property(s => s.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.AccountName).HasMaxLength(300);
        builder.Property(s => s.BankName).HasMaxLength(350).IsRequired();
        builder.Property(s => s.AccountTag).HasMaxLength(350).IsRequired().HasDefaultValue("{{BankAccountNumber}}");
    }
}