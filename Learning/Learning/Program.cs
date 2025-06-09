using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    class Program
    {
        public async Task DownloadDataAsync()
        {
            try
            {
                Console.WriteLine("Downloading data...");
                throw new InvalidOperationException("Simulated download error.");
                await Task.Delay(1000);
                Console.WriteLine("Data downloaded 1 successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");

            }
        }

        public async Task DownloadDataAsync2()
        {
            Console.WriteLine("Downloading data 2 ...");
            await Task.Delay(3000);
            Console.WriteLine("Data downloaded 2  successfully.");
        }

        //public static async Task Main(string[] args)
        //{
        //    Console.WriteLine("Hello, World!");
        //    Program program = new Program();
        //    Task task1 = program.DownloadDataAsync();
        //    Task task2 = program.DownloadDataAsync2();
        //    await Task.WhenAll(task1, task2);
        //    Console.WriteLine("All downloads completed.");
        //}
    }
}
