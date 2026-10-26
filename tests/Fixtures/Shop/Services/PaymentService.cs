namespace Shop.Services;

public class PaymentService
{
    public bool Charge(decimal amount)
    {
        return amount > 0;
    }
}
