# Overview 
This is an integration service that fetches product data from a public e-commerce API. The goal is to group products by their categories and save the results in a structured JSON format.

# Build and run
I have built my app to be compatible with .Net 6.0. So please make sure that you have installed .Net SDK 6.0.428.

To build and run the ProductAggregator app, please navigate to `ProductAggregator`
```
cd ProductAggregator
dotnet build
dotnet run
```

The output JSON file of products grouped by their categories can be found in `grouped_products.json`.  

# Testing
Unit tests still need to be added. Necessary test cases for example are the products fetched from https://fakestoreapi.com/products are grouped and sorted correctly. 


