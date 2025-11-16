using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_products_service.src.DTOs
{
    public class UpdateProductDto
    {
        [StringLength(150, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public float? Price { get; set; }

        public string? Category { get; set; }

        public IFormFile? Image { get; set; }        
    }
}