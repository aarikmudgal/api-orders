using System.Collections.Generic;

namespace eshop.api.order.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public string TotalPrice { get; set; }
        public List<Article> Articles { get; set; }
    } 
}
