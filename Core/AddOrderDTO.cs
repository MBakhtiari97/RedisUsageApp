namespace Core;

public class AddOrderDTO
{
    public Core.Order Order { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}