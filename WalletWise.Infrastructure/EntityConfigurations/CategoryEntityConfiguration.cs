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

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.Description)
                .HasMaxLength(250)
                .IsRequired(false);

            //Index

            builder.HasIndex( o => new {o.Name, o.UserId}).
                HasDatabaseName("IDX_Category_Name")
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

        }
    }
}

