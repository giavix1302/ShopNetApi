namespace ShopNetApi.Models
{
    public class OrderItem
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public bool IsReviewed { get; set; } = false;
    }
}
