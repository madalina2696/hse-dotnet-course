
namespace TheMiddleman.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Durability { get; set; }
        public int BasePrice { get; set; }
        public int MinProductionRate { get; set; }
        public int MaxProductionRate { get; set; }
        public int Availability { get; set; } = 0; // fängt mit 0 an
        public double MaxAvailability { get; set; } // Die maximale Verfügbarkeit des Produkts
        public int OriginalBasePrice { get; set; }
        public int SellingPrice => (int)Math.Round(BasePrice * 0.8);
        public int BuyingPrice { get; set; }
    }
}