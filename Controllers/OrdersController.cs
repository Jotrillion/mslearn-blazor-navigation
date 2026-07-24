using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly PizzaStoreContext _db;

    public OrdersController(PizzaStoreContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderWithStatus>>> GetOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .OrderByDescending(o => o.CreatedTime)
            .ToListAsync();

        return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
    }

    [HttpGet("latest")]
    public async Task<ActionResult<OrderWithStatus>> GetLatestOrder()
    {
        var order = await _db.Orders
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .OrderByDescending(o => o.CreatedTime)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return OrderWithStatus.FromOrder(order);
    }

    [HttpPost]
    public async Task<ActionResult<int>> PostOrder([FromBody] Order order)
    {
        if (order == null || order.Pizzas == null)
        {
            return BadRequest("Order is required.");
        }

        order.CreatedTime = DateTime.UtcNow;
        order.UserId = "guest";

        foreach (var pizza in order.Pizzas)
        {
            if (pizza == null)
            {
                continue;
            }

            if (pizza.Special != null)
            {
                pizza.SpecialId = pizza.Special.Id;
            }

            pizza.Toppings ??= new List<PizzaTopping>();
            pizza.Special = null;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(order.OrderId);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderWithStatus>> GetOrderWithStatus(int orderId)
    {
        var order = await _db.Orders
            .Where(o => o.OrderId == orderId)
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .SingleOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return OrderWithStatus.FromOrder(order);
    }
}
