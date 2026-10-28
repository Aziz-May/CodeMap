namespace Shop.Services;

public class PaymentService : IPaymentGateway
{   
    private int _successCount , _failureCount ;
    public bool Charge(decimal amount)
    {
        return amount > 0;
    }
}
