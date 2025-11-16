using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_products_service.src.Data;
using censudex_products_service.src.Interfaces;
using censudex_products_service.src.Models;
using MongoDB.Driver;

namespace censudex_products_service.src.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(MongoContext context)
        {
            _collection = context.Products;
        }
        public async Task<bool> CreateAsync(Product product)
        {
            await _collection.InsertOneAsync(product);
            return true;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _collection.Find(p => p.IsActive).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _collection.Find(p => p.Id == id && p.IsActive).FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateByName(string name)
        {
            var count = await _collection.CountDocumentsAsync(p => p.Name == name && p.IsActive);
            return count > 0;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var update = Builders<Product>.Update.Set(p => p.IsActive, false);
            await _collection.UpdateOneAsync(p => p.Id == id, update);
            return true;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            await _collection.ReplaceOneAsync(t => t.Id == product.Id, product);
            return true;
        }
    }
}