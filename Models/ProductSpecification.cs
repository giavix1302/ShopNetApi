namespace ShopNetApi.Models
{
    public class ProductSpecification
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string SpecName { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
    }
}
