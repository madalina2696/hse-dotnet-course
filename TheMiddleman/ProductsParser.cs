using System;
using System.Collections.Generic;
using System.IO;
using TheMiddleman.Entity;

class ProductsParser
{
    private string _filePath;

    public ProductsParser(string filePath)
    {
        _filePath = filePath;
    }

    private string ReadProductName(string line)
    {
        return line.Substring(8);
    }

    private int ReadProductDurability(string line)
    {
        return int.Parse(line.Substring(14));
    }

    public List<Product> ReadProducts()
    {
        string[] lines = File.ReadAllLines(_filePath);
        List<Product> products = new List<Product>();
        Product? currentProduct = null;
        int idCounter = 1;

        foreach (var line in lines)
        {
            if (line.StartsWith("- name: "))
            {
                string name = ReadProductName(line);
                currentProduct = new Product { Id = idCounter++, Name = name, Durability = 0 };
            }
            else if (line.StartsWith("  durability: "))
            {
                if (currentProduct != null)
                {
                    int durability = ReadProductDurability(line);
                    currentProduct.Durability = durability;
                }
            }
            else if (line.StartsWith("  baseprice: "))
            {
                int basePrice = int.Parse(line.Substring(13));
                if (currentProduct != null)
                {
                    currentProduct.BasePrice = basePrice;
                    products.Add(currentProduct);
                    currentProduct = null;
                }
            }
        }

        return products;
    }
}
