using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization; // For JsonPropertyName
using System.Threading.Tasks;

// --- 1. Enums: Define product categories and availability status ---
public enum ProductCategory
{
    Electronics,
    Books,
    Clothing,
    HomeGoods,
    Food,
    Other
}

public enum ProductAvailabilityStatus
{
    InStock,
    OutOfStock,
    LowStock,
    Discontinued
}

// --- 2. Interfaces: Define contracts for common behavior ---

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime LastModifiedAt { get; set; }
}

public interface ISellable
{
    string Sku { get; }
    decimal Price { get; }
    void DisplayPrice();
}

// --- 3. Base Class: Common properties and methods for all products ---

public abstract class Product : IAuditable, ISellable
{
    [JsonPropertyName("sku")]
    public string Sku { get; private set; } // Stock Keeping Unit - unique identifier
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("quantityInStock")]
    public int QuantityInStock { get; set; }
    [JsonPropertyName("price")]
    public decimal Price { get; private set; }
    [JsonPropertyName("category")]
    public ProductCategory Category { get; set; }
    [JsonPropertyName("availabilityStatus")]
    public ProductAvailabilityStatus AvailabilityStatus
    {
        get
        {
            if (QuantityInStock <= 0) return ProductAvailabilityStatus.OutOfStock;
            if (QuantityInStock <= 5) return ProductAvailabilityStatus.LowStock;
            return ProductAvailabilityStatus.InStock;
        }
        // Setter is needed for deserialization, but logic is in getter
        set { /* Required for JSON deserialization, actual logic is in getter */ }
    }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("lastModifiedAt")]
    public DateTime LastModifiedAt { get; set; }

    // Event for inventory changes
    public event EventHandler<InventoryUpdateEventArgs> InventoryUpdated;

    protected Product(string sku, string name, string description, int quantityInStock, decimal price, ProductCategory category)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU cannot be null or empty.", nameof(sku));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (quantityInStock < 0) throw new ArgumentOutOfRangeException(nameof(quantityInStock), "Quantity cannot be negative.");

        Sku = sku.ToUpper(); // Standardize SKU
        Name = name;
        Description = description;
        QuantityInStock = quantityInStock;
        Price = price;
        Category = category;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    // Default constructor for JSON deserialization
    protected Product() { }

    public virtual void DisplayProductDetails()
    {
        Console.WriteLine($"\n--- Product Details: {Name} ({Sku}) ---");
        Console.WriteLine($"Description: {Description}");
        Console.WriteLine($"Category: {Category}");
        Console.WriteLine($"Current Stock: {QuantityInStock} ({AvailabilityStatus})");
        DisplayPrice();
        Console.WriteLine($"Created: {CreatedAt:yyyy-MM-dd HH:mm:ss} (UTC)");
        Console.WriteLine($"Last Modified: {LastModifiedAt:yyyy-MM-dd HH:mm:ss} (UTC)");
    }

    public void DisplayPrice()
    {
        Console.WriteLine($"Price: {Price:C}"); // Currency formatting
    }

    /// <summary>
    /// Updates the quantity of the product in stock.
    /// Raises an InventoryUpdated event if the quantity changes.
    /// </summary>
    /// <param name="changeAmount">The amount to change the quantity by (positive for adding, negative for removing).</param>
    public void UpdateStock(int changeAmount)
    {
        int oldQuantity = QuantityInStock;
        if (QuantityInStock + changeAmount < 0)
        {
            throw new InvalidOperationException($"Cannot reduce stock of {Name} below zero. Current: {QuantityInStock}, Attempted change: {changeAmount}");
        }

        QuantityInStock += changeAmount;
        LastModifiedAt = DateTime.UtcNow;
        Console.WriteLine($"Stock of '{Name}' (SKU: {Sku}) updated from {oldQuantity} to {QuantityInStock}.");

        // Raise the event
        if (oldQuantity != QuantityInStock)
        {
            OnInventoryUpdated(new InventoryUpdateEventArgs(Sku, oldQuantity, QuantityInStock, Name));
        }
    }

    protected virtual void OnInventoryUpdated(InventoryUpdateEventArgs e)
    {
        InventoryUpdated?.Invoke(this, e);
    }
}

