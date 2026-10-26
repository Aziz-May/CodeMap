using Shop.Controllers;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop;

public static class Program
{
    public static void Main()
    {
        var db = new AppDbContext();
        var payment = new PaymentService();
        var orderService = new OrderService(payment, db);
        var controller = new OrdersController(orderService);

        controller.Create(new Order { Id = 1, Total = 50m });
    }
}
