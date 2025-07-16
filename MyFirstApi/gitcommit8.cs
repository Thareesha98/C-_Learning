using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManager
{
    class Program
    {
        static void Main(string[] args)
        {
            LibraryApp app = new LibraryApp();
            app.Run();
        }
    }

    public class LibraryApp
    {
        private Library library = new Library();

        public void Run()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Library Management System ===");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. List Books");
                Console.WriteLine("3. Register Borrower");
                Console.WriteLine("4. Borrow Book");
                Console.WriteLine("5. Return Book");
                Console.WriteLine("6. View Borrowed Books");
                Console.WriteLine("7. Exit");
                Console.Write("Choose an option: ");

                switch (Console.ReadLine())
                {
                    case "1": AddBook(); break;
                    case "2": ListBooks(); break;
                    case "3": RegisterBorrower(); break;
                    case "4": BorrowBook(); break;
                    case "5": ReturnBook(); break;
                    case "6": ViewBorrowedBooks(); break;
                    case "7": exit = true; break;
                    default: Console.WriteLine("Invalid choice."); break;
                }

                if (!exit)
                {
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
        }

        private void AddBook()
        {
            Console.Write("Enter book title: ");
            string title = Console.ReadLine();
            Console.Write("Enter author: ");
            string author = Console.ReadLine();
            Console.Write("Enter ISBN: ");
            string isbn = Console.ReadLine();

            Book book = new Book(title, author, isbn);
            library.AddBook(book);
            Console.WriteLine("Book added successfully.");
        }

        private void ListBooks()
        {
            var books = library.GetAvailableBooks();
            Console.WriteLine("Available Books:");
            if (books.Count == 0)
            {
                Console.WriteLine("No books available.");
                return;
            }

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }

        private void RegisterBorrower()
        {
            Console.Write("Enter borrower name: ");
            string name = Console.ReadLine();
            Console.Write("Enter borrower ID: ");
            string id = Console.ReadLine();

            Borrower borrower = new Borrower(name, id);
            library.RegisterBorrower(borrower);
            Console.WriteLine("Borrower registered.");
        }

        private void BorrowBook()
        {
            Console.Write("Enter borrower ID: ");
            string borrowerId = Console.ReadLine();
            Console.Write("Enter book ISBN: ");
            string isbn = Console.ReadLine();

            bool success = library.BorrowBook(borrowerId, isbn);
            Console.WriteLine(success ? "Book borrowed." : "Borrowing failed.");
        }

        private void ReturnBook()
        {
            Console.Write("Enter borrower ID: ");
            string borrowerId = Console.ReadLine();
            Console.Write("Enter book ISBN: ");
            string isbn = Console.ReadLine();

            bool success = library.ReturnBook(borrowerId, isbn);
            Console.WriteLine(success ? "Book returned." : "Return failed.");
        }

        private void ViewBorrowedBooks()
        {
            Console.Write("Enter borrower ID: ");
            string borrowerId = Console.ReadLine();

            var books = library.GetBorrowedBooks(borrowerId);
            Console.WriteLine("Borrowed Books:");
            if (books.Count == 0)
            {
                Console.WriteLine("No books borrowed.");
                return;
            }

            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }
    }

    public class Library
    {
        private List<Book> books = new List<Book>();
        private List<Borrower> borrowers = new List<Borrower>();
        private List<Loan> loans = new List<Loan>();

        public void AddBook(Book book)
        {
            books.Add(book);
        }

        public List<Book> GetAvailableBooks()
        {
            var borrowedIsbns = loans.Select(l => l.Book.ISBN).ToHashSet();
            return books.Where(b => !borrowedIsbns.Contains(b.ISBN)).ToList();
        }

        public void RegisterBorrower(Borrower borrower)
        {
            borrowers.Add(borrower);
        }

        public bool BorrowBook(string borrowerId, string isbn)
        {
            var borrower = borrowers.FirstOrDefault(b => b.ID == borrowerId);
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (borrower == null || book == null || loans.Any(l => l.Book.ISBN == isbn))
                return false;

            loans.Add(new Loan(borrower, book, DateTime.Now));
            return true;
        }

        public bool ReturnBook(string borrowerId, string isbn)
        {
            var loan = loans.FirstOrDefault(l => l.Borrower.ID == borrowerId && l.Book.ISBN == isbn);
            if (loan == null)
                return false;

            loans.Remove(loan);
            return true;
        }

        public List<Book> GetBorrowedBooks(string borrowerId)
        {
            return loans.Where(l => l.Borrower.ID == borrowerId).Select(l => l.Book).ToList();
        }
    }

    public class Book
    {
        public string Title { get; }
        public string Author { get; }
        public string ISBN { get; }

        public Book(string title, string author, string isbn)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
        }

        public override string ToString()
        {
            return $"{Title} by {Author} (ISBN: {ISBN})";
        }
    }

    public class Borrower
    {
        public string Name { get; }
        public string ID { get; }

        public Borrower(string name, string id)
        {
            Name = name;
            ID = id;
        }

        public override string ToString()
        {
            return $"{Name} (ID: {ID})";
        }
    }

    public class Loan
    {
        public Borrower Borrower { get; }
        public Book Book { get; }
        public DateTime BorrowedDate { get; }

        public Loan(Borrower borrower, Book book, DateTime borrowedDate)
        {
            Borrower = borrower;
            Book = book;
            BorrowedDate = borrowedDate;
        }

        public override string ToString()
        {
            return $"{Book.Title} borrowed by {Borrower.Name} on {BorrowedDate.ToShortDateString()}";
        }
    }
}
