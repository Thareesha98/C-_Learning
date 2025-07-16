using System;
using System.Collections.Generic;
using System.Linq;

// --- 1. Interfaces for defining contracts ---

public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    DateTime LastModifiedDate { get; set; }
}

public interface ISellable
{
    decimal Price { get; }
    void DisplayPrice();
}

// --- 2. Base Class for common properties and methods ---

public abstract class InventoryItem : IAuditable
{
    public string ItemId { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    protected InventoryItem(string itemId, string name, string description, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Item ID cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.");
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        ItemId = itemId;
        Name = name;
        Description = description;
        Quantity = quantity;
        CreatedDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
    }

    public virtual void DisplayDetails()
    {
        Console.WriteLine($"--- Item ID: {ItemId} ---");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Description: {Description}");
        Console.WriteLine($"Quantity: {Quantity}");
        Console.WriteLine($"Created: {CreatedDate}");
        Console.WriteLine($"Last Modified: {LastModifiedDate}");
    }

    public void UpdateQuantity(int change)
    {
        if (Quantity + change < 0)
        {
            throw new InvalidOperationException("Attempting to make quantity negative.");
        }
        Quantity += change;
        LastModifiedDate = DateTime.Now;
        Console.WriteLine($"Quantity of {Name} updated to {Quantity}.");
    }
}

// --- 3. Derived Classes demonstrating inheritance and specific properties ---

public class Electronics : InventoryItem, ISellable
{
    public string Manufacturer { get; set; }
    public int WarrantyMonths { get; set; }
    public decimal Price { get; private set; }

    public Electronics(string itemId, string name, string description, int quantity,
                       string manufacturer, int warrantyMonths, decimal price)
        : base(itemId, name, description, quantity)
    {
        if (string.IsNullOrWhiteSpace(manufacturer)) throw new ArgumentException("Manufacturer cannot be null or empty.");
        if (warrantyMonths < 0) throw new ArgumentOutOfRangeException(nameof(warrantyMonths), "Warranty cannot be negative.");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Manufacturer = manufacturer;
        WarrantyMonths = warrantyMonths;
        Price = price;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails(); // Call base class method
        Console.WriteLine($"Manufacturer: {Manufacturer}");
        Console.WriteLine($"Warranty: {WarrantyMonths} months");
        DisplayPrice();
    }

    public void DisplayPrice()
    {
        Console.WriteLine($"Price: {Price:C}");
    }
}

public class Book : InventoryItem, ISellable
{
    public string Author { get; set; }
    public string ISBN { get; set; }
    public int NumberOfPages { get; set; }
    public decimal Price { get; private set; }

    public Book(string itemId, string name, string description, int quantity,
                string author, string isbn, int numberOfPages, decimal price)
        : base(itemId, name, description, quantity)
    {
        if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Author cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN cannot be null or empty.");
        if (numberOfPages <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfPages), "Number of pages must be positive.");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Author = author;
        ISBN = isbn;
        NumberOfPages = numberOfPages;
        Price = price;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();
        Console.WriteLine($"Author: {Author}");
        Console.WriteLine($"ISBN: {ISBN}");
        Console.WriteLine($"Pages: {NumberOfPages}");
        DisplayPrice();
    }

    public void DisplayPrice()
    {
        Console.WriteLine($"Price: {Price:C}");
    }
}

// --- 4. Inventory Manager Class to handle collections and operations ---

public class InventoryManager
{
    private List<InventoryItem> _items;

    public InventoryManager()
    {
        _items = new List<InventoryItem>();
    }

    public void AddItem(InventoryItem item)
    {
        if (_items.Any(i => i.ItemId == item.ItemId))
        {
            Console.WriteLine($"Error: Item with ID '{item.ItemId}' already exists. Cannot add duplicate.");
            return;
        }
        _items.Add(item);
        Console.WriteLine($"Added item: {item.Name} (ID: {item.ItemId})");
    }

    public InventoryItem GetItemById(string itemId)
    {
        return _items.FirstOrDefault(i => i.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveItem(string itemId)
    {
        var itemToRemove = GetItemById(itemId);
        if (itemToRemove != null)
        {
            _items.Remove(itemToRemove);
            Console.WriteLine($"Removed item: {itemToRemove.Name} (ID: {itemId})");
        }
        else
        {
            Console.WriteLine($"Item with ID '{itemId}' not found.");
        }
    }

    public void ListAllItems()
    {
        if (!_items.Any())
        {
            Console.WriteLine("No items in inventory.");
            return;
        }
        Console.WriteLine("\n--- Current Inventory ---");
        foreach (var item in _items)
        {
            item.DisplayDetails();
            Console.WriteLine("-------------------------");
        }
    }

    public void SearchItems(string searchTerm)
    {
        var results = _items.Where(item =>
            item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            item.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            item.ItemId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (results.Any())
        {
            Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---");
            foreach (var item in results)
            {
                item.DisplayDetails();
                Console.WriteLine("-------------------------");
            }
        }
        else
        {
            Console.WriteLine($"No items found matching '{searchTerm}'.");
        }
    }

    // Polymorphism in action: listing sellable items and their prices
    public void ListSellableItems()
    {
        Console.WriteLine("\n--- Sellable Items in Inventory ---");
        foreach (var item in _items)
        {
            if (item is ISellable sellableItem) // Check if the item implements ISellable
            {
                Console.WriteLine($"Item: {item.Name} (ID: {item.ItemId})");
                sellableItem.DisplayPrice();
                Console.WriteLine("-------------------------");
            }
        }
    }
}

// --- 5. Main Application Entry Point ---

public class Program
{
    public static void Main(string[] args)
    {
        InventoryManager manager = new InventoryManager();

        // Add some items
        manager.AddItem(new Electronics("ELEC001", "Laptop", "High-performance laptop", 10, "Dell", 12, 1200.00m));
        manager.AddItem(new Book("BOOK001", "The Hitchhiker's Guide to the Galaxy", "A comedic science fiction series.", 50, "Douglas Adams", "978-0345391803", 224, 9.99m));
        manager.AddItem(new Electronics("ELEC002", "Smart Phone", "Latest model smart phone", 25, "Samsung", 6, 899.99m));
        manager.AddItem(new Book("BOOK002", "1984", "Dystopian social science fiction novel", 30, "George Orwell", "978-0451524935", 328, 7.50m));

        manager.ListAllItems();

        // Update quantity
        var laptop = manager.GetItemById("ELEC001");
        if (laptop != null)
        {
            laptop.UpdateQuantity(-2); // Sell 2 laptops
        }

        // Try to add a duplicate (will show error)
        manager.AddItem(new Electronics("ELEC001", "Another Laptop", "Should not be added", 1, "HP", 12, 1000.00m));

        // Search for items
        manager.SearchItems("phone");
        manager.SearchItems("fiction");

        // Remove an item
        manager.RemoveItem("BOOK001");

        manager.ListAllItems();

        // Demonstrate polymorphism with sellable items
        manager.ListSellableItems();

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }
}