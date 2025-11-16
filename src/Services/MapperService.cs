using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using censudex_products_service.src.DTOs;
using censudex_products_service.src.Interfaces;
using censudex_products_service.src.Models;

namespace censudex_products_service.src.Services
{
    public class MapperService : IMapperService
    {
        private readonly IMapper _mapper;
        public MapperService(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        public Product CreateDtoToProduct(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            return product;
        }

        public ProductResponseDto ProductResponse(Product product)
        {
            var response = _mapper.Map<ProductResponseDto>(product);
            return response;
        }

        public void UpdateTicketFromDto(Product product, UpdateProductDto updateProductDto)
        {
            _mapper.Map(updateProductDto, product);
            product.UpdateAt = DateTime.UtcNow;
        }
    }
}