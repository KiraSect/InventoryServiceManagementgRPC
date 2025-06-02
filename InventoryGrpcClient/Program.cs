using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using InventoryService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace InventoryGrpcClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5160");
            var client = new InventoryManagement.InventoryManagementClient(channel);

            while (true)
            {
                Console.WriteLine("\n=== Inventory Management Menu ===");
                Console.WriteLine("1. List all products");
                Console.WriteLine("2. Add product");
                Console.WriteLine("3. Get product by ID");
                Console.WriteLine("4. Update product stock");
                Console.WriteLine("5. Remove product");
                Console.WriteLine("6. Exit");
                Console.Write("Select option: ");

                var input = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            await ListProductsAsync(client);
                            break;

                        case "2":
                            await AddProductAsync(client);
                            break;

                        case "3":
                            await GetProductByIdAsync(client);
                            break;

                        case "4":
                            await UpdateProductStockAsync(client);
                            break;

                        case "5":
                            await RemoveProductAsync(client);
                            break;

                        case "6":
                            Console.WriteLine("Exiting...");
                            return;

                        default:
                            Console.WriteLine("Invalid option, please try again.");
                            break;
                    }
                }
                catch (RpcException rpcEx)
                {
                    Console.WriteLine($"gRPC Error ({rpcEx.Status.StatusCode}): {rpcEx.Status.Detail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }

        private static async Task ListProductsAsync(InventoryManagement.InventoryManagementClient client)
        {
            var response = await client.ListProductsAsync(new ListProductsRequest());
            if (response.Products.Count == 0)
            {
                Console.WriteLine("No products found.");
                return;
            }

            Console.WriteLine("Products:");
            foreach (var p in response.Products)
                Console.WriteLine($" - Id={p.Id}, Name={p.Name}, Quantity={p.Quantity}");
        }

        private static async Task AddProductAsync(InventoryManagement.InventoryManagementClient client)
        {
            Console.Write("Enter product name: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Product name cannot be empty.");
                return;
            }

            Console.Write("Enter quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 0)
            {
                Console.WriteLine("Invalid quantity. Must be a non-negative integer.");
                return;
            }

            var product = new Product { Name = name, Quantity = quantity };
            var added = await client.AddProductAsync(new AddProductRequest { Product = product });
            Console.WriteLine($"Added: Id={added.Id}, Name={added.Name}, Quantity={added.Quantity}");
        }

        private static async Task GetProductByIdAsync(InventoryManagement.InventoryManagementClient client)
        {
            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
            {
                Console.WriteLine("Invalid ID. Must be a positive integer.");
                return;
            }

            try
            {
                var product = await client.GetProductAsync(new GetProductRequest { Id = id });
                Console.WriteLine($"Product: Id={product.Id}, Name={product.Name}, Quantity={product.Quantity}");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                Console.WriteLine($"Product with id={id} not found.");
            }
        }

        private static async Task UpdateProductStockAsync(InventoryManagement.InventoryManagementClient client)
        {
            Console.Write("Enter product ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
            {
                Console.WriteLine("Invalid ID. Must be a positive integer.");
                return;
            }

            Console.Write("Enter stock change (positive or negative integer): ");
            if (!int.TryParse(Console.ReadLine(), out int delta))
            {
                Console.WriteLine("Invalid input for stock change.");
                return;
            }

            try
            {
                var updated = await client.UpdateStockAsync(new UpdateStockRequest { ProductId = id, Delta = delta });
                Console.WriteLine($"Updated: Id={updated.Id}, Name={updated.Name}, Quantity={updated.Quantity}");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                Console.WriteLine($"Product with id={id} not found.");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
            {
                Console.WriteLine($"Invalid operation: {ex.Status.Detail}");
            }
        }
        private static async Task RemoveProductAsync(InventoryManagement.InventoryManagementClient client)
        {
            Console.Write("Enter product ID to remove: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
            {
                Console.WriteLine("Invalid ID. Must be a positive integer.");
                return;
            }

            try
            {
                Console.WriteLine("Calling RemoveProductAsync...");
                await client.RemoveProductAsync(new RemoveProductRequest { Id = id });
                Console.WriteLine($"Product with Id={id} removed.");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Caught RpcException: {ex.StatusCode} - {ex.Status.Detail}");

                if (ex.StatusCode == StatusCode.NotFound)
                {
                    Console.WriteLine($"Product with id={id} not found.");
                }
                else
                {
                    Console.WriteLine($"gRPC Error ({ex.StatusCode}): {ex.Status.Detail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            Console.WriteLine("End of RemoveProductAsync");
        }

    }
}