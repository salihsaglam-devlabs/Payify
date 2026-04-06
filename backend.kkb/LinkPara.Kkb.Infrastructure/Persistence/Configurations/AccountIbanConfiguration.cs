using LinkPara.Kkb.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Kkb.Infrastructure.Persistence.Configurations
{
    internal class AccountIbanConfiguration : IEntityTypeConfiguration<AccountIban>
    {
        public void Configure(EntityTypeBuilder<AccountIban> builder)
        {
            builder.Property(t => t.Iban);
            builder.Property(t => t.IdentityNo);

            builder.HasIndex(t => new { t.Iban, t.IdentityNo });
        }
    }
}
