using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace ProductAggregator
{
    public class Program
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
                string outputFilePath = "grouped_products.json";  // Default output file path
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

            // Use the using statement to ensure the HttpClient is properly disposed of
            using var httpClient = new HttpClient();
            
            // Retry up to 3 times (with 2 second intervals) if API call fails
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    Console.WriteLine("Fetching products from API...");

                    var response = await httpClient.GetAsync(apiUrl);  // Send a GET request to the endpoint

                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();  // Read the response content as a string

                    // Deserialize the JSON response content into a list of Product objects. 
                    // Set an option to ignore case sensitivity in property names.
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
        /// If a product’s category is empty or has only whitespace, by default it is assigned to Uncategorized. 
        /// For each product, include Id, Title, and Price.
        /// </summary>
        /// <param name="products"></param>
        /// <returns>A dictionary, where key is a category and value is a list of products grouped by category.</returns>
        public static Dictionary<string, List<GroupedProduct>> GroupProductsByCategory(List<Product> products)
        {   
            const string DefaultCategory = "Uncategorized";

            Console.WriteLine("Grouping products by category...");

            // Group products by Category (which acts as dictionary key) and sort each group by ascending price 
            return products
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Category) ? DefaultCategory : p.Category)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(p => new GroupedProduct
                    {
                        Id = p.Id,
                   