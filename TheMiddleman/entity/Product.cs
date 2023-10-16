// Added namespace
namespace TheMiddleman.Entity
{
    public class Product
    {
        public int Id { get; set; }

        // Removed 'required'
        public required string Name { get; set; }

        public int Durability { get; set; }
    }
}
