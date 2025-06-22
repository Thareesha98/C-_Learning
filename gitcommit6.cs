using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// --- 1. Security & Hashing Utilities ---
public static class SecurityHelper
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 10000; // Number of iterations for PBKDF2

    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// Returns salt:hash format.
    /// </summary>
    public static string HashPassword(string password)
    {
        using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] salt = algorithm.Salt;
            byte[] hash = algorithm.GetBytes(KeySize);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);

        using (var algorithm = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] inputHash = algorithm.GetBytes(KeySize);
            return inputHash.SequenceEqual(hash);
        }
    }

    /// <summary>
    /// Generates a unique ID (GUID) for entities.
    /// </summary>
    public static string GenerateUniqueId()
    {
        return Guid.NewGuid().ToString("N"); // No hyphens
    }
}

// --- 2. User & Authentication Classes ---

public class User
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } // Stores salt:hash
    [JsonPropertyName("role")]
    public UserRole Role { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    public User(string username, string password, UserRole role = UserRole.Agent)
    {
        UserId = SecurityHelper.GenerateUniqueId();
        Username = username;
        PasswordHash = SecurityHelper.HashPassword(password);
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    // Default constructor for JSON deserialization
    public User() { }
}

public enum UserRole
{
    Agent,
    Manager,
    Admin
}

public class AuthenticationService
{
    private readonly Dictionary<string, User> _users; // Key: Username
    private readonly string _usersFilePath;
    private User _currentUser;

    public User CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;

    public AuthenticationService(string usersFilePath)
    {
        _usersFilePath = usersFilePath;
        _users = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
        Task.Run(LoadUsersAsync).Wait(); // Load users synchronously during startup
        EnsureDefaultAdminUser();
    }

    /// <summary>
    /// Loads user data from a JSON file.
    /// </summary>
    private async Task LoadUsersAsync()
    {
        if (!File.Exists(_usersFilePath))
        {
            Console.WriteLine("User data file not found. Starting with no users.");
            return;
        }
        try
        {
            string jsonString = await File.ReadAllTextAsync(_usersFilePath);
            var userList = JsonSerializer.Deserialize<List<User>>(jsonString);
            if (userList != null)
            {
                foreach (var user in userList)
                {
                    _users[user.Username] = user;
                }
            }
            Console.WriteLine($"Loaded {_users.Count} users.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading users: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves user data to a JSON file.
    /// </summary>
    private async Task SaveUsersAsync()
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(_users.Values.ToList(), new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_usersFilePath, jsonString);
            Console.WriteLine("User data saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving users: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures an initial admin user exists for first-time setup.
    /// </summary>
    private void EnsureDefaultAdminUser()
    {
        if (!_users.ContainsKey("admin"))
        {
            Console.WriteLine("Creating default admin user: 'admin' with password 'password'. Please change this in a real system!");
            var adminUser = new User("admin", "password", UserRole.Admin);
            _users.Add(adminUser.Username, adminUser);
            Task.Run(SaveUsersAsync).Wait(); // Save immediately
        }
    }

    public bool Login(string username, string password)
    {
        if (_users.TryGetValue(username, out User user))
        {
            if (SecurityHelper.VerifyPassword(password, user.PasswordHash))
            {
                _currentUser = user;
                _currentUser.LastLoginAt = DateTime.UtcNow;
                Console.WriteLine($"User '{username}' logged in successfully as {user.Role}.");
                Task.Run(SaveUsersAsync); // Save login timestamp asynchronously
                return true;
            }
        }
        Console.WriteLine("Invalid username or password.");
        return false;
    }

    public void Logout()
    {
        if (_currentUser != null)
        {
            Console.WriteLine($"User '{_currentUser.Username}' logged out.");
            _currentUser = null;
        }
    }

    public bool RegisterUser(string username, string password, UserRole role, User creator)
    {
        if (creator == null || creator.Role < UserRole.Admin) // Only Admin can create users
        {
            Console.WriteLine("Permission denied: Only administrators can register new users.");
            return false;
        }
        if (_users.ContainsKey(username))
        {
            Console.WriteLine($"Username '{username}' already exists.");
            return false;
        }

        var newUser = new User(username, password, role);
        _users.Add(username, newUser);
        Task.Run(SaveUsersAsync); // Save new user asynchronously
        Console.WriteLine($"User '{username}' ({role}) registered successfully by {creator.Username}.");
        return true;
    }
}

// --- 3. CRM Core Entities: Customer and Interaction ---

public class Customer
{
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("phone")]
    public string Phone { get; set; }
    [JsonPropertyName("address")]
    public string Address { get; set; }
    [JsonPropertyName("company")]
    public string Company { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
    [JsonPropertyName("interactions")]
    public List<Interaction> Interactions { get; set; } // List of interactions for this customer

    public Customer(string firstName, string lastName, string email, string phone, string address, string company)
    {
        CustomerId = SecurityHelper.GenerateUniqueId();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Address = address;
        Company = company;
        CreatedAt = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;
        Interactions = new List<Interaction>();
    }

    // Default constructor for JSON deserialization
    public Customer() { Interactions = new List<Interaction>(); }

    public void DisplaySummary()
    {
        Console.WriteLine($"ID: {CustomerId.Substring(0, 8)}... | Name: {FirstName} {LastName} | Email: {Email ?? "N/A"} | Company: {Company ?? "N/A"}");
    }

    public void DisplayDetails()
    {
        Console.WriteLine("\n--- Customer Details ---");
        Console.WriteLine($"Customer ID: {CustomerId}");
        Console.WriteLine($"Name: {FirstName} {LastName}");
        Console.WriteLine($"Email: {Email}");
        Console.WriteLine($"Phone: {Phone}");
        Console.WriteLine($"Address: {Address}");
        Console.WriteLine($"Company: {Company}");
        Console.WriteLine($"Created At: {CreatedAt:yyyy-MM-dd HH:mm:ss} (UTC)");
        Console.WriteLine($"Last Updated: {LastUpdated:yyyy-MM-dd HH:mm:ss} (UTC)");
        Console.WriteLine($"Total Interactions: {Interactions.Count}");
        Console.WriteLine("------------------------");
    }
}

public class Interaction
{
    [JsonPropertyName("interactionId")]
    public string InteractionId { get; set; }
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } // Link to customer
    [JsonPropertyName("interactionType")]
    public InteractionType Type { get; set; }
    [JsonPropertyName("interactionDate")]
    public DateTime InteractionDate { get; set; }
    [JsonPropertyName("notes")]
    public string Notes { get; set; }
    [JsonPropertyName("recordedByUserId")]
    public string RecordedByUserId { get; set; } // User who recorded this interaction

    public Interaction(string customerId, InteractionType type, string notes, string recordedByUserId)
    {
        InteractionId = SecurityHelper.GenerateUniqueId();
        CustomerId = customerId;
        Type = type;
        InteractionDate = DateTime.UtcNow;
        Notes = notes;
        RecordedByUserId = recordedByUserId;
    }

    // Default constructor for JSON deserialization
    public Interaction() { }

    public void DisplayInteraction()
    {
        Console.WriteLine($"- Interaction ID: {InteractionId.Substring(0, 8)}...");
        Console.WriteLine($"  Type: {Type}");
        Console.WriteLine($"  Date: {InteractionDate:yyyy-MM-dd HH:mm:ss} (UTC)");
        Console.WriteLine($"  Notes: {Notes}");
        Console.WriteLine($"  Recorded By User ID: {RecordedByUserId.Substring(0, 8)}...");
    }
}

public enum InteractionType
{
    Call,
    Email,
    Meeting,
    SupportTicket,
    Other
}

// --- 4. Data Access / Repository Layer ---

public class CrmRepository
{
    private readonly string _customersFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public CrmRepository(string customersFilePath)
    {
        _customersFilePath = customersFilePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    /// <summary>
    /// Asynchronously loads all customer data including their interactions.
    /// </summary>
    public async Task<List<Customer>> LoadCustomersAsync()
    {
        if (!File.Exists(_customersFilePath))
        {
            Console.WriteLine("Customer data file not found. Starting with an empty CRM.");
            return new List<Customer>();
        }

        try
        {
            Console.WriteLine($"Loading CRM data from '{_customersFilePath}'...");
            string jsonString = await File.ReadAllTextAsync(_customersFilePath);
            var customers = JsonSerializer.Deserialize<List<Customer>>(jsonString, _jsonOptions);
            Console.WriteLine($"Successfully loaded {customers?.Count ?? 0} customers.");
            return customers ?? new List<Customer>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing CRM data from '{_customersFilePath}': {ex.Message}");
            Console.WriteLine("Please check the JSON file format. Returning empty list.");
            return new List<Customer>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error loading CRM data from '{_customersFilePath}': {ex.Message}");
            return new List<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while loading customers: {ex.Message}");
            return new List<Customer>();
        }
    }

    /// <summary>
    /// Asynchronously saves all customer data including their interactions.
    /// </summary>
    public async Task SaveCustomersAsync(List<Customer> customers)
    {
        try
        {
            Console.WriteLine($"Saving {customers.Count} customers to '{_customersFilePath}'...");
            string jsonString = JsonSerializer.Serialize(customers, _jsonOptions);
            await File.WriteAllTextAsync(_customersFilePath, jsonString);
            Console.WriteLine("CRM data saved successfully.");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error serializing CRM data: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error saving CRM data to '{_customersFilePath}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred while saving CRM data: {ex.Message}");
        }
    }
}

// --- 5. CRM Service Layer (Business Logic) ---

public class CrmService
{
    private readonly Dictionary<string, Customer> _customers; // Key: CustomerId
    private readonly CrmRepository _repository;
    private readonly AuthenticationService _authService; // Dependency injection for auth

    public CrmService(string dataFilePath, AuthenticationService authService)
    {
        _repository = new CrmRepository(dataFilePath);
        _authService = authService;
        _customers = new Dictionary<string, Customer>();
    }

    /// <summary>
    /// Initializes the CRM by loading data.
    /// </summary>
    public async Task InitializeCrmAsync()
    {
        var loadedCustomers = await _repository.LoadCustomersAsync();
        foreach (var customer in loadedCustomers)
        {
            _customers[customer.CustomerId] = customer;
        }
    }

    /// <summary>
    /// Saves all CRM data.
    /// </summary>
    public async Task SaveCrmAsync()
    {
        await _repository.SaveCustomersAsync(_customers.Values.ToList());
    }

    /// <summary>
    /// Adds a new customer to the CRM. Requires login.
    /// </summary>
    public void AddCustomer(string firstName, string lastName, string email, string phone, string address, string company)
    {
        if (!_authService.IsLoggedIn)
        {
            Console.WriteLine("Error: Must be logged in to add a customer.");
            return;
        }
        // Basic validation
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Error: First name and last name are required.");
            return;
        }
        if (!IsValidEmail(email))
        {
            Console.WriteLine("Error: Invalid email format.");
            return;
        }

        var newCustomer = new Customer(firstName, lastName, email, phone, address, company);
        _customers.Add(newCustomer.CustomerId, newCustomer);
        Console.WriteLine($"Customer '{newCustomer.FirstName} {newCustomer.LastName}' added with ID: {newCustomer.CustomerId}.");
        newCustomer.DisplaySummary();
    }

    /// <summary>
    /// Retrieves a customer by their ID.
    /// </summary>
    public Customer GetCustomerById(string customerId)
    {
        _customers.TryGetValue(customerId, out var customer);
        return customer;
    }

    /// <summary>
    /// Updates an existing customer's details. Requires login.
    /// </summary>
    public void UpdateCustomer(string customerId, string firstName, string lastName, string email, string phone, string address, string company)
    {
        if (!_authService.IsLoggedIn)
        {
            Console.WriteLine("Error: Must be logged in to update a customer.");
            return;
        }

        var customer = GetCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine($"Error: Customer with ID '{customerId}' not found.");
            return;
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Error: First name and last name are required for update.");
            return;
        }
        if (!IsValidEmail(email))
        {
            Console.WriteLine("Error: Invalid email format for update.");
            return;
        }

        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = address;
        customer.Company = company;
        customer.LastUpdated = DateTime.UtcNow;
        Console.WriteLine($"Customer '{customer.FirstName} {customer.LastName}' (ID: {customerId}) updated successfully.");
    }

    /// <summary>
    /// Deletes a customer and all their interactions. Requires Admin role.
    /// </summary>
    public void DeleteCustomer(string customerId)
    {
        if (!_authService.IsLoggedIn || _authService.CurrentUser.Role < UserRole.Admin)
        {
            Console.WriteLine("Permission denied: Only administrators can delete customers.");
            return;
        }

        if (_customers.Remove(customerId, out var deletedCustomer))
        {
            Console.WriteLine($"Customer '{deletedCustomer.FirstName} {deletedCustomer.LastName}' (ID: {customerId}) deleted successfully.");
        }
        else
        {
            Console.WriteLine($"Error: Customer with ID '{customerId}' not found for deletion.");
        }
    }

    /// <summary>
    /// Logs a new interaction for a customer. Requires login.
    /// </summary>
    public void LogCustomerInteraction(string customerId, InteractionType type, string notes)
    {
        if (!_authService.IsLoggedIn)
        {
            Console.WriteLine("Error: Must be logged in to log an interaction.");
            return;
        }
        var customer = GetCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine($"Error: Customer with ID '{customerId}' not found to log interaction.");
            return;
        }
        if (string.IsNullOrWhiteSpace(notes))
        {
            Console.WriteLine("Error: Interaction notes cannot be empty.");
            return;
        }

        var interaction = new Interaction(customerId, type, notes, _authService.CurrentUser.UserId);
        customer.Interactions.Add(interaction);
        customer.LastUpdated = DateTime.UtcNow; // Update customer's timestamp too
        Console.WriteLine($"Interaction of type '{type}' logged for customer '{customer.FirstName} {customer.LastName}'.");
    }

    /// <summary>
    /// Displays all interactions for a specific customer.
    /// </summary>
    public void DisplayCustomerInteractions(string customerId)
    {
        var customer = GetCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine($"Error: Customer with ID '{customerId}' not found.");
            return;
        }

        Console.WriteLine($"\n--- Interactions for {customer.FirstName} {customer.LastName} (ID: {customerId}) ---");
        if (!customer.Interactions.Any())
        {
            Console.WriteLine("No interactions recorded for this customer.");
            return;
        }

        foreach (var interaction in customer.Interactions.OrderByDescending(i => i.InteractionDate))
        {
            interaction.DisplayInteraction();
            Console.WriteLine("---");
        }
    }

    /// <summary>
    /// Lists all customers, optionally filtered by name or company.
    /// </summary>
    public void ListCustomers(string searchTerm = null)
    {
        Console.WriteLine("\n--- All Customers ---");
        IEnumerable<Customer> query = _customers.Values;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (c.Company != null && c.Company.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            );
            Console.WriteLine($"Filtering by search term: '{searchTerm}'");
        }

        var customersToList = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();

        if (!customersToList.Any())
        {
            Console.WriteLine("No customers found.");
            return;
        }

        foreach (var customer in customersToList)
        {
            customer.DisplaySummary();
        }
        Console.WriteLine("---------------------");
    }

    /// <summary>
    /// Basic email format validation.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// --- 6. Main Application UI/Entry Point ---

public class CrmApp
{
    private const string CustomersDataFile = "crm_customers.json";
    private const string UsersDataFile = "crm_users.json";

    private AuthenticationService _authService;
    private CrmService _crmService;

    public async Task Run()
    {
        Console.OutputEncoding = Encoding.UTF8; // For better console output

        _authService = new AuthenticationService(UsersDataFile);
        _crmService = new CrmService(CustomersDataFile, _authService);

        Console.WriteLine("Initializing CRM system...");
        await _crmService.InitializeCrmAsync();

        if (!_authService.IsLoggedIn)
        {
            Console.WriteLine("\nPlease login to continue.");
            if (!LoginPrompt())
            {
                Console.WriteLine("Failed to log in. Exiting application.");
                return;
            }
        }

        bool running = true;
        while (running)
        {
            DisplayMainMenu();
            string choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddCustomer();
                        break;
                    case "2":
                        _crmService.ListCustomers();
                        break;
                    case "3":
                        await ViewCustomerDetails();
                        break;
                    case "4":
                        await UpdateCustomer();
                        break;
                    case "5":
                        await LogInteraction();
                        break;
                    case "6":
                        await SearchCustomers();
                        break;
                    case "7":
                        await ManageUsers(); // Admin-only
                        break;
                    case "8":
                        _authService.Logout();
                        if (!LoginPrompt()) // Force re-login
                        {
                            Console.WriteLine("Login required. Exiting application.");
                            running = false;
                        }
                        break;
                    case "9":
                        await _crmService.SaveCrmAsync();
                        _authService.Logout();
                        running = false;
                        Console.WriteLine("CRM data saved. Exiting application. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
                Console.WriteLine($"Details: {ex.StackTrace}");
            }
            Console.WriteLine("\n--- Press any key to continue ---");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private bool LoginPrompt()
    {
        int attempts = 0;
        const int maxAttempts = 3;
        while (!_authService.IsLoggedIn && attempts < maxAttempts)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = ReadPasswordMasked(); // Mask password input
            Console.WriteLine(); // New line after password input

            if (_authService.Login(username, password))
            {
                return true;
            }
            attempts++;
            Console.WriteLine($"Login failed. {maxAttempts - attempts} attempts remaining.");
        }
        return false;
    }

    private void DisplayMainMenu()
    {
        Console.WriteLine($"\n--- CRM Main Menu (Logged in as: {_authService.CurrentUser.Username} ({_authService.CurrentUser.Role})) ---");
        Console.WriteLine("1. Add New Customer");
        Console.WriteLine("2. List All Customers");
        Console.WriteLine("3. View Customer Details & Interactions");
        Console.WriteLine("4. Update Customer Details");
        Console.WriteLine("5. Log Customer Interaction");
        Console.WriteLine("6. Search Customers");
        if (_authService.CurrentUser.Role >= UserRole.Admin)
        {
            Console.WriteLine("7. Manage Users (Admin Only)");
        }
        Console.WriteLine("8. Logout / Switch User");
        Console.WriteLine("9. Save CRM & Exit");
        Console.Write("Enter your choice: ");
    }

    private async Task AddCustomer()
    {
        Console.WriteLine("\n--- Add New Customer ---");
        Console.Write("First Name: ");
        string firstName = Console.ReadLine();
        Console.Write("Last Name: ");
        string lastName = Console.ReadLine();
        Console.Write("Email: ");
        string email = Console.ReadLine();
        Console.Write("Phone: ");
        string phone = Console.ReadLine();
        Console.Write("Address: ");
        string address = Console.ReadLine();
        Console.Write("Company: ");
        string company = Console.ReadLine();

        _crmService.AddCustomer(firstName, lastName, email, phone, address, company);
    }

    private async Task ViewCustomerDetails()
    {
        Console.Write("Enter Customer ID or partial ID to view details: ");
        string inputId = Console.ReadLine();
        var customer = FindCustomerByPartialId(inputId);

        if (customer == null)
        {
            Console.WriteLine($"Customer with ID or partial ID '{inputId}' not found.");
            return;
        }
        customer.DisplayDetails();
        _crmService.DisplayCustomerInteractions(customer.CustomerId);
    }

    private async Task UpdateCustomer()
    {
        Console.Write("Enter Customer ID or partial ID to update: ");
        string inputId = Console.ReadLine();
        var customer = FindCustomerByPartialId(inputId);

        if (customer == null)
        {
            Console.WriteLine($"Customer with ID or partial ID '{inputId}' not found.");
            return;
        }

        Console.WriteLine($"\n--- Updating Customer: {customer.FirstName} {customer.LastName} (ID: {customer.CustomerId}) ---");
        Console.Write($"New First Name (current: {customer.FirstName}): ");
        string newFirstName = Console.ReadLine();
        Console.Write($"New Last Name (current: {customer.LastName}): ");
        string newLastName = Console.ReadLine();
        Console.Write($"New Email (current: {customer.Email}): ");
        string newEmail = Console.ReadLine();
        Console.Write($"New Phone (current: {customer.Phone}): ");
        string newPhone = Console.ReadLine();
        Console.Write($"New Address (current: {customer.Address}): ");
        string newAddress = Console.ReadLine();
        Console.Write($"New Company (current: {customer.Company}): ");
        string newCompany = Console.ReadLine();

        _crmService.UpdateCustomer(customer.CustomerId, newFirstName, newLastName, newEmail, newPhone, newAddress, newCompany);
    }

    private async Task LogInteraction()
    {
        Console.Write("Enter Customer ID or partial ID to log interaction for: ");
        string inputId = Console.ReadLine();
        var customer = FindCustomerByPartialId(inputId);

        if (customer == null)
        {
            Console.WriteLine($"Customer with ID or partial ID '{inputId}' not found.");
            return;
        }

        Console.WriteLine($"\n--- Logging Interaction for {customer.FirstName} {customer.LastName} ---");
        Console.WriteLine("Select Interaction Type:");
        foreach (InteractionType type in Enum.GetValues(typeof(InteractionType)))
        {
            Console.WriteLine($"{(int)type}. {type}");
        }
        Console.Write("Enter type number: ");
        InteractionType interactionType = (InteractionType)int.Parse(Console.ReadLine());

        Console.Write("Enter Notes: ");
        string notes = Console.ReadLine();

        _crmService.LogCustomerInteraction(customer.CustomerId, interactionType, notes);
    }

    private async Task SearchCustomers()
    {
        Console.Write("Enter search term (name, email, company): ");
        string searchTerm = Console.ReadLine();
        _crmService.ListCustomers(searchTerm);
    }

    private async Task ManageUsers()
    {
        if (_authService.CurrentUser.Role < UserRole.Admin)
        {
            Console.WriteLine("Access Denied: You must be an Administrator to manage users.");
            return;
        }

        Console.WriteLine("\n--- User Management (Admin Only) ---");
        Console.WriteLine("1. Register New User");
        Console.WriteLine("2. Delete User (Not Implemented in this simplified example)");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("New Username: ");
                string newUsername = Console.ReadLine();
                Console.Write("New Password: ");
                string newPassword = ReadPasswordMasked();
                Console.WriteLine(); // New line after password

                Console.WriteLine("Select Role:");
                foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
                {
                    Console.WriteLine($"{(int)role}. {role}");
                }
                Console.Write("Enter role number: ");
                UserRole newRole = (UserRole)int.Parse(Console.ReadLine());

                _authService.RegisterUser(newUsername, newPassword, newRole, _authService.CurrentUser);
                break;
            case "2":
                Console.WriteLine("User deletion not implemented in this basic example.");
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }

    /// <summary>
    /// Helper to find a customer by full or partial ID.
    /// </summary>
    private Customer FindCustomerByPartialId(string partialId)
    {
        return _crmService.ListCustomers(null) // Get all customers (a bit inefficient for large lists, but fine for small demos)
                          .Where(c => c.CustomerId.StartsWith(partialId, StringComparison.OrdinalIgnoreCase))
                          .FirstOrDefault();
    }

    /// <summary>
    /// Reads password from console with masked input (*).
    /// </summary>
    private static string ReadPasswordMasked()
    {
        string pass = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true); // true means don't display the key pressed

            // Backspace should remove the last character
            if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
            {
                pass = pass.Substring(0, (pass.Length - 1));
                Console.Write("\b \b"); // Erase last char and backspace
            }
            else if (!char.IsControl(key.KeyChar)) // Append character if it's not a control key
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
        }
        while (key.Key != ConsoleKey.Enter); // Stop on Enter key
        return pass;
    }

    public static async Task Main(string[] args)
    {
        // Optional: Delete existing data files for a fresh start each run
        // if (File.Exists(CustomersDataFile)) File.Delete(CustomersDataFile);
        // if (File.Exists(UsersDataFile)) File.Delete(UsersDataFile);

        var app = new CrmApp();
        await app.Run();
    }
}