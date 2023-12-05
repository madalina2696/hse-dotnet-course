
namespace TheMiddleman.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Durability { get; set; }
        public double BasePrice { get; set; }
        public int MinProductionRate { get; set; }
        public int MaxProductionRate { get; set; }
        public int Availability { get; set; } = 0;
        public double SellingPrice => Math.Round(BasePrice * 0.8);
        public double BuyingPrice { get; set; }
    }
}