using System;
using System.Collections.Generic;

namespace HabitTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsoleApp().Run();
        }
    }

    public class ConsoleApp
    {
        private UserManager _userManager = new UserManager();
        private HabitManager _habitManager = new HabitManager();
        private User _currentUser;

        public void Run()
        {
            while (_currentUser == null)
            {
                Console.Clear();
                Console.WriteLine("Welcome to Habit Tracker");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.Write("Choose: ");
                string input = Console.ReadLine();

                if (input == "1")
                    _currentUser = _userManager.Login();
                else if (input == "2")
                    _currentUser = _userManager.Register();
                else
                    Console.WriteLine("Invalid choice.");
            }

            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine($"Logged in as: {_currentUser.Username}");
                Console.WriteLine("1. Create Habit");
                Console.WriteLine("2. View Habits");
                Console.WriteLine("3. Mark Habit Done");
                Console.WriteLine("4. View Progress");
                Console.WriteLine("5. Logout");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": _habitManager.CreateHabit(_currentUser); break;
                    case "2": _habitManager.ViewHabits(_currentUser); break;
                    case "3": _habitManager.MarkHabitDone(_currentUser); break;
                    case "4": _habitManager.ViewProgress(_currentUser); break;
                    case "5": exit = true; _currentUser = null; break;
                    default: Console.WriteLine("Invalid choice."); break;
                }

                if (!exit)
                {
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
            }

            Run(); // Return to login
        }
    }

    public class UserManager
    {
        private List<User> _users = new List<User>();

        public User Login()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            foreach (var user in _users)
            {
                if (user.Username == username)
                {
                    Console.WriteLine("Login successful.");
                    return user;
                }
            }

            Console.WriteLine("User not found.");
            return null;
        }

        public User Register()
        {
            Console.Write("Choose a username: ");
            string username = Console.ReadLine();

            foreach (var user in _users)
            {
                if (user.Username == username)
                {
                    Console.WriteLine("Username already taken.");
                    return null;
                }
            }

            User newUser = new User { Username = username };
            _users.Add(newUser);
            Console.WriteLine("Registration successful.");
            return newUser;
        }
    }

    public class HabitManager
    {
        public void CreateHabit(User user)
        {
            Console.Write("Enter habit name: ");
            string name = Console.ReadLine();

            Habit newHabit = new Habit
            {
                Name = name,
                CreatedDate = DateTime.Now,
                CompletionDates = new List<DateTime>()
            };

            user.Habits.Add(newHabit);
            Console.WriteLine("Habit created.");
        }

        public void ViewHabits(User user)
        {
            Console.WriteLine("Your Habits:");
            if (user.Habits.Count == 0)
            {
                Console.WriteLine("No habits yet.");
                return;
            }

            for (int i = 0; i < user.Habits.Count; i++)
            {
                var h = user.Habits[i];
                Console.WriteLine($"{i + 1}. {h.Name} | Created: {h.CreatedDate.ToShortDateString()} | Completions: {h.CompletionDates.Count}");
            }
        }

        public void MarkHabitDone(User user)
        {
            ViewHabits(user);
            Console.Write("Enter habit number to mark as done: ");
            if (int.TryParse(Console.ReadLine(), out int index))
            {
                index--;
                if (index >= 0 && index < user.Habits.Count)
                {
                    user.Habits[index].CompletionDates.Add(DateTime.Now);
                    Console.WriteLine("Habit marked as done.");
                }
                else
                {
                    Console.WriteLine("Invalid index.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }
        }

        public void ViewProgress(User user)
        {
            Console.WriteLine("Habit Progress:");
            foreach (var h in user.Habits)
            {
                Console.WriteLine($"- {h.Name}: {h.CompletionDates.Count} times");
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public List<Habit> Habits { get; set; } = new List<Habit>();
    }

    public class Habit
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<DateTime> CompletionDates { get; set; }
    }
}