// --- Event Arguments for Inventory Updates ---
public class InventoryUpdateEventArgs : EventArgs
{
    public string Sku { get; }
    public string ProductName { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public InventoryUpdateEventArgs(string sku, int oldQuantity, int newQuantity, string productName)
    {
        Sku = sku;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
        ProductName = productName;
    }
}

// --- 4. Derived Classes: Specific product types ---

public class ElectronicProduct : Product
{
    [JsonPropertyName("brand")]
    public string Brand { get; set; }
    [JsonPropertyName("warrantyPeriodMonths")]
    public int WarrantyPeriodMonths { get; set; } // In months

    public ElectronicProduct(string sku, string name, string description, int quantityInStock, decimal price,
                             string brand, int warrantyPeriodMonths)
        : base(sku, name, description, quantityInStock, price, ProductCategory.Electronics)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentException("Brand cannot be null or empty.", nameof(brand));
        if (warrantyPeriodMonths < 0) throw new ArgumentOutOfRangeException(nameof(warrantyPeriodMonths), "Warranty period cannot be negative.");
        Brand = brand;
        WarrantyPeriodMonths = warrantyPeriodMonths;
    }

    // Default constructor for JSON deserialization
    public ElectronicProduct() : base() { }

    public override void DisplayProductDetails()
    {
        base.DisplayProductDetails();
        Console.WriteLine($"Brand: {Brand}");
        Console.WriteLine($"Warranty: {WarrantyPeriodMonths} months");
    }
}

public class BookProduct : Product
{
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("isbn")]
    public string Isbn { get; set; }
    [JsonPropertyName("pages")]
    public int Pages { get; set; }
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; }

    public BookProduct(string sku, string name, string description, int quantityInStock, decimal price,
                       string author, string isbn, int pages, string publisher)
        : base(sku, name, description, quantityInStock, price, ProductCategory.Books)
    {
        if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Author cannot be null or empty.", nameof(author));
        if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN cannot be null or empty.", nameof(isbn));
        if (string.IsNullOrWhiteSpace(publisher)) throw new ArgumentException("Publisher cannot be null or empty.", nameof(publisher));
        if (pages <= 0) throw new ArgumentOutOfRangeException(nameof(pages), "Pages must be positive.");

        Author = author;
        Isbn = isbn;
        Pages = pages;
        Publisher = publisher;
    }

    // Default constructor for JSON deserialization
    public BookProduct() : base() { }

    public override void DisplayProductDetails()
    {
        base.DisplayProductDetails();
        Console.WriteLine($"Author: {Author}");
        Console.WriteLine($"ISBN: {Isbn}");
        Console.WriteLine($"Pages: {Pages}");
        Console.WriteLine($"Publisher: {Publisher}");
    }
}

// --- 5. Data Persistence Layer: Save and Load products ---

public class ProductCatalogRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ProductCatalogRepository(string filePath)
    {
        _filePath = filePath;
        // Configure JSON serializer to handle polymorphism and pretty print
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ProductJsonConverter() } // Custom converter for polymorphism
        };
    }

    /// <summary>
    /// Asynchronously loads products from a JSON file.
    /// </summary>
    /// <returns>A dictionary of products, keyed by SKU.</returns>
    public async Task<Dictionary<string, Product>> LoadProductsAsync()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"No existing product catalog found at '{_filePath}'. Starting with an empty catalog.");
            return new Dictionary<string, Product>();
        }

        try
        {
            Console.WriteLine($"Loading product catalog from '{_filePath}'...");
            string jsonString = await File.ReadAllTextAsync(_filePath);
            var productsList = JsonSerializer.Deserialize<List<Product>>(jsonString, _jsonSerializerOptions);

            var productsDictionary = productsList?.ToDictionary(p => p.Sku, p => p) ?? new Dictionary<string, Product>();
            Console.WriteLine($"Successfully loaded {productsDictionary.Count} products.");
            return productsDictionary;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing product data from '{_filePath}': {ex.Message}");
            Console.WriteLine("Please check the JSON file format. Returning empty catalog.");
            return new Dictionary<string, Product>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error loading product data from '{_filePath}': {ex.Message}");
            return new Dictionary<string, Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while loading products: {ex.Message}");
            return new Dictionary<string, Product>();
        }
    }

    /// <summary>
    /// Asynchronously saves products to a JSON file.
    /// </summary>
    /// <param name="products">The dictionary of products to save.</param>
    public async Task SaveProductsAsync(Dictionary<string, Product> products)
    {
        try
        {
            Console.WriteLine($"Saving {products.Count} products to '{_filePath}'...");
            var productsList = products.Values.ToList();
            string jsonString = JsonSerializer.Serialize(productsList, _jsonSerializerOptions);
            await File.WriteAllTextAsync(_filePath, jsonString);
            Console.WriteLine("Product catalog saved successfully.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error serializing product data: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error saving product data to '{_filePath}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while saving products: {ex.Message}");
        }
    }
}

