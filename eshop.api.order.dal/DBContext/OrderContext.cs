using eshop.api.order.dal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace eshop.api.order.dal.DBContext
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {

        }
        
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedArticle> OrderedArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderedArticles)
                .WithOne(e => e.Order);
        }
        
        public void CheckConnection(out bool dbStatusOK)
        {
            try
            {

                this.Database.OpenConnection();
                this.Database.ExecuteSqlCommand("SELECT 1");
                this.Database.CloseConnection();
                dbStatusOK = true;
            }
            catch (Exception ex)
            {
                dbStatusOK = false;
                throw ex;
            }
            finally
            {
                this.Database.CloseConnection();
            }
        }
    }
}
