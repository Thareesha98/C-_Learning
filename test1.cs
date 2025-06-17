using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem
{
    public class Book
    {
        public string Title { get; set; }
        public bool IsCheckedOut { get; set; }

        public Book(string title)
        {
            Title = title;
            IsCheckedOut = false;
        }
    }

    public class User
    {
        public string Name { get; set; }
        public List<Book> BorrowedBooks { get; set; } = new List<Book>();

        public User(string name)
        {
            Name = name;
        }

        public bool CanBorrow()
        {
            return BorrowedBooks.Count < 3;
        }

        public void BorrowBook(Book book)
        {
            if (!CanBorrow())
            {
                Console.WriteLine("You have reached your borrowing limit (3 books).");
                return;
            }

            if (book.IsCheckedOut)
            {
                Console.WriteLine($"'{book.Title}' is already checked out.");
                return;
            }

            book.IsCheckedOut = true;
            BorrowedBooks.Add(book);
            Console.WriteLine($"You have borrowed '{book.Title}'.");
        }

        public void ReturnBook(Book book)
        {
            if (BorrowedBooks.Contains(book))
            {
                book.IsCheckedOut = false;
                BorrowedBooks.Remove(book);
                Console.WriteLine($"You have returned '{book.Title}'.");
            }
            else
            {
                Console.WriteLine($"You did not borrow '{book.Title}'.");
            }
        }

        public void ListBorrowedBooks()
        {
            Console.WriteLine($"{Name}'s Borrowed Books:");
            if (BorrowedBooks.Count == 0)
            {
                Console.WriteLine("  None");
                return;
            }

            foreach (var book in BorrowedBooks)
            {
                Console.WriteLine($"  - {book.Title}");
            }
        }
    }

    class Program
    {
        static List<Book> libraryBooks = new List<Book>
        {
            new Book("The Great Gatsby"),
            new Book("1984"),
            new Book("To Kill a Mockingbird"),
            new Book("The Catcher in the Rye"),
            new Book("Moby-Dick")
        };

        static void Main()
        {
            Console.WriteLine("Welcome to the Library Management System");
            Console.Write("Enter your name to log in: ");
            string userName = Console.ReadLine();
            User currentUser = new User(userName);

            while (true)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. View All Books");
                Console.WriteLine("2. Search for a Book");
                Console.WriteLine("3. Borrow a Book");
                Console.WriteLine("4. Return a Book");
                Console.WriteLine("5. My Borrowed Books");
                Console.WriteLine("6. Exit");
                Console.Write("Choose an option (1-6): ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        ListAllBooks();
                        break;

                    case "2":
                        Console.Write("Enter book title to search: ");
                        string searchTitle = Console.ReadLine();
                        SearchBook(searchTitle);
                        break;

                    case "3":
                        Console.Write("Enter book title to borrow: ");
                        string borrowTitle = Console.ReadLine();
                        BorrowBook(currentUser, borrowTitle);
                        break;

                    case "4":
                        Console.Write("Enter book title to return: ");
                        string returnTitle = Console.ReadLine();
                        ReturnBook(currentUser, returnTitle);
                        break;

                    case "5":
                        currentUser.ListBorrowedBooks();
                        break;

                    case "6":
                        Console.WriteLine("Thank you for using the Library Management System. Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid option. Please choose 1-6.");
                        break;
                }
            }
        }

        static void ListAllBooks()
        {
            Console.WriteLine("Library Books:");
            foreach (var book in libraryBooks)
            {
                string status = book.IsCheckedOut ? "Checked Out" : "Available";
                Console.WriteLine($"  - {book.Title} [{status}]");
            }
        }

        static void SearchBook(string title)
        {
            var book = libraryBooks.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                Console.WriteLine($"'{book.Title}' is in the library and is currently {(book.IsCheckedOut ? "checked out" : "available")}.");
            }
            else
            {
                Console.WriteLine($"'{title}' is not in the collection.");
            }
        }

        static void BorrowBook(User user, string title)
        {
            var book = libraryBooks.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                user.BorrowBook(book);
            }
            else
            {
                Console.WriteLine($"'{title}' is not available in the library.");
            }
        }

        static void ReturnBook(User user, string title)
        {
            var book = libraryBooks.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                user.ReturnBook(book);
            }
            else
            {
                Console.WriteLine($"'{title}' is not recognized in the library.");
            }
        }
    }
}