// Custom JSON Converter to handle polymorphic deserialization
// This is crucial because JsonSerializer doesn't inherently know which derived type to create
// without type hints or a custom converter.
public class ProductJsonConverter : JsonConverter<Product>
{
    public override Product Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Copy options to avoid infinite loop with custom converter
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(this); // Remove this converter to prevent recursion

        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            // Try to determine the product type based on properties
            if (doc.RootElement.TryGetProperty("category", out JsonElement categoryElement))
            {
                if (Enum.TryParse<ProductCategory>(categoryElement.GetString(), out ProductCategory category))
                {
                    switch (category)
                    {
                        case ProductCategory.Electronics:
                            return doc.RootElement.Deserialize<ElectronicProduct>(newOptions);
                        case ProductCategory.Books:
                            return doc.RootElement.Deserialize<BookProduct>(newOptions);
                        // Add cases for other product categories as they are created
                        default:
                            Console.WriteLine($"Warning: Unknown product category '{category}'. Deserializing as base Product type.");
                            return doc.RootElement.Deserialize<Product>(newOptions);
                    }
                }
            }
            // Fallback if category is not found or invalid
            return doc.RootElement.Deserialize<Product>(newOptions);
        }
    }

    public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
    {
        // Use default serialization for the actual type
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}


// --- 6. Product Catalog Manager: Orchestrates operations ---

public class ProductCatalogManager
{
    private Dictionary<string, Product> _products; // Keyed by SKU for quick lookup
    private readonly ProductCatalogRepository _repository;

