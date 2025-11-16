using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_products_service.src.DTOs;
using censudex_products_service.src.Interfaces;

namespace censudex_products_service.src.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICloudinaryService _cloudinary;
        private readonly IMapperService _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ICloudinaryService cloudinary, IMapperService mapper, ILogger<ProductService> logger)
        {
            _repo = repo;
            _cloudinary = cloudinary;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            if (await _repo.IsDuplicateByName(createProductDto.Name)) throw new InvalidOperationException("Nombre de producto duplicado.");

            var product = _mapper.CreateDtoToProduct(createProductDto);

            var updloadResult = await _cloudinary.UploadImageAsync(createProductDto.Image!);

            product.IsActive = true;
            product.ImageUrl = updloadResult;

            var created = await _repo.CreateAsync(product);
            if (!created)
            {
                _logger.LogError("No se pudo crear el producto en la base de datos.");
                throw new Exception("Error al crear producto.");
            }

            return _mapper.ProductResponse(product);
        }


        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return null;

            return _mapper.ProductResponse(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync()
        {
            var products = await _repo.GetAllAsync();
            var list = new List<ProductResponseDto>();
            foreach (var p in products)
            {
                list.Add(_mapper.ProductResponse(p));
            }

            return list;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var exists = await _repo.GetByIdAsync(id);
            if (exists == null) return false;

            var result = await _repo.SoftDeleteAsync(id);

            return result;
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return null;

            if (!string.IsNullOrWhiteSpace(updateProductDto.Name) && !updateProductDto.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _repo.IsDuplicateByName(updateProductDto.Name)) throw new InvalidOperationException("Nombre de producto duplicado.");
            }

            if (updateProductDto.Image != null)
            {
                var updload = await _cloudinary.UploadImageAsync(updateProductDto.Image);

                if (!string.IsNullOrEmpty(updload)) _logger.LogError("No se retorno URL");

                product.ImageUrl = updload;
            }

            _mapper.UpdateTicketFromDto(product, updateProductDto);

            var updated = await _repo.UpdateAsync(product);
            if (!updated)
            {
                _logger.LogError("No se pudo actualizar el producto con ID {id}", id);
                throw new Exception("Error al actualizar producto.");
            }

            return _mapper.ProductResponse(product);
        }
    }
}