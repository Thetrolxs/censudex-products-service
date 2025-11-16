using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_products_service.src.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 a 150 caracteres.")]
        public required string Name { get; set; } 

        [StringLength(5000, ErrorMessage = "La descripción no puede superar los 5000 caracteres.")]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public float Price { get; set; }

        [Required]
        public required string Category { get; set; }

        [Required]
        public required IFormFile? Image { get; set; }
    }
}