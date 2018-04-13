using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.order.dal.ViewModel;
using eshop.api.order.dal.Models;
using eshop.api.order.dal.DBContext;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eshop.api.order.dal.Services
{
    public class OrderDBService : IOrderService
    {
        public enum OrderStatus
        {
            Active = 1,
            Pending,
            Submitted
        }

        private readonly OrderContext _context;
        public OrderDBService(OrderContext context)
        {
            _context = context;
            //CheckConnection();
        }
        private void CheckConnection()
        {
            try
            {
                _context.Database.GetDbConnection();
                _context.Database.OpenConnection();
            }
            catch (Exception ex)
            {
                // log db connectivity issue
                throw;
            }
        }


        public IEnumerable<Order> GetOrders(string customerId)
        {
            try
            {
                return _context.Orders.Include(a => a.OrderedArticles).Where(o => o.CustomerId == customerId); //Commented temporarily
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Order GetOrder(string orderId, string customerId)
        {
            try
            {
                return _context.Orders.Include(a => a.OrderedArticles).SingleOrDefault(o => o.OrderId== orderId && o.CustomerId== customerId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        
        public bool InsertOrder(string customerId, Order orderToBeInserted, out Order addedsOrdered, out string statusMessage)
        {
            try
            {
                orderToBeInserted.OrderId = Guid.NewGuid().ToString();
                orderToBeInserted.CustomerId = customerId;
                _context.Orders.Add(orderToBeInserted);
                int status = _context.SaveChanges();
                addedsOrdered = orderToBeInserted;
                statusMessage = "New Order added successfully";
                return true;
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                addedsOrdered = null;
                throw;
            }
        }


        public bool AddArticlesToOrder(string customerId, string orderId, OrderedArticle articlesOrdered, out Order addedArticlesOrdered, out string statusMessage)
        {
            //_context.Entry(articlesOrdered).State = EntityState.Modified;
            bool isExistingArticle;
            try
            {
                if (!OrderExists(orderId))
                {
                    addedArticlesOrdered = null;
                    statusMessage = $"The Order ID {orderId} does not exist.";
                }
                
                var order = _context.Orders.Include(a => a.OrderedArticles).SingleOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);


                // update the price and quantity if the article already exists in the order else add new article
                UpdateOrderArticles(ref order, articlesOrdered, out isExistingArticle);

                // update order total price
                UpdateOrderTotalPrice(ref order);

                //articlesOrdered.O = Guid.NewGuid().ToString();
                articlesOrdered.Order = order;
                if(!isExistingArticle)
                _context.OrderedArticles.Add(articlesOrdered);
                //_context.Orders.Add(order);


                //newArticle.ArticleId = Guid.NewGuid().ToString();
                _context.SaveChanges();
                statusMessage = $"Article was sucessfully updated in the Order for Order Id - {orderId}";
                addedArticlesOrdered = order;
                return true;
            }
            catch (DbUpdateConcurrencyException e)
            {
                statusMessage = e.Message;
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateOrderArticles(ref Order orderToUpdate, OrderedArticle article, out bool isExistingArticle)
        {
            OrderedArticle articleToUpdate = orderToUpdate.OrderedArticles.Where(a => a.ArticleId == article.ArticleId).FirstOrDefault();

            if (articleToUpdate == null)
            {
                article.TotalPrice = article.Quantity * article.ArticlePrice;
                orderToUpdate.OrderedArticles.Add(article);
                isExistingArticle = false;
            }
            else
            {
                // update quantity and total article price
                articleToUpdate.Quantity = articleToUpdate.Quantity + article.Quantity;
                articleToUpdate.TotalPrice = articleToUpdate.Quantity * articleToUpdate.ArticlePrice;
                isExistingArticle = true;
            }

        }

        private void UpdateOrderTotalPrice(ref Order orderToUpdate)
        {
            orderToUpdate.OrderTotalPrice = orderToUpdate.OrderedArticles.Sum(a => a.TotalPrice);
        }

        public bool RemoveArticlesToOrder(string orderId, int articleid,string customerId, out Order removeArticlesOrdered, out string statusMessage)
        {
            try
            {
                var order = _context.Orders.Include(a => a.OrderedArticles).SingleOrDefault(m => m.OrderId == orderId);
                if (order == null)
                {
                    removeArticlesOrdered = null;
                    statusMessage = $"Order with id - {orderId} not found";
                    return false;
                }
                else
                {
                    var article = order.OrderedArticles.Where(m =>  m.ArticleId== articleid);
                    //var article = _context.Orders.Where(o => o.OrderId == orderId);
                    if (article == null)
                    {
                        removeArticlesOrdered = null;
                        statusMessage = $"Article with id - {articleid} not found";
                        return false;
                    }
                    var articleTotalPrice = article.FirstOrDefault().TotalPrice;
                    _context.OrderedArticles.Remove(article.FirstOrDefault());
                    _context.SaveChanges();

                    removeArticlesOrdered = _context.Orders.Include(a => a.OrderedArticles).Where(m => m.OrderId == orderId && m.CustomerId == customerId).FirstOrDefault();

                    //Commented for now, need to update the totals in case item is deleted
                    // update order total price
                    _context.Orders.Include(a => a.OrderedArticles).Where(m => m.OrderId == orderId && m.CustomerId == customerId).FirstOrDefault().
                                OrderTotalPrice = removeArticlesOrdered.OrderTotalPrice - articleTotalPrice;
                    
                    _context.SaveChanges();

                    removeArticlesOrdered = _context.Orders.Include(a => a.OrderedArticles).Where(m => m.OrderId == orderId && m.CustomerId == customerId).FirstOrDefault();
                    statusMessage = $"Article with id - {articleid} deleted successfully";
                    return true;
                }
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                removeArticlesOrdered = null;
                throw e;
            }
            
        }

        public bool DeleteOrder(string orderId, out Order deletedArticlesOrdered, out string statusMessage)
        {
            try
            {
                var order = _context.Orders.Include(a => a.OrderedArticles).SingleOrDefault(m => m.OrderId == orderId);
                if (order == null)
                {
                    deletedArticlesOrdered = null;
                    statusMessage = $"Order with id - {orderId} not found";
                    return false;
                }

                var orderedArticles = _context.OrderedArticles.Where(o => o.OrderId== orderId);
                foreach (var item in orderedArticles)
                {
                    _context.OrderedArticles.Remove(item);
                }

                _context.Orders.Remove(order);
                _context.SaveChanges();
                deletedArticlesOrdered = order;
                statusMessage = $"Order with id - {orderId} deleted successfully";
                return true;
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                deletedArticlesOrdered = null;
                throw e;
            }
        }

        public bool UpdateOrderStatus(string orderId,string customerId, string status, out Order updatedOrder, out string statusMessage)
        {
            try
            {
                var order = _context.Orders.Where(o => o.OrderId == orderId && o.CustomerId == customerId && o.Status != Convert.ToInt32(status)).FirstOrDefault();
                if (order == null)
                {
                    statusMessage = $"Order with Id - {orderId} for customer {customerId} not found";
                    updatedOrder = null;
                    return false;
                }
                else
                {
                    updatedOrder = _context.Orders.Include(a => a.OrderedArticles).
                            Where(o => o.OrderId == orderId && o.CustomerId == customerId && o.Status != Convert.ToInt32(status)).FirstOrDefault();
                    updatedOrder.Status = Convert.ToInt32(status);
                    _context.SaveChanges();
                    statusMessage = $"Order with Id - {orderId} for customer {customerId} was updated Sucessfully";
                    return true;
                }
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                throw e;
            }
            
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
