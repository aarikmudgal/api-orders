using System;
using System.Collections.Generic;
using System.Text;

namespace eshop.api.order.dal.Models
{
    public class ArticleStock
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; }
        public int TotalQuantity { get; set; }
    }
}
