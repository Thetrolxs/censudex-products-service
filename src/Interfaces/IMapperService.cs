using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_products_service.src.DTOs;
using censudex_products_service.src.Models;

namespace censudex_products_service.src.Interfaces
{
    public interface IMapperService
    {
        ProductResponseDto ProductResponse(Product product);
        Product CreateDtoToProduct(CreateProductDto createProductDto);
        void UpdateTicketFromDto(Product product, UpdateProductDto updateProductDto);
    }
}