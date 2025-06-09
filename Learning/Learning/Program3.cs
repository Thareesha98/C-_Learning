using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning { 
    public class Product{
        public string Name { get; set; }

        public Product(string name)
        {
            Name = name;
        }
    }

    public class Review
    {
        public string Content { get; set; }

        public Review(string content)
        {
            Content = content;
        }
    }




    internal class Program3
    {
        public async Task<List<Product>> FetchProductsAsync()
        {
            await Task.Delay(2000);
            return new List<Product>
            {
                new Product("Eraser"),
                new Product("Pencil"),
                new Product("Notebook"),
                new Product("Pen"),
            };

        }

        public async Task<List<Review>> FetchReviewsAsync()
        {
            await Task.Delay(3000); // Simulating a 3-second delay for fetching reviews
            return new List<Review>
        {
            new Review("Great product!"),
            new Review("Good value for the money."),
        };
        }

        public async Task FetchDataAsync()
        {
            Task<List<Product>> productTask = FetchProductsAsync();
            Task<List<Review>> reviewsTask = FetchReviewsAsync();

            await Task.WhenAll(productTask, reviewsTask);
            List<Product> products = await productTask;
            List<Review> reviews = await reviewsTask;

            Console.WriteLine("Products:");
            foreach (Product product in products)
            {
                Console.WriteLine(product.Name);
            }

            foreach (Review review in reviews)
            {
                Console.WriteLine(review.Content);
            }

        }


        //public static async Task Main(string[] args)
        //{
        //    // Calling the asynchronous method to fetch and display products and reviews
        //    Program3 program = new Program3();
        //    await program.FetchDataAsync();
        //}

    }
    }
