using System.Collections.Generic;

namespace TheMiddleman.Entity
{
    public class Intermediary
    {
        public string? Name { get; set; }
        public string? Company { get; set; }
        public int AccountBalance { get; set; }
        public List<Product>? Products { get; set; }
    }
}
