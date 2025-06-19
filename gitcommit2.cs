using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // Namespace for JSON serialization

// Represents a single contact
public class Contact
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    public Contact(string name, string email, string phoneNumber)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    // Default constructor needed for JSON deserialization
    public Contact() { }

    public void DisplayContact()
    {
        Console.WriteLine($"Name: {Name}, Email: {Email}, Phone: {PhoneNumber}");
    }
}

// Manages a collection of contacts and handles file operations
public class ContactManager
{
    private List<Contact> _contacts;
    private readonly string _filePath;

    public ContactManager(string fileName = "contacts.json")
    {
        _contacts = new List<Contact>();
        _filePath = Path.Combine(AppContext.BaseDirectory, fileName); // Ensures file is in application directory
        LoadContactsFromFile(); // Try to load existing contacts when manager is created
    }

    public void AddContact(Contact contact)
    {
        _contacts.Add(contact);
        Console.WriteLine($"Contact '{contact.Name}' added.");
    }

    public List<Contact> GetAllContacts()
    {
        return _contacts;
    }

    public Contact FindContactByName(string name)
    {
        return _contacts.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveContact(string name)
    {
        Contact contactToRemove = FindContactByName(name);
        if (contactToRemove != null)
        {
            _contacts.Remove(contactToRemove);
            Console.WriteLine($"Contact '{contactToRemove.Name}' removed.");
        }
        else
        {
            Console.WriteLine($"Contact '{name}' not found.");
        }
    }

    public void SaveContactsToFile()
    {
        try
        {
            // Configure JSON options for pretty printing
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_contacts, options);
            File.WriteAllText(_filePath, jsonString);
            Console.WriteLine($"Contacts saved successfully to {_filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving contacts: {ex.Message}");
        }
    }

    public void LoadContactsFromFile()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                _contacts = JsonSerializer.Deserialize<List<Contact>>(jsonString) ?? new List<Contact>();
                Console.WriteLine($"Contacts loaded successfully from {_filePath}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing contacts from file (JSON format issue): {ex.Message}");
                _contacts = new List<Contact>(); // Reset contacts to avoid corrupted state
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading contacts: {ex.Message}");
                _contacts = new List<Contact>(); // Reset contacts on other errors
            }
        }
        else
        {
            Console.WriteLine("No existing contacts file found. Starting with an empty list.");
        }
    }
}

// Main program to demonstrate usage
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Simple Contact Management System ---");

        ContactManager manager = new ContactManager();

        Console.WriteLine("\n--- Adding New Contacts ---");
        manager.AddContact(new Contact("Alice Smith", "alice@example.com", "123-456-7890"));
        manager.AddContact(new Contact("Bob Johnson", "bob@example.com", "987-654-3210"));
        manager.AddContact(new Contact("Charlie Brown", "charlie@example.com", "555-123-4567"));

        Console.WriteLine("\n--- All Contacts Before Save ---");
        foreach (var contact in manager.GetAllContacts())
        {
            contact.DisplayContact();
        }

        manager.SaveContactsToFile(); // Save contacts to JSON

        Console.WriteLine("\n--- Removing Bob Johnson ---");
        manager.RemoveContact("Bob Johnson");

        Console.WriteLine("\n--- All Contacts After Removal (still in memory) ---");
        foreach (var contact in manager.GetAllContacts())
        {
            contact.DisplayContact();
        }

        // Simulate restarting the application by creating a new manager
        Console.WriteLine("\n--- Simulating Application Restart (Loading from file) ---");
        ContactManager newManager = new ContactManager();

        Console.WriteLine("\n--- Contacts Loaded from File ---");
        foreach (var contact in newManager.GetAllContacts())
        {
            contact.DisplayContact();
        }

        // Add another contact and save again
        Console.WriteLine("\n--- Adding new contact and saving again ---");
        newManager.AddContact(new Contact("Diana Prince", "diana@example.com", "111-222-3333"));
        newManager.SaveContactsToFile();

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }
}