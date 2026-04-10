namespace TetPee.Service.Order;

public class Request
{
    public class CreateOrderRequest
    {
        public string Address { get; set; }
        public List<ProductOrderRequest> Products { get; set; }
    }

    public class ProductOrderRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}