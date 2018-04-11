using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eshop.api.order.Models.Article
{
    public class ArticleOrderedContext : DbContext
    {
        public ArticleOrderedContext(DbContextOptions<ArticleOrderedContext> options) : base(options)
        {

        }
        public DbSet<ArticleOrdered> ArticleOrdereds { get; set; }
    }
}
