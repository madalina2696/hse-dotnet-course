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

    private int ReadProductMinProductionRate(string line)
    {
        return int.Parse(line.Substring(20));
    }

    private int ReadBasePrice(string line)
    {
        return int.Parse(line.Substring(13));
    }

    private int ReadProductMaxProductionRate(string line)
    {
        return int.Parse(line.Substring(20));
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
                currentProduct = new Product { Id = idCounter++, Name = ReadProductName(line) };
            }
            else if (line.StartsWith("  durability: "))
            {
                if (currentProduct != null)
                {
                    currentProduct.Durability = ReadProductDurability(line);
                    products.Add(currentProduct);
                }
            }
            else if (line.StartsWith("  baseprice: "))
            {
                if (currentProduct != null)
                {
                    currentProduct.BasePrice = ReadBasePrice(line);
                }
            }
            else if (line.StartsWith("  minProductionRate: "))
            {
                if (currentProduct != null)
                {
                    currentProduct.MinProductionRate = ReadProductMinProductionRate(line);
                }
            }
            else if (line.StartsWith("  maxProductionRate: "))
            {
                if (currentProduct != null)
                {
                    currentProduct.MaxProductionRate = ReadProductMaxProductionRate(line);
                }
            }
        }
        return products;
    }
}