    public ProductCatalogManager(string dataFilePath)
    {
        _repository = new ProductCatalogRepository(dataFilePath);
        _products = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase); // Case-insensitive SKU lookup
    }

    public async Task InitializeCatalogAsync()
    {
        _products = await _repository.LoadProductsAsync();
        // Subscribe to InventoryUpdated events for all loaded products
        foreach (var product in _products.Values)
        {
            product.InventoryUpdated += OnProductInventoryUpdated;
        }
    }

    public async Task SaveCatalogAsync()
    {
        await _repository.SaveProductsAsync(_products);
    }

    /// <summary>
    /// Adds a new product to the catalog.
    /// </summary>
    public void AddProduct(Product product)
    {
        if (_products.ContainsKey(product.Sku))
        {
            Console.WriteLine($"Error: Product with SKU '{product.Sku}' already exists. Cannot add duplicate.");
            return;
        }

        _products.Add(product.Sku, product);
        product.InventoryUpdated += OnProductInventoryUpdated; // Subscribe to its events
        Console.WriteLine($"Added new product: {product.Name} (SKU: {product.Sku})");
    }

    /// <summary>
    /// Retrieves a product by its SKU.
    /// </summary>
    public Product GetProductBySku(string sku)
    {
        _products.TryGetValue(sku.ToUpper(), out var product);
        return product;
    }

    /// <summary>
    /// Updates an existing product's details (excluding SKU and Price).
    /// </summary>
    public void UpdateProductDetails(string sku, string newName, string newDescription, ProductCategory newCategory)
    {
        var product = GetProductBySku(sku);
        if (product == null)
        {
            Console.WriteLine($"Error: Product with SKU '{sku}' not found for update.");
            return;
        }

        product.Name = newName;
        product.Description = newDescription;
        product.Category = newCategory;
        product.LastModifiedAt = DateTime.UtcNow;
        Console.WriteLine($"Updated details for product: {product.Name} (SKU: {sku})");
    }

    /// <summary>
    /// Removes a product from the catalog.
    /// </summary>
    public void RemoveProduct(string sku)
    {
        if (_products.Remove(sku.ToUpper(), out var removedProduct))
        {
            removedProduct.InventoryUpdated -= OnProductInventoryUpdated; // Unsubscribe
            Console.WriteLine($"Removed product: {removedProduct.Name} (SKU: {sku})");
        }
        else
        {
            Console.WriteLine($"Error: Product with SKU '{sku}' not found for removal.");
        }
    }

    /// <summary>
    /// Lists all products in the catalog, optionally filtering by category.
    /// </summary>
    public void ListAllProducts(ProductCategory? filterCategory = null)
    {
        Console.WriteLine("\n--- Current Product Catalog ---");
        var productsToList = filterCategory.HasValue
            ? _products.Values.Where(p => p.Category == filterCategory.Value).ToList()
            : _products.Values.ToList();

        if (!productsToList.Any())
        {
            Console.WriteLine(filterCategory.HasValue ? $"No products found in category: {filterCategory.Value}" : "No products in the catalog.");
            return;
        }

        foreach (var product in productsToList.OrderBy(p => p.Name))
        {
            product.DisplayProductDetails();
            Console.WriteLine("-----------------------------");
        }
    }

    /// <summary>
    /// Searches for products by name or description.
    /// </summary>
    public void SearchProducts(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }

        var results = _products.Values.Where(p =>
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Sku.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (results.Any())
        {
            Console.WriteLine($"\n--- Search Results for '{searchTerm}' ---");
            foreach (var product in results.OrderBy(p => p.Name))
            {
                product.DisplayProductDetails();
                Console.WriteLine("-----------------------------");
            }
        }
        else
        {
            Console.WriteLine($"No products found matching '{searchTerm}'.");
        }
    }

    /// <summary>
    /// Handles the InventoryUpdated event from a product.
    /// </summary>
    private void OnProductInventoryUpdated(object sender, InventoryUpdateEventArgs e)
    {
        Console.WriteLine($"[EVENT] Inventory Change for {e.ProductName} (SKU: {e.Sku}): {e.OldQuantity} -> {e.NewQuantity}");
        // Here you could trigger other actions, e.g., send a low stock alert, update a GUI, etc.
    }
}

// --- 7. Main Application Logic / Simulation of Usage ---

public class ECommerceApp
{
    private const string DataFileName = "product_catalog.json";
    private ProductCatalogManager _catalogManager;

