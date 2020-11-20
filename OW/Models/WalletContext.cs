using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;


namespace OW.Models
{
    public class WalletContext : DbContext
    {
        public WalletContext(DbContextOptions<WalletContext> options)
             : base(options)
        {
        }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Currency>()
        //        .HasOne<User>(s => s.User)
        //        .WithMany(g => g.Currencies)
        //        .HasForeignKey(s => s.UserId);
        //}


        public DbSet<User> Users { get; set; }
        public DbSet<Currency> Currencies { get; set; }
    }
}
