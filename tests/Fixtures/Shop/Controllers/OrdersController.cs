using Shop.Models;
using Shop.Services;

namespace Shop.Controllers;

public class OrdersController
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    public void Create(Order order)
    {
        _orderService.PlaceOrder(order);
    }
}
