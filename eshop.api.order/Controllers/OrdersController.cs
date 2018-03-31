using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using eshop.api.order.Models;
using System;

namespace eshop.api.order.Controllers
{
    enum OrderStatus
    {
        Active = 1,
        Pending,
        Submitted
    }

    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private static List<Order> orders = null;

        static OrdersController()
        {
            LoadOrdersFromFile();
        }

        private static void LoadOrdersFromFile()
        {
            orders = new List<Order>(JsonConvert.DeserializeObject<List<Order>>(System.IO.File.ReadAllText(@"orders.json")));
        }

        private void WriteToFile()
        {
            try
            {
                System.IO.File.WriteAllText(@"orders.json", JsonConvert.SerializeObject(orders));
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("health")]
        public IActionResult GetHealth(string health)
        {
            bool fileExists = System.IO.File.Exists("./orders.json");
            IActionResult response = fileExists ? Ok("Service is Healthy") : StatusCode(500, "Orders file not available");
            return response;
        }

        // GET api/orders
        [HttpGet]
        public IActionResult GetOrders()
        {
            var customerId = Convert.ToString(Request.Query["customerId"]);
            var custSpecificOrders = customerId == null ? orders : orders.Where(o => o.CustomerId == customerId).ToList();
            return new ObjectResult(custSpecificOrders);
        }

        // GET api/orders/5
        [HttpGet("{orderId}")]
        public IActionResult GetOrderDetails(string orderId)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            Order order = orders.Find(o => o.OrderId == orderId && o.CustomerId == customerId);

            if(order == null)
            {
                return NotFound($"Order with Id - {orderId} and customer id - {customerId} not found");
            }

            JObject successobj = new JObject()
                {
                    { "StatusMessage", $"Order Details Fetched for Order Id {order.OrderId}" },
                    { "Order", JObject.Parse(JsonConvert.SerializeObject(order)) }
                };
            return Ok(successobj);            
        }

        // POST api/orders
        [HttpPost]
        public IActionResult AddNewOrder([FromBody]JObject value)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            Order newOrder = null;
            string responseMsg = string.Empty;
            try
            {
                Order activeOrder = GetActiveOrderForCustomer(customerId);

                if (activeOrder == null)
                {
                    // create new order object for specific customer
                    newOrder = JsonConvert.DeserializeObject<Order>(value.ToString());
                    newOrder.OrderId = Guid.NewGuid().ToString();

                    // add new customer to list
                    orders.Add(newOrder);
                    WriteToFile();
                    responseMsg = $"Order created successfully for customer id { newOrder.CustomerId}";
                }
                else
                {
                    newOrder = activeOrder;
                    responseMsg = $"One Order already active for customer id { newOrder.CustomerId}";
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", responseMsg },
                    { "OrderId", newOrder.OrderId.ToString() }
                };
                return Ok(successobj);

            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
          
        }

        private Order GetActiveOrderForCustomer(string customerId)
        {
            Order activeOrder = orders.Find(o => o.CustomerId == customerId && o.Status == Convert.ToInt32(OrderStatus.Active));
            return activeOrder;
        }

