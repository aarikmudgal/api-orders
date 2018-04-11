using eshop.api.order.dal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


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
        //ModelBuilder
        public bool CheckConnection()
        {
            try
            {
                this.Database.OpenConnection();
                this.Database.CloseConnection();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
