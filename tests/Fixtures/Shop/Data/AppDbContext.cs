using System.Collections.Generic;
using Shop.Models;

namespace Shop.Data;

public class AppDbContext
{
    public List<Order> Orders { get; } = new();
    public List<Product> Products { get; } = new();

    public void SaveChanges()
    {
    }
}