    public async Task Run()
    {
        _catalogManager = new ProductCatalogManager(DataFileName);
        await _catalogManager.InitializeCatalogAsync();

        Console.WriteLine("\n--- E-commerce Product Catalog Application ---");
        bool running = true;
        while (running)
        {
            Console.WriteLine("\nSelect an option:");
            Console.WriteLine("1. Add New Product");
            Console.WriteLine("2. List All Products");
            Console.WriteLine("3. Search Products");
            Console.WriteLine("4. Update Product Stock");
            Console.WriteLine("5. Update Product Details");
            Console.WriteLine("6. Remove Product");
            Console.WriteLine("7. List Electronics");
            Console.WriteLine("8. List Books");
            Console.WriteLine("9. Save Catalog & Exit");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddNewProduct();
                        break;
                    case "2":
                        _catalogManager.ListAllProducts();
                        break;
                    case "3":
                        Console.Write("Enter search term (e.g., 'phone', 'fiction'): ");
                        string searchTerm = Console.ReadLine();
                        _catalogManager.SearchProducts(searchTerm);
                        break;
                    case "4":
                        await UpdateProductStock();
                        break;
                    case "5":
                        await UpdateProductDetails();
                        break;
                    case "6":
                        Console.Write("Enter SKU of product to remove: ");
                        string removeSku = Console.ReadLine();
                        _catalogManager.RemoveProduct(removeSku);
                        break;
                    case "7":
                        _catalogManager.ListAllProducts(ProductCategory.Electronics);
                        break;
                    case "8":
                        _catalogManager.ListAllProducts(ProductCategory.Books);
                        break;
                    case "9":
                        await _catalogManager.SaveCatalogAsync();
                        running = false;
                        Console.WriteLine("Exiting application.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // In a real app, you might log this error more formally
            }
            Console.WriteLine("\n--- Press any key to continue ---");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private async Task AddNewProduct()
    {
        Console.WriteLine("--- Add New Product ---");
        Console.Write("Enter SKU: ");
        string sku = Console.ReadLine();
        Console.Write("Enter Name: ");
        string name = Console.ReadLine();
        Console.Write("Enter Description: ");
        string description = Console.ReadLine();
        Console.Write("Enter Quantity: ");
        int quantity = int.Parse(Console.ReadLine());
        Console.Write("Enter Price: ");
        decimal price = decimal.Parse(Console.ReadLine());

        Console.WriteLine("Select Category:");
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            Console.WriteLine($"{(int)category}. {category}");
        }
        Console.Write("Enter category number: ");
        ProductCategory productCategory = (ProductCategory)int.Parse(Console.ReadLine());

        if (productCategory == ProductCategory.Electronics)
        {
            Console.Write("Enter Brand: ");
            string brand = Console.ReadLine();
            Console.Write("Enter Warranty Period (months): ");
            int warranty = int.Parse(Console.ReadLine());
            _catalogManager.AddProduct(new ElectronicProduct(sku, name, description, quantity, price, brand, warranty));
        }
        else if (productCategory == ProductCategory.Books)
        {
            Console.Write("Enter Author: ");
            string author = Console.ReadLine();
            Console.Write("Enter ISBN: ");
            string isbn = Console.ReadLine();
            Console.Write("Enter Number of Pages: ");
            int pages = int.Parse(Console.ReadLine());
            Console.Write("Enter Publisher: ");
            string publisher = Console.ReadLine();
            _catalogManager.AddProduct(new BookProduct(sku, name, description, quantity, price, author, isbn, pages, publisher));
        }
        else
        {
            // For other general categories, just use the base Product class
            Console.WriteLine($"Adding product as general {productCategory} category.");
            // Note: The base Product constructor is protected. We'd need a specific derived class for "General" products,
            // or modify the base class constructor to be public, or make Product not abstract.
            // For simplicity in this example, if not Elec or Book, we'll indicate this limitation.
            Console.WriteLine("Error: Only Electronic and Book products can be added through this interface currently. For other types, a specific derived class is needed.");
        }
    }

    private async Task UpdateProductStock()
    {
        Console.Write("Enter SKU of product to update stock: ");
        string sku = Console.ReadLine();
        var product = _catalogManager.GetProductBySku(sku);

        if (product == null)
        {
            Console.WriteLine($"Product with SKU '{sku}' not found.");
            return;
        }

        Console.WriteLine($"Current stock for {product.Name}: {product.QuantityInStock}");
        Console.Write("Enter stock change amount (e.g., 5 to add 5, -2 to remove 2): ");
        int change = int.Parse(Console.ReadLine());

        try
        {
            product.UpdateStock(change);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Stock update failed: {ex.Message}");
        }
    }

    private async Task UpdateProductDetails()
    {
        Console.Write("Enter SKU of product to update details: ");
        string sku = Console.ReadLine();
        var product = _catalogManager.GetProductBySku(sku);

        if (product == null)
        {
            Console.WriteLine($"Product with SKU '{sku}' not found.");
            return;
        }

        Console.WriteLine($"--- Updating details for {product.Name} (SKU: {product.Sku}) ---");
        Console.Write($"Enter new Name (current: {product.Name}): ");
        string newName = Console.ReadLine();
        Console.Write($"Enter new Description (current: {product.Description}): ");
        string newDescription = Console.ReadLine();

        Console.WriteLine("Select new Category:");
        foreach (ProductCategory category in Enum.GetValues(typeof(ProductCategory)))
        {
            Console.WriteLine($"{(int)category}. {category}");
        }
        Console.Write($"Enter new category number (current: {product.Category}): ");
        ProductCategory newCategory = (ProductCategory)int.Parse(Console.ReadLine());

        _catalogManager.UpdateProductDetails(sku, newName, newDescription, newCategory);
    }

    public static async Task Main(string[] args)
    {
        // Clean up previous data file for a fresh start each run if desired
        // if (File.Exists(DataFileName)) File.Delete(DataFileName);

        var app = new ECommerceApp();
        await app.Run();
    }
}