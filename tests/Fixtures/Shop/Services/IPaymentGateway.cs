namespace Shop.Services; 

public interface IPaymentGateway
{
    bool Charge(decimal amount);
}