        [HttpPost]
        [Route("{orderid}/articles")]
        public IActionResult AddArticlesToOrder(string orderid, [FromBody]JObject value)
        {
            Order orderToUpdate = null;
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {
                // Add article to specific order object for specific customer
                Article article = JsonConvert.DeserializeObject<Article>(value.ToString());


                orderToUpdate = orders.Find(o => o.CustomerId == customerId && o.OrderId == orderid);

                if(orderToUpdate == null)
                {
                    return NotFound($"Order with {orderid} for customer {customerId} not found");
                }

                // update the price and quantity if the article already exists in the order else add new article
                UpdateOrderArticles(ref orderToUpdate, article);
                
                // update order total price
                UpdateOrderTotalPrice(ref orderToUpdate);
                
                WriteToFile();

                JObject successobj = new JObject()
                {
                    { "StatusMessage", $"Articles added successfully for order id {orderid} and customer id {customerId}" },
                    { "Order", JObject.Parse(JsonConvert.SerializeObject(orderToUpdate)) }
                };

                return Ok(successobj);
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
        }

        private void UpdateOrderTotalPrice(ref Order orderToUpdate)
        {
            orderToUpdate.OrderTotalPrice = orderToUpdate.Articles.Sum(a => a.TotalPrice);
        }

        private void UpdateOrderArticles(ref Order orderToUpdate, Article article)
        {
            Article articleToUpdate = orderToUpdate.Articles.Find(a => a.ArticleId == article.ArticleId);
            if (articleToUpdate == null)
            {
                article.TotalPrice = article.Quantity * article.ArticlePrice;
                orderToUpdate.Articles.Add(article);
            }
            else
            {
                // update quantity and total article price
                articleToUpdate.Quantity = articleToUpdate.Quantity + article.Quantity;
                articleToUpdate.TotalPrice = articleToUpdate.Quantity * articleToUpdate.ArticlePrice;
            }

        }
        

        [HttpDelete]
        [Route("{orderid}/articles/{articleid}")]
        public IActionResult RemoveArticleFromOrder(string orderid, int articleid)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {
                Order orderToRemoveArticleFrom = orders.Find(o => o.CustomerId == customerId && o.OrderId == orderid);

                Article article = orderToRemoveArticleFrom.Articles.Find(a => a.ArticleId == articleid);

                if (article == null)
                {
                    return NotFound($"Article id - {articleid} not found for order id - {orderid} and customerId - {customerId}");
                }
                // reduce the total order cost
                orderToRemoveArticleFrom.OrderTotalPrice = orderToRemoveArticleFrom.OrderTotalPrice - article.TotalPrice;
                orderToRemoveArticleFrom.Articles.Remove(article);

                WriteToFile();

                JObject successobj = new JObject()
                {
                    { "StatusMessage", $"Article with id {articleid} deleted successfully from order id {orderid} and customer id {customerId}" },
                    { "Order", JObject.Parse(JsonConvert.SerializeObject(orderToRemoveArticleFrom)) }
                };

                return Ok(successobj);
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpPut]
        //[Route("{orderid}/articles/{articleid}")]
        //public IActionResult UpdateArticleFromOrder(string orderid, string articleid, [FromBody]JObject value)
        //{
        //    string customerId = Convert.ToString(Request.Headers["customerId"]);
        //    if (customerId == null)
        //    {
        //        return BadRequest("Customer Id missing in the header");
        //    }

        //    try
        //    {
        //        Order orderToUpdateArticle = orders.Find(o => o.CustomerId == customerId && o.OrderId == orderid);

        //        Article article = orderToUpdateArticle.Articles.Find(a => a.ArticleId == articleid);
        //        if (article == null)
        //        {
        //            return NotFound($"Article with id - {articleid} not found for order - {orderid} and customer id- {customerId}");
        //        }
        //        int quantity;
        //        int.TryParse(value["Quantity"].ToString(), out quantity);
        //        article.Quantity = quantity;
        //        // TODO: increment quantiy and also update article price and total price accrodingly

        //        WriteToFile();
        //    }
        //    catch (System.Exception ex)
        //    {
        //        // log the exception
        //        // internal server errror
        //        return StatusCode(500, ex.Message);
        //    }
        //    return Ok($"Article with id {articleid} updated quantity successfully from order id {orderid} and customer id {customerId}");
        //}

        // PUT api/values/5
        [HttpPut("{orderid}")]
        public IActionResult UpdateOrderStatus(string orderid, JObject status)
        {
            // TODO: this can be used to update status
            
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {
                Order order = orders.Find(o => o.OrderId == orderid && o.CustomerId == customerId);

                if(order == null)
                {
                    return NotFound($"Order with Id - {orderid} for customer {customerId} not found");
                }
                var statusJson = JsonConvert.DeserializeObject<dynamic>(status.ToString());

                order.Status = statusJson.status;
                WriteToFile();
                return Ok($"Status updated successfully for Order with Id - {orderid}");
            }
            catch (System.Exception ex)
            {
                // log error/exception
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE api/order/5
        [HttpDelete("{orderid}")]
        public IActionResult DeleteOrder(string orderid)
        {
            try
            {
                Order order = orders.Find(x => x.OrderId == orderid);
                if (order == null)
                {
                    return NotFound($"Order with id {orderid} not found");
                }
                orders.Remove(order);
                WriteToFile();
                return Ok($"Order with Id - {orderid} deleted successfully");
            }
            catch (System.Exception ex)
            {
                // log the exception
                return StatusCode(500, ex.Message);
            }
        }
    }
}
