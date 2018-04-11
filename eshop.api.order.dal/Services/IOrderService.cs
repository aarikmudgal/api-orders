using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.order.dal.ViewModel;
using eshop.api.order.dal.Models;

namespace eshop.api.order.dal.Services
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOrders(string customerId);
        Order GetOrder(string orderId, string customerId);
        bool InsertOrder(string customerId, Order orderToBeInserted, out Order addedsOrdered, out string statusMessage);
        bool AddArticlesToOrder(string customerId, string orderId, OrderedArticle articlesOrdered, out Order addedArticlesOrdered, out string statusMessage);
        bool RemoveArticlesToOrder(string orderId, int articleid, string customerId, out Order addedArticlesOrdered, out string statusMessage);
        bool DeleteOrder(string orderId, out Order deletedArticlesOrdered, out string statusMessage);
        bool UpdateOrderStatus(string orderId,string customerId, string statusId, out Order updatedOrder, out string statusMessage);
    }
}
