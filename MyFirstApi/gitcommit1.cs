using System;
using System.Collections.Generic;
using System.Linq;

// Represents a product in our system
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ProductCategory Category { get; set; }

    public Product(int id, string name, decimal price, int stock, ProductCategory category)
    {
        Id = id;
        Name = name;
        Price = price;
        Stock = stock;
        Category = category;
    }

    public void DisplayProductDetails()
    {
        Console.WriteLine($"ID: {Id}, Name: {Name}, Price: {Price:C}, Stock: {Stock}, Category: {Category}");
    }
}

// Enum to categorize products
public enum ProductCategory
{
    Electronics,
    Books,
    HomeGoods,
    Food,
    Apparel
}

// Manages a collection of products
public class ProductManager
{
    private List<Product> _products;

    public ProductManager()
    {
        _products = new List<Product>();
        // Initialize with some sample data
        AddProduct(new Product(101, "Laptop", 1200.00m, 50, ProductCategory.Electronics));
        AddProduct(new Product(102, "C# Programming Book", 45.99m, 120, ProductCategory.Books));
        AddProduct(new Product(103, "Coffee Maker", 75.00m, 30, ProductCategory.HomeGoods));
        AddProduct(new Product(104, "Smartphone", 800.00m, 75, ProductCategory.Electronics));
        AddProduct(new Product(105, "T-Shirt", 25.00m, 200, ProductCategory.Apparel));
        AddProduct(new Product(106, "Milk", 3.50m, 300, ProductCategory.Food));
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
        Console.WriteLine($"Product '{product.Name}' added.");
    }

    public Product GetProductById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public List<Product> GetAllProducts()
    {
        return _products;
    }

    public void UpdateProductStock(int id, int newStock)
    {
        Product product = GetProductById(id);
        if (product != null)
        {
            product.Stock = newStock;
            Console.WriteLine($"Stock for '{product.Name}' updated to {newStock}.");
        }
        else
        {
            Console.WriteLine($"Product with ID {id} not found.");
        }
    }

    public void RemoveProduct(int id)
    {
        Product productToRemove = GetProductById(id);
        if (productToRemove != null)
        {
            _products.Remove(productToRemove);
            Console.WriteLine($"Product '{productToRemove.Name}' removed.");
        }
        else
        {
            Console.WriteLine($"Product with ID {id} not found.");
        }
    }

    public List<Product> SearchProductsByName(string searchTerm)
    {
        return _products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public List<Product> GetProductsByCategory(ProductCategory category)
    {
        return _products.Where(p => p.Category == category).ToList();
    }

    public List<Product> GetProductsUnderPrice(decimal maxPrice)
    {
        return _products.Where(p => p.Price < maxPrice).OrderBy(p => p.Price).ToList();
    }
}

// Main program to demonstrate usage
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Product Management System ---");

        ProductManager manager = new ProductManager();

        Console.WriteLine("\n--- All Products ---");
        foreach (var product in manager.GetAllProducts())
        {
            product.DisplayProductDetails();
        }

        Console.WriteLine("\n--- Searching for 'book' ---");
        var books = manager.SearchProductsByName("book");
        foreach (var product in books)
        {
            product.DisplayProductDetails();
        }

        Console.WriteLine("\n--- Products in Electronics category ---");
        var electronics = manager.GetProductsByCategory(ProductCategory.Electronics);
        foreach (var product in electronics)
        {
            product.DisplayProductDetails();
        }

        Console.WriteLine("\n--- Products under $100 ---");
        var affordableItems = manager.GetProductsUnderPrice(100.00m);
        foreach (var product in affordableItems)
        {
            product.DisplayProductDetails();
        }

        Console.WriteLine("\n--- Updating stock for Laptop (ID 101) ---");
        manager.UpdateProductStock(101, 45);
        manager.GetProductById(101)?.DisplayProductDetails();

        Console.WriteLine("\n--- Removing Coffee Maker (ID 103) ---");
        manager.RemoveProduct(103);
        Console.WriteLine("\n--- All Products After Removal ---");
        foreach (var product in manager.GetAllProducts())
        {
            product.DisplayProductDetails();
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }
}