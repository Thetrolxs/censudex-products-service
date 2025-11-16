# 📦 censudex-products-service

**Microservicio desarrollada como parte del Sistema de Censudex**  
Este microservicio representa el backend de una plataforma de creación, visualización, edición y eliminación de tickets. 

---

## 📚 Descripción del proyecto

Este proyecto consiste en el desarrollo de un microservicio utilizando **.NET 9** y **Mongo DB**, orientada a la gestión de productos del sistema Censudex 
La arquitectura sigue buenas prácticas de diseño, incluyendo los patrones **Repository**, lo que permite una separación clara de responsabilidades y facilita el mantenimiento y escalabilidad del sistema.

El foco del desarrollo actual está en la estructuración del backend para futuras integraciones frontend.

---

## 🧑‍💻 Autor

- **Ignacio Alfonso Morales Harnisch**
---

## 🧱 Tecnologías utilizadas

- [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [Git](https://git-scm.com/)
- [Docker or Docker Desktop](https://docs.docker.com/)
- [Mongo DB](https://www.mongodb.com/)
- UUID V4
- C#
- Patrones: Repository

---

## 🗂️ Estructura del proyecto

```
Src/
│
├── Controllers/        → Controladores donde se encuentran los endpoints
├── Data/               → MongoContext
├── DTOs/               → Clases para transferencia de datos (CreateProductDto, ProductResponseDto, UpdateProductDto)
├── Helpers/            → Archivo con ayudas de mappingprofile
├── Interfaces/         → Interfaces de los servicios y repositorios
├── Models/             → Entidades del dominio: Product.
├── Repositories/       → Implementaciones de lógica de acceso a datos
├── Services/           → Servicios que interactuan con los controladores
├── Settings/           → Modelo para la conexión con la base de datos
├── Program.cs          → Configuración general del servidor y servicios
```

---

## ⚙️ Cómo ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/Thetrolxs/censudex-products-service.git
cd censudex-products-service
```

### 2. Agregar el appsettings.json

agregar el siguiente codigo en la carpeta principal del proyecto con nombre appsettings.json
```bash
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "<conexión de la base de datos (puede ser en un cluster o de forma local>",
    "DatabaseName": "<nombre de la base de datos>"
  },
  "CloudinarySettings": {
    "CloudName": "YOUR_CLOUDNAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}

```
---
## Opciones para construir el proyecto

### A. Construir el proyecto usando docker compose

```bash
docker compose up --build
```

El microservicio se iniciará en docker con en `http://localhost:5000`.

---

### B.1 Construir el proyecto de forma local
```bash
dotnet restore
```
### B.2 Construir el proyecto de forma local
```bash
dotnet build
```

### B.3 Construir el proyecto de forma local
```bash
dotnet run
```

El microservicio se iniciará en docker con en `http://localhost:5211`.

---
## 📖 Endpoints
| Metodo  | Endpoint | Descripción  |
| ------------- | ------------- | ------------- |
| POST | `/products/` | Se crea un nuevo producto |
| GET | `/products/` | Se obtiene una lista de productos |
| GET | `/products/{id}` | Se obtiene un producto en base a su ID |
| PATCH | `/products/{id}` | Se actualiza un producto en base a su ID |
| DELETE | `/products/{id}` | Se borra (softDelete) un producto en base a su ID |
