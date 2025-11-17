using censudex_products_service.src.DTOs;
using censudex_products_service.src.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using products;

namespace censudex_products_service.src.Services
{
    public class ProductsGrpcService : ProductsService.ProductsServiceBase
    {
        private readonly IProductService _productService;
        private readonly IMapperService _mapperService;
        private readonly ILogger<ProductsGrpcService> _logger;

        public ProductsGrpcService(IProductService productService, IMapperService mapperService, ILogger<ProductsGrpcService> logger)
        {
            _productService = productService;
            _mapperService = mapperService;
            _logger = logger;
        }

        #region Helpers

        /// <summary>
        /// Convierte ProductResponseDto (DTO) a ProductResponse (proto)
        /// </summary>
        private static ProductResponse ToProtoProductResponse(ProductResponseDto dto)
        {
            var proto = new ProductResponse
            {
                Id = dto.Id.ToString(),
                Name = dto.Name ?? string.Empty,
                Category = dto.Category ?? string.Empty,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                IsActive = dto.IsActive,
                Description = dto.Description ?? string.Empty,
            };

            // Si CreateAt es DateTime:
            proto.CreateAt = Timestamp.FromDateTime(
                dto.CreateAt.ToUniversalTime()
            );
            
            // Si UpdateAt es DateTime:
            proto.UpdateAt = Timestamp.FromDateTime(
                dto.UpdateAt.ToUniversalTime()
            );

            return proto;
        }

        /// <summary>
        /// Crea un IFormFile (FormFile) desde bytes en memoria para reutilizar tu servicio de Cloudinary.
        /// Observación: FormFile usa el MemoryStream vivo; se procesa inmediatamente por CreateProductAsync / UploadImageAsync.
        /// </summary>
        private static IFormFile? ByteArrayToFormFile(byte[]? bytes, string? fileName, string? contentType)
        {
            if (bytes == null || bytes.Length == 0) return null;

            var ms = new MemoryStream(bytes);
            ms.Position = 0;

            // En .NET Core, FormFile se construye con (Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
            var formFile = new FormFile(ms, 0, bytes.Length, "image", fileName ?? "file")
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType ?? "application/octet-stream"
            };

            return formFile;
        }

        #endregion

        public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                var resp = new ListProductsResponse();
                resp.Products.AddRange(products.Select(ToProtoProductResponse));
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListProducts gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id requerido"));

                if (!Guid.TryParse(request.Id, out var guid))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id no es GUID válido"));

                var dto = await _productService.GetProductByIdAsync(guid);
                if (dto == null)
                    throw new RpcException(new Status(StatusCode.NotFound, "Producto no encontrado"));

                return ToProtoProductResponse(dto);
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetProduct gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        public override async Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
        {
            try
            {
                // Validaciones básicas (puedes delegarlas al servicio)
                if (string.IsNullOrWhiteSpace(request.Name))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Name es requerido"));
                if (string.IsNullOrWhiteSpace(request.Category))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Category es requerido"));

                // Mapear CreateProductRequest -> CreateProductDto
                var createDto = new CreateProductDto
                {
                    Name = request.Name,
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                    Price = request.Price,
                    Category = request.Category,
                    Image = null
                };

                // Si se envían bytes de imagen en el proto, convertir a IFormFile para reutilizar tu Cloudinary service.
                if (request.Image != null && request.Image.Length > 0)
                {
                    // request.Image es ByteString en C# (si en proto definiste bytes image = 5;)
                    var bytes = request.Image.ToByteArray();
                    var formFile = ByteArrayToFormFile(bytes, request.ImageFileName, request.ImageContentType);
                    createDto.Image = formFile;
                }

                ProductResponseDto created;
                try
                {
                    created = await _productService.CreateProductAsync(createDto);
                }
                catch (InvalidOperationException invOp)
                {
                    // Ejemplo: nombre duplicado
                    throw new RpcException(new Status(StatusCode.AlreadyExists, invOp.Message));
                }
                catch (ArgumentException argEx)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, argEx.Message));
                }

                return ToProtoProductResponse(created);
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateProduct gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        public override async Task<ProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id requerido"));

                if (!Guid.TryParse(request.Id, out var guid))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id no es GUID válido"));

                // Build UpdateProductDto. Tu UpdateProductDto ya tiene propiedades opcionales.
                var updateDto = new UpdateProductDto
                {
                    Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                    Price = request.Price == 0 ? (float?)null : request.Price, // Si en proto no usas wrappers: cuidado con 0
                    Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category,
                    Image = null
                };

                // Si envían imagen en bytes, conviértela
                if (request.Image != null && request.Image.Length > 0)
                {
                    var bytes = request.Image.ToByteArray();
                    var formFile = ByteArrayToFormFile(bytes, request.ImageFileName, request.ImageContentType);
                    updateDto.Image = formFile;
                }

                ProductResponseDto? updated;
                try
                {
                    updated = await _productService.UpdateProductAsync(guid, updateDto);
                }
                catch (InvalidOperationException invOp)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, invOp.Message));
                }
                catch (ArgumentException argEx)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, argEx.Message));
                }

                if (updated == null)
                    throw new RpcException(new Status(StatusCode.NotFound, "Producto no encontrado"));

                return ToProtoProductResponse(updated);
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateProduct gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }

        public override async Task<Empty> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Id))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id requerido"));

                if (!Guid.TryParse(request.Id, out var guid))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Id no es GUID válido"));

                var result = await _productService.SoftDeleteAsync(guid);
                if (!result)
                    throw new RpcException(new Status(StatusCode.NotFound, "Producto no encontrado"));

                return new Empty();
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteProduct gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Error interno del servidor"));
            }
        }
    }
}