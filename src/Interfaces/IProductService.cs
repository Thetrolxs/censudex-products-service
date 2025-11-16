using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_products_service.src.DTOs;

namespace censudex_products_service.src.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetProductsAsync();
        Task<ProductResponseDto?> GetProductByIdAsync(Guid id);
        Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}