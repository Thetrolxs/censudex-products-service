using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_products_service.src.Models;

namespace censudex_products_service.src.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> CreateAsync(Product product);
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<bool> UpdateAsync(Product product);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<bool> IsDuplicateByName(string name); 
    }
}