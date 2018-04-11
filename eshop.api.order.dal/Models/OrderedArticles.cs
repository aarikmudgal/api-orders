using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace eshop.api.order.dal.Models
{
    [Table("OrderedArticles")]
    public class OrderedArticle
    {
        [Key]
        public int ArticleId { get; set; }
        public string OrderId { get; set; }
        public string ArticleName { get; set; }
        public string ArticleDescription { get; set; }
        public double ArticlePrice { get; set; }
        public double TotalPrice { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        
        public Order Order { get; set; }
    }
}
