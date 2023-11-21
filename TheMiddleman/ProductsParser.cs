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
                SetProductDurability(currentProduct, line, products);
            }
            else if (line.StartsWith("  baseprice: "))
            {
                SetProductBasePrice(currentProduct, line);
            }
            else if (line.StartsWith("  minProductionRate: "))
            {
                SetProductMinProductionRate(currentProduct, line);
            }
            else if (line.StartsWith("  maxProductionRate: "))
            {
                SetProductMaxProductionRate(currentProduct, line);
            }
        }
        return products;
    }

    private void SetProductDurability(Product? product, string line, List<Product> products)
    {
        if (product != null)
        {
            product.Durability = ReadProductDurability(line);
            products.Add(product);
        }
    }

    private void SetProductBasePrice(Product? product, string line)
    {
        if (product != null)
        {
            product.BasePrice = ReadBasePrice(line);
        }
    }

    private void SetProductMinProductionRate(Product? product, string line)
    {
        if (product != null)
        {
            product.MinProductionRate = ReadProductMinProductionRate(line);
        }
    }

    private void SetProductMaxProductionRate(Product? product, string line)
    {
        if (product != null)
        {
            product.MaxProductionRate = ReadProductMaxProductionRate(line);
        }
    }
}