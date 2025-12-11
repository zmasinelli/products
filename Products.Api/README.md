# Products API

A .NET 9.0 Web API for managing products and categories with PostgreSQL backend.

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database
- Database schema applied (see `../migrations/README.md`)

## Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=products;Username=your_user;Password=your_password"
  }
}
```

For development, you can override this in `appsettings.Development.json` (not tracked in git).

## Running the API

```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Products

#### GET /api/products
Returns all active products with category information.

**Response:** Array of `ProductDto`
```json
[
  {
    "id": 1,
    "name": "Product Name",
    "description": "Product description",
    "price": 29.99,
    "categoryId": 1,
    "categoryName": "Category Name",
    "stockQuantity": 100,
    "createdDate": "2024-01-01T00:00:00Z",
    "isActive": true
  }
]
```

#### GET /api/products/{id}
Returns a specific product by ID. Returns 404 if not found or inactive.

**Response:** `ProductDto`

#### POST /api/products
Creates a new product.

**Request Body:** `CreateProductDto`
```json
{
  "name": "Product Name",
  "description": "Product description",
  "price": 29.99,
  "categoryId": 1,
  "stockQuantity": 100
}
```

**Validation:**
- `name`: Required, max 255 characters
- `price`: Required, must be non-negative
- `categoryId`: Required, must reference an active category
- `stockQuantity`: Optional, defaults to 0, must be non-negative

**Response:** `ProductDto` (201 Created)

#### PUT /api/products/{id}
Updates an existing product. Returns 404 if not found or inactive.

**Request Body:** `UpdateProductDto` (all fields optional)
```json
{
  "name": "Updated Name",
  "price": 39.99,
  "stockQuantity": 50
}
```

**Response:** 204 No Content

#### DELETE /api/products/{id}
Soft deletes a product (sets `IsActive = false`). Returns 404 if not found.

**Response:** 204 No Content

### Categories

#### GET /api/categories
Returns all active categories.

**Response:** Array of `CategoryDto`
```json
[
  {
    "id": 1,
    "name": "Category Name",
    "description": "Category description",
    "isActive": true
  }
]
```

#### POST /api/categories
Creates a new category.

**Request Body:** `CreateCategoryDto`
```json
{
  "name": "Category Name",
  "description": "Category description"
}
```

**Validation:**
- `name`: Required, max 255 characters
- `description`: Optional

**Response:** `CategoryDto` (201 Created)

## Error Responses

- **400 Bad Request**: Validation errors or invalid data
- **404 Not Found**: Resource not found or inactive
- **500 Internal Server Error**: Server error

## Technology Stack

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- Npgsql (PostgreSQL provider)
- Swagger/OpenAPI

## Project Structure

```
Products.Api/
├── Controllers/          # API controllers
├── Data/                # DbContext and data access
├── Models/              # Entity models and DTOs
│   └── DTOs/           # Data transfer objects
├── Program.cs          # Application entry point
└── appsettings.json    # Configuration
```



