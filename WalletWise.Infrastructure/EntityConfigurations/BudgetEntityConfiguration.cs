using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;

namespace WalletWise.Infrastructure.EntityConfigurations
{
    public class BudgetEntityConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.Mode)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            //Relationship

            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Budgets)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Budgets)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
