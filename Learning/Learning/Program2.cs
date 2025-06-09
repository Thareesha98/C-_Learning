using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    internal class Program2
    {
        public async Task<List<string>> FetchProductAsync()
        {
            try
            {
                await Task.Delay(2000);
                return new List<string>
                {
                    "Eco Bag",
                    "Reusable Straw"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching products: " + ex.Message);
                return new List<string>();
            }
        }

        public async Task DisplayProuctAsync()
        {
            var product = await FetchProductAsync();
            foreach (var item in product)
            {
                Console.WriteLine(item);
            }
        }

        //public static async Task Main(string[] args)
        //{
        //    Program2 program2 = new Program2();
        //    await program2.DisplayProuctAsync();
        //}
    }
}
