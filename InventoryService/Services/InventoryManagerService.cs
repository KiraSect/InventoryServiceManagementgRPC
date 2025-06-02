using Grpc.Core;
using InventoryService;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Text.Json;

namespace InventoryGrpcService.Services
{
    public class InventoryManagementService : InventoryManagement.InventoryManagementBase
    {
        private static readonly ConcurrentDictionary<int, Product> Products = new();
        private static int _nextId = 1;
        private static readonly string StorageFile = "products.json";
        private static readonly List<IServerStreamWriter<StockAlert>> AlertSubscribers = new();
        private const int LowStockThreshold = 5;

        public InventoryManagementService()
        {
            LoadProductsFromFile();
        }

        public override Task<Product> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = request.Product;

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product name cannot be empty"));

            if (product.Quantity < 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Quantity cannot be negative"));

            product.Id = Interlocked.Increment(ref _nextId);
            Products[product.Id] = product;
            SaveProductsToFile();

            LogStockChange(product.Id, product.Quantity);

            return Task.FromResult(product);
        }

        public override Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            if (!Products.TryGetValue(request.Id, out var product))
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with id={request.Id} not found"));

            return Task.FromResult(product);
        }

        public override Task<Product> UpdateStock(UpdateStockRequest request, ServerCallContext context)
        {
            if (!Products.TryGetValue(request.ProductId, out var product))
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with id={request.ProductId} not found"));

            if (product.Quantity + request.Delta < 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Stock cannot be negative"));

            product.Quantity += request.Delta;
            SaveProductsToFile();

            LogStockChange(product.Id, request.Delta);

            if (product.Quantity < LowStockThreshold)
            {
                var alert = new StockAlert
                {
                    Product = product,
                    Message = $"Low stock alert for '{product.Name}', quantity = {product.Quantity}"
                };
                _ = NotifySubscribersAsync(alert);
            }

            return Task.FromResult(product);
        }

        public override Task<Empty> RemoveProduct(RemoveProductRequest request, ServerCallContext context)
        {
            if (!Products.TryRemove(request.Id, out var removedProduct))
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with id={request.Id} not found"));

            SaveProductsToFile();

            LogStockChange(request.Id, -removedProduct.Quantity);

            return Task.FromResult(new Empty());
        }

        public override Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
        {
            var response = new ListProductsResponse();
            response.Products.AddRange(Products.Values);
            return Task.FromResult(response);
        }

        public override async Task StreamStockAlerts(Empty request, IServerStreamWriter<StockAlert> responseStream, ServerCallContext context)
        {
            lock (AlertSubscribers)
            {
                AlertSubscribers.Add(responseStream);
            }

            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                lock (AlertSubscribers)
                {
                    AlertSubscribers.Remove(responseStream);
                }
            }
        }

        private async Task NotifySubscribersAsync(StockAlert alert)
        {
            List<IServerStreamWriter<StockAlert>> subscribersCopy;
            lock (AlertSubscribers)
            {
                subscribersCopy = new List<IServerStreamWriter<StockAlert>>(AlertSubscribers);
            }

            foreach (var subscriber in subscribersCopy)
            {
                try
                {
                    await subscriber.WriteAsync(alert);
                }
                catch
                {
                }
            }
        }

        private void SaveProductsToFile()
        {
            try
            {
                var list = Products.Values.ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(StorageFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving products: {ex.Message}");
            }
        }

        private void LoadProductsFromFile()
        {
            if (File.Exists(StorageFile))
            {
                try
                {
                    var json = File.ReadAllText(StorageFile);
                    var list = JsonSerializer.Deserialize<List<Product>>(json);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var product in list)
                        {
                            Products[product.Id] = product;
                            if (product.Id >= _nextId)
                                _nextId = product.Id + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading products: {ex.Message}");
                }
            }
        }

        private void LogStockChange(int productId, int delta)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Console.WriteLine($"[StockChange] ProductId: {productId}, Delta: {delta}, Timestamp: {timestamp}");
        }
    }
}
