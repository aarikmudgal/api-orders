using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.order.dal.Models;

namespace eshop.api.order.dal.ViewModel
{
    class ArticlesOrdered
    {
        public IEnumerable<Order> Order { get; set; }
        public IEnumerable<OrderedArticle> OrderedArticles { get; set; }
    }
}
