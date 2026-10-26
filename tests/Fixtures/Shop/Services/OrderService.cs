using Shop.Data;
using Shop.Models;

namespace Shop.Services;

public class OrderService
{
    private readonly PaymentService _paymentService;
    private readonly AppDbContext _db;

    public OrderService(PaymentService paymentService, AppDbContext db)
    {
        _paymentService = paymentService;
        _db = db;
    }

    public void PlaceOrder(Order order)
    {
        // Charge must succeed before we save
        var paid = _paymentService.Charge(order.Total);
        if (paid)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }
    }
}
