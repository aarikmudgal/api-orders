using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace eshop.api.order.dal.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public int Status { get; set; }
        public double OrderTotalPrice { get; set; }
        //public List<OrderedArticle> OrderedArticles { get; set; }
        public ICollection<OrderedArticle> OrderedArticles { get; set; }
    }
}
