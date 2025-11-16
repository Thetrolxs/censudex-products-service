using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace censudex_products_service.src.Services
{
    public class CloudinarySettings
    {
        public required string CloudName { get; set; }
        public required string ApiKey { get; set; }
        public required string ApiSecret { get; set; }
        public string UploadFolder { get; set; } = "censudex/products";
        public int MaxFileSizeMb { get; set; } = 10;

    }

    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            _settings = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length == 0) throw new ArgumentException("Empty file.", nameof(file));
            if (file.Length > _settings.MaxFileSizeMb * 1024 * 1024) 
                throw new ArgumentException($"El archivo es demasiado grande. Max {_settings.MaxFileSizeMb} MB.", nameof(file));
            
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Tipo de archivo invalido, solo las imagenes son permitidas.", nameof(file));

            try
            {
                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = _settings.UploadFolder,
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result == null) throw new Exception("Archivo nulo de Cloudinary.");
                if (result.Error != null)
                {
                    _logger.LogError("Error al cargar Cloudinary: {ErrorMessage}", result.Error.Message);
                    throw new Exception($"Error al cargar Cloudinary: {result.Error.Message}");
                }

                var url = result.SecureUrl?.AbsoluteUri ?? result.Url?.AbsoluteUri;
                if (string.IsNullOrEmpty(url))
                    throw new Exception("Se subió el archivo correctamente, pero no se retorno URL.");

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo a Cloudinary.");
                throw; 
            }
        }
    }
}