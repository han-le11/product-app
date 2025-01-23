using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace ProductAggregator
{
    class Program
    {
        /// <summary>
        /// Main entry point of the program, with console logging to track progress and error.
        /// </summary>
        /// <returns></returns>
        static async Task Main()
        {
            Console.WriteLine("Starting product aggregation...");
            
            try
            {
                var products = await FetchProductsAsync();
                var groupedProducts = GroupProductsByCategory(products);
                string outputFilePath = "grouped_products.json";
                await SaveGroupedProductsToJsonFile(groupedProducts, outputFilePath);

                Console.WriteLine($"Product aggregation completed. Output saved to {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetch products from the Fake Store API with retry logic.
        /// </summary>
        /// <returns>Products fetched from the API.</returns>
        static async Task<List<Product>> FetchProductsAsync()
        {
            const string apiUrl = "https://fakestoreapi.com/products";
            using var httpClient = new HttpClient();
            
            // Retry up to 3 times (with 2 second intervals) if API call fails
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    Console.WriteLine("Fetching products from API...");

                    var response = await httpClient.GetAsync(apiUrl);

                    response.EnsureSuccessStatusCode(); 

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<Product>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (products == null || products.Count == 0)
                    {
                        throw new Exception("No products found in API response.");  // Throw exception if no products found
                    }

                    return products;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching products (attempt {retry + 1}): {ex.Message}");

                    if (retry == 2) throw; // Re-throw after 3rd failed attempt
                }

                await Task.Delay(2000); // Wait 2 seconds before retrying
            }

            return new List<Product>();
        }

        /// <summary>
        /// Group products by category.
        /// </summary>
        /// <param name="products"></param>
        /// <returns>A list of products grouped by category.</returns>
        static Dictionary<string, List<GroupedProduct>> GroupProductsByCategory(List<Product> products)
        {   
            const string DefaultCategory = "Uncategorized";

            Console.WriteLine("Grouping products by category...");

            return products
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Category) ? DefaultCategory : p.Category)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(p => new GroupedProduct
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Price = p.Price
                    }).OrderBy(p => p.Price).ToList()
                );
        }

        /// <summary>
        /// Save grouped products to JSON file.
        /// </summary>
        /// <param name="groupedProducts"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static async Task SaveGroupedProductsToJsonFile(Dictionary<string, List<GroupedProduct>> groupedProducts, string filePath)
        {
            Console.WriteLine("Saving grouped products to JSON file...");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true  // Format the output JSON with indentation for readability
            };

            var jsonProductContent = JsonSerializer.Serialize(groupedProducts, options);

            await File.WriteAllTextAsync(filePath, jsonProductContent); 
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }

    public class GroupedProduct
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
    }
}
