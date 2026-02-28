using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Constants;
using WalletWise.Domain.Entities;

namespace WalletWise.Persistence.Seeds
{
    public static class DataSeeder
    {
        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = DefaultUser.Id,
                    Username = "userDefault1"
                }

            );



        }
    }
}
