using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InventoryService;
using Google.Protobuf.WellKnownTypes;

namespace InventoryService.Repositories
{
    public class InventoryRepository
    {
        private readonly ConcurrentDictionary<int, Product> _products = new();
        private int _nextId = 1;
        public Product AddProduct(Product product)
        {
            var id = _nextId++;
            var newProduct = new Product
            {
                Id = id,
                Name = product.Name,
                Quantity = product.Quantity
            };
            _products[id] = newProduct;
            return newProduct;
        }

        public Product? GetProduct(int id)
        {
            _products.TryGetValue(id, out var product);
            return product;
        }

        public Product? UpdateStock(int productId, int delta)
        {
            if (_products.TryGetValue(productId, out var product))
            {
                product.Quantity += delta;
                return product;
            }
            return null;
        }

        public bool RemoveProduct(int id)
        {
            return _products.TryRemove(id, out _);
        }

        public List<Product> ListProducts()
        {
            return new List<Product>(_products.Values);
        }
    }
}
