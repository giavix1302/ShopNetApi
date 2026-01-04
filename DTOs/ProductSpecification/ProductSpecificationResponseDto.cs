namespace ShopNetApi.DTOs.ProductSpecification
{
    public class ProductSpecificationResponseDto
    {
        public long Id { get; set; }
        public string SpecName { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
    }
}
