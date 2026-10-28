using System.Collections.Generic;

namespace Shop.Models;

public class Order
{
    public OrderStatus Status {get; set;}
    public int Id { get; set; }
    public decimal Total { get; set; }
    public List<Product> Products { get; set; } = new();
}
