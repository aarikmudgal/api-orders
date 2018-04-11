using System;

namespace eshop.api.order.dal
{
    public class Class1
    {
    }


    //[Produces("application/json")]
    //[Route("api/Orders")]
    //public class OrdersController : Controller
    //{
    //    private readonly OrderContext _context;

    //    public OrdersController(OrderContext context)
    //    {
    //        _context = context;
    //    }

    //    // GET: api/Orders
    //    [HttpGet]
    //    public IEnumerable<Order> GetOrders()
    //    {
    //        return _context.Orders;
    //    }

    //    // GET: api/Orders/5
    //    [HttpGet("{id}")]
    //    public async Task<IActionResult> GetOrder([FromRoute] string id)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderId == id);

    //        if (order == null)
    //        {
    //            return NotFound();
    //        }

    //        return Ok(order);
    //    }

    //    // PUT: api/Orders/5
    //    [HttpPut("{id}")]
    //    public async Task<IActionResult> PutOrder([FromRoute] string id, [FromBody] Order order)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        if (id != order.OrderId)
    //        {
    //            return BadRequest();
    //        }

    //        _context.Entry(order).State = EntityState.Modified;

    //        try
    //        {
    //            await _context.SaveChangesAsync();
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            if (!OrderExists(id))
    //            {
    //                return NotFound();
    //            }
    //            else
    //            {
    //                throw;
    //            }
    //        }

    //        return NoContent();
    //    }

    //    // POST: api/Orders
    //    [HttpPost]
    //    public async Task<IActionResult> PostOrder([FromBody] Order order)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        _context.Orders.Add(order);
    //        await _context.SaveChangesAsync();

    //        return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
    //    }

    //    // DELETE: api/Orders/5
    //    [HttpDelete("{id}")]
    //    public async Task<IActionResult> DeleteOrder([FromRoute] string id)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderId == id);
    //        if (order == null)
    //        {
    //            return NotFound();
    //        }

    //        _context.Orders.Remove(order);
    //        await _context.SaveChangesAsync();

    //        return Ok(order);
    //    }

    //    private bool OrderExists(string id)
    //    {
    //        return _context.Orders.Any(e => e.OrderId == id);
    //    }
    //}
}
