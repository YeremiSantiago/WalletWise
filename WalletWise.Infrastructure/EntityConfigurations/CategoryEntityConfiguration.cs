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
    public class CategoryEntityConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            //Index

            builder.HasIndex(x => x.Name).
                HasDatabaseName("IDX_Category_Name")
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

        }
    }
}

