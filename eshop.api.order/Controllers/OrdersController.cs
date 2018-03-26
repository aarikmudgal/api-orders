using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using eshop.api.order.Models;
using System;

namespace eshop.api.order.Controllers
{
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

            return new ObjectResult(order);
            
        }

        // POST api/values
        [HttpPost]
        public IActionResult AddNewOrder([FromBody]JObject value)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            Order newOrder = null;
            try
            {
                // create new order object for specific customer
                newOrder = JsonConvert.DeserializeObject<Order>(value.ToString());
                newOrder.OrderId = Guid.NewGuid().ToString();

                // add new customer to list
                orders.Add(newOrder);
                WriteToFile();
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
            JObject successobj = new JObject()
            {
                { "StatusMessage", $"Order created successfully for customer id {newOrder.CustomerId}" },
                { "NewOrderId", newOrder.OrderId.ToString() }
            };
            return Ok(successobj);
        }

        [HttpPost]
        [Route("{id}/articles")]
        public IActionResult AddArticlesToOrder(string id, [FromBody]JArray value)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {
                // Add articles to specific order object for specific customer
                List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(value.ToString());

                Order orderToUpdate = orders.Find(o => o.CustomerId == customerId && o.OrderId == id);
                orderToUpdate.Articles.AddRange(articles);
                
                WriteToFile();
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
            return Ok($"Articles added successfully for order id {id} and customer id {customerId}");
        }

        [HttpDelete]
        [Route("{orderid}/articles/{articleid}")]
        public IActionResult RemoveArticleFromOrder(string orderid, string articleid)
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
                orderToRemoveArticleFrom.Articles.Remove(article);

                WriteToFile();
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
            return Ok($"Article with id {articleid} deleted successfully from order id {orderid} and customer id {customerId}");
        }

        [HttpPut]
        [Route("{orderid}/articles/{articleid}")]
        public IActionResult UpdateArticleFromOrder(string orderid, string articleid, [FromBody]JObject value)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {
                Order orderToUpdateArticle = orders.Find(o => o.CustomerId == customerId && o.OrderId == orderid);

                Article article = orderToUpdateArticle.Articles.Find(a => a.ArticleId == articleid);
                if (article == null)
                {
                    return NotFound($"Article with id - {articleid} not found for order - {orderid} and customer id- {customerId}");
                }
                int quantity;
                int.TryParse(value["Quantity"].ToString(), out quantity);
                article.Quantity = quantity;

                WriteToFile();
            }
            catch (System.Exception ex)
            {
                // log the exception
                // internal server errror
                return StatusCode(500, ex.Message);
            }
            return Ok($"Article with id {articleid} updated quantity successfully from order id {orderid} and customer id {customerId}");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]JObject value)
        {
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }

            try
            {

                int index = orders.IndexOf(orders.Find(x => x.OrderId == id));
                orders.Remove(orders.Find(x => x.OrderId == id));
                orders.Insert(index, JsonConvert.DeserializeObject<Order>(value.ToString()));
                WriteToFile();
            }
            catch (System.Exception ex)
            {
                // log error/exception
                return StatusCode(500, ex.Message);
            }
            return Ok($"Order with Id - {id} updated successfully");            
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
            }
            catch (System.Exception ex)
            {
                // log the exception
                return StatusCode(500, ex.Message);
            }
            return Ok($"Order with Id - {orderid} deleted successfully");            
        }
    }
}
