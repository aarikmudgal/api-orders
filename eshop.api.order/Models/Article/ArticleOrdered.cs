using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eshop.api.order.Models.Article
{
    public class ArticleOrdered
    {
        [Key]
        public string ArticleId { get; set; }
        public int Quantity { get; set; }
    }
}
