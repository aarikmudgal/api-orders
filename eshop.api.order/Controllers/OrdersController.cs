using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eshop.api.order.dal.DBContext;
using eshop.api.order.dal.Models;
using eshop.api.order.dal.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static eshop.api.order.dal.Services.OrderDBService;
using eshop.api.order.Kafka;

namespace eshop.api.order.Controllers
{
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly OrderContext _context;

        public bool DBDriven = true;

        IOrderService orderService;

        public OrdersController(OrderContext context)
        {
            _context = context;
            if (DBDriven)
            {
                orderService = new OrderDBService(_context);
            }
        }

        // GET api/Orders/health
        [HttpGet]
        [Route("health")]
        public IActionResult GetHealth(string health)
        {
            bool dbConnOk = false;
            string statusMessage = string.Empty;
            try
            {
                if (_context.CheckConnection())
                {
                    dbConnOk = true;
                    statusMessage = "Order Service is Healthy";
                }

            }
            catch (Exception ex)
            {
                statusMessage = $"Order database not available - {ex.Message}";

            }
            IActionResult response = dbConnOk ? Ok("Order Service is Healthy") : StatusCode(500, "Order database not available");
            return response;
        }

        // GET: api/Orders
        [HttpGet]
        public IActionResult GetOrders()
        {
            try
            {
                string customerId = Convert.ToString(Request.Headers["customerId"]);
                return new ObjectResult(orderService.GetOrders(customerId));
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error while getting order - {ex.Message}");
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public IActionResult GetOrder([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            var order = orderService.GetOrder(id, customerId);

            if (order == null)
            {
                return NotFound("The Order ID was not found");
            }

            JObject successobj = new JObject()
                {
                    { "StatusMessage", $"Order details fetched successfully for order id {id}" },
                    { "Order",  JsonConvert.SerializeObject(order, Formatting.Indented,
                                new JsonSerializerSettings() {
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                }
                            )
                        }
                };
            return Ok(successobj);
        }

        // POST: api/Orders
        [HttpPost]
        public IActionResult AddNewOrder([FromBody] Order order)
        {
            Order addedOrder;
            Order newOrder = null;
            string statusMessage;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string customerId = Convert.ToString(Request.Headers["customerId"]);
                if (customerId == null)
                {
                    return BadRequest("Customer Id missing in the header");
                }

                Order activeOrder = GetActiveOrderForCustomer(customerId);

                if (activeOrder == null)
                {
                    // create new order object for specific customer
                    orderService.InsertOrder(customerId, order, out addedOrder, out statusMessage);

                    // add new customer to list
                    newOrder = addedOrder;
                }
                else
                {
                    newOrder = activeOrder;
                    statusMessage = $"One Order already active for customer id { newOrder.CustomerId}";
                }

                if(newOrder.OrderedArticles == null)
                {
                    newOrder.OrderedArticles = new List<OrderedArticle>();
                }

                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "Order",  JsonConvert.SerializeObject(newOrder, Formatting.Indented,
                                new JsonSerializerSettings() {
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                }
                            )
                        }
                };
                return Ok(successobj);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + " Inner Exception- " + ex.InnerException.Message);
            }
        }

        private Order GetActiveOrderForCustomer(string customerId)
        {
            Order activeOrder = _context.Orders.Include(a => a.OrderedArticles).
                                Where(o => o.CustomerId == customerId && o.Status == Convert.ToInt32(OrderStatus.Active)).SingleOrDefault();
            return activeOrder;
        }


        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder([FromRoute] string id)
        {
            Order deletedOrder;
            string statusMessage;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var status = orderService.DeleteOrder(id, out deletedOrder, out statusMessage);
                if (deletedOrder == null)
                {
                    return NotFound($"Order with id {id} not found");
                }

                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "DeletedOrder", JObject.Parse(JsonConvert.SerializeObject(deletedOrder)) }
                };
                return Ok(successobj);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
                OrderedArticle article = JsonConvert.DeserializeObject<OrderedArticle>(value.ToString());
                var status = orderService.AddArticlesToOrder(customerId, orderid, article, out orderToUpdate, out string statusMessage);

                if (orderToUpdate == null)
                {
                    return NotFound($"Order with {orderid} for customer {customerId} not found");
                }

                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "UpdatedOrder", JsonConvert.SerializeObject(orderToUpdate, Formatting.Indented,
                                new JsonSerializerSettings() {
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                }
                            )
                        }
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
                var status = orderService.RemoveArticlesToOrder(orderid, articleid, customerId, out Order orderToUpdate, out string statusMessage);
                if (!status)
                {
                    return NotFound(statusMessage);
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "UpdatedOrder",  JsonConvert.SerializeObject(orderToUpdate, Formatting.Indented,
                                new JsonSerializerSettings() {
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                }
                            )
                        } 
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

        // PUT api/Orders/5 , ("{id}")
        [HttpPut]
        [Route("{orderid}/status/{status}")]
        public IActionResult UpdateOrderStatus(string orderid, string status)
        {
            // TODO: this can be used to update status
            Order updatedOrder;
            string customerId = Convert.ToString(Request.Headers["customerId"]);
            if (customerId == null)
            {
                return BadRequest("Customer Id missing in the header");
            }
            try
            {
                var updateStatus = orderService.UpdateOrderStatus(orderid, customerId, status, out updatedOrder, out string statusMessage);

                if (!updateStatus)
                {
                    return StatusCode(500, statusMessage);
                }
                else
                {
                    string articlesJson = GetArticlesJsonString(updatedOrder);

                    // push articles json to kafka
                    Task.Factory.StartNew(() => {
                        var producer = new OrderProducer();
                        producer.Produce(articlesJson);
                    });

                    

                    JObject successobj = new JObject()
                    {
                        { "StatusMessage", statusMessage },
                        { "UpdatedOrder", JsonConvert.SerializeObject(updatedOrder, Formatting.Indented,
                                new JsonSerializerSettings() {
                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                }
                            )
                        }
                    };
                    return Ok(successobj);
                }
            }
            catch (System.Exception ex)
            {
                // log error/exception
                return StatusCode(500, ex.Message);
            }
        }

        private string GetArticlesJsonString(Order order)
        {
            List<ArticleStock> articles = new List<ArticleStock>();

            foreach (var item in order.OrderedArticles)
            {
                articles.Add(new ArticleStock() { ArticleId = item.ArticleId, ArticleName = item.ArticleName, TotalQuantity = item.Quantity });
            }

            string json = JsonConvert.SerializeObject(articles, Formatting.Indented);
            return json;
        }
    }

}