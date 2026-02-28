using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;

namespace WalletWise.Persistence.EntityConfigurations
{
    public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).
                HasPrecision(18, 2).
                HasColumnType("decimal(18,2)").
                IsRequired();

            builder.Property(x => x.Type).
                IsRequired().
                HasConversion<string>();

            builder.Property(x => x.CategoryId)
                .IsRequired();

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.WalletId)
                .IsRequired();

            //Relationships

            builder.HasOne(x => x.Category).
                WithMany(x => x.Transactions).
                HasForeignKey(x => x.CategoryId).
                OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wallet).
                WithMany(x => x.Transactions).
                HasForeignKey(x => x.WalletId).
                OnDelete(DeleteBehavior.Restrict);

            //Index

            builder.HasIndex(x => x.Type).
                HasDatabaseName("IDX_Transaction_Type");

        }
    }
}
