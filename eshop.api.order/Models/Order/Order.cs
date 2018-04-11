using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshop.api.order.Models.Order
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public List<eshop.api.order.Models.Article.ArticleOrdered> ArticleOrdered { get; set; }
    }
}
