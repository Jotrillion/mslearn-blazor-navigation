using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Data;

public class PizzaStoreContext : DbContext
{
    public PizzaStoreContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<PizzaSpecial> Specials { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Pizza> Pizzas { get; set; }
    public DbSet<PizzaTopping> PizzaToppings { get; set; }
    public DbSet<Topping> Toppings { get; set; }
}
