using censudex_products_service.src.Data;
using censudex_products_service.src.Helpers;
using censudex_products_service.src.Interfaces;
using censudex_products_service.src.Repositories;
using censudex_products_service.src.Services;
using censudex_products_service.src.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(50051, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // opcional: puerto REST
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 10 * 1024 * 1024; // 20 MB, por ejemplo
});


builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

var cfg = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var env = Environment.GetEnvironmentVariable("MONGODB_URI");
    var options = sp.GetService<IOptions<MongoDbSettings>>()?.Value;
    var conn = env ?? options?.ConnectionString;

    if (string.IsNullOrWhiteSpace(conn))
    {
        throw new InvalidOperationException("La ruta de conexión con MongoDB no ha sido encontrada. Ingresar MONGODB_URI o configurar el MongoDbSettings en el appsettings.json.");
    }

    var settings = MongoClientSettings.FromConnectionString(conn);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    return new MongoClient(settings);
});

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    if (options == null || string.IsNullOrWhiteSpace(options.DatabaseName))
        throw new InvalidOperationException("MongoDbSettings.DatabaseName no ha sido configurado.");
    return client.GetDatabase(options.DatabaseName);
});

builder.Services.AddSingleton<MongoContext>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMapperService, MapperService>();

var app = builder.Build();

app.MapGrpcService<ProductsGrpcService>();
app.MapControllers();

app.Run();
