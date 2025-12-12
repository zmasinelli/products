# Products Project

A product management system with PostgreSQL database, .NET Web API backend, and Angular frontend.

## 1. Quick Start

### Prerequisites

- **.NET 8.0 SDK** or later
- **Node.js 18+** and **npm** (for Angular frontend)
- **Docker** and **Docker Compose** (for PostgreSQL database)
- **Angular CLI** (install globally: `npm install -g @angular/cli`)

### Setup and Run Instructions

#### Database Setup

Start the local PostgreSQL database and PGAdmin using Docker Compose:

```bash
docker-compose up -d
```

This will start:
- **PostgreSQL** on `localhost:5432`
  - Database: `products`
  - Username: `admin`
  - Password: `password123`
- **PGAdmin** on `http://localhost:5050`
  - Email: `admin@products.local`
  - Password: `admin`

To stop the services:
```bash
docker-compose down
```

To stop and remove volumes (deletes all data):
```bash
docker-compose down -v
```

**Connecting PGAdmin to the Database:**
1. Open http://localhost:5050 in your browser
2. Login with `admin@products.local` / `admin`
3. Right-click "Servers" → "Register" → "Server"
4. General tab: Name: `Products Local`
5. Connection tab:
   - Host name/address: `postgres` (use the service name from docker-compose)
   - Port: `5432`
   - Maintenance database: `products`
   - Username: `admin`
   - Password: `password123`
6. Click "Save"

#### API Setup

1. Navigate to the API directory:
   ```bash
   cd Products.Api
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5000` (or the port shown in the console). Swagger UI is available at `http://localhost:5000/swagger` in development mode.

The database will be automatically seeded with sample data on first run.

#### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

The Angular application will be available at `http://localhost:4200`.

## 2. Architecture

### Overall Architecture Approach

The project follows a **layered architecture** with clear separation of concerns:

- **Controllers Layer**: Thin controllers that handle HTTP requests/responses and delegate to services
- **Services Layer**: Business logic and orchestration, implementing interfaces for dependency inversion
- **Data Layer**: Entity Framework Core DbContext for data access
- **Models Layer**: Domain entities and DTOs separated for data transfer

**Key Architectural Principles:**
- **Dependency Injection**: All services registered in `Program.cs` with appropriate lifetimes
- **Interface-based Design**: Services implement interfaces (`IProductService`, `ICategoryService`) enabling testability and flexibility
- **DTO Pattern**: Separate DTOs from domain entities to control API contracts and prevent over-posting
- **Global Exception Handling**: Centralized error handling via middleware

### Database Schema

**Category Table:**
- `id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(255), NOT NULL)
- `description` (TEXT, nullable)
- `is_active` (BOOLEAN, DEFAULT true)

**Product Table:**
- `id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(255), NOT NULL)
- `description` (TEXT, nullable)
- `price` (DECIMAL(10,2), NOT NULL, CHECK > 0)
- `category_id` (INTEGER, NOT NULL, FK to category.id)
- `stock_quantity` (INTEGER, DEFAULT 0, CHECK >= 0)
- `created_date` (TIMESTAMP WITH TIME ZONE, DEFAULT CURRENT_TIMESTAMP)
- `is_active` (BOOLEAN, DEFAULT true)

**Relationships:**
- Product → Category: Many-to-One (Foreign Key with RESTRICT on delete)

**Constraints:**
- Price must be positive
- Stock quantity must be non-negative
- Foreign key prevents deletion of categories with products

### Technology Choices

**Backend:**
- **.NET 8.0**: Modern, performant framework with excellent async/await support
- **Entity Framework Core 9.0**: ORM for type-safe database access and migrations
- **PostgreSQL**: Robust relational database with excellent JSON support and performance
- **Npgsql**: PostgreSQL provider for EF Core
- **Swagger/OpenAPI**: API documentation and testing

**Frontend:**
- **Angular 21**: Component-based framework with strong typing and dependency injection
- **RxJS**: Reactive programming for HTTP calls and state management
- **TypeScript**: Type safety and modern JavaScript features

**Infrastructure:**
- **Docker Compose**: Containerized database for consistent development environment
- **PGAdmin**: Database administration tool

## 3. Design Decisions

### Single Responsibility and Dependency Inversion

**Single Responsibility Principle (SRP):**
- **Controllers**: Only handle HTTP concerns (routing, status codes, model validation)
- **Services**: Contain business logic and orchestrate data access
- **DbContext**: Manages database connections and entity configuration
- **DTOs**: Represent data contracts for API boundaries
- **Middleware**: Handles cross-cutting concerns (exception handling)

**Dependency Inversion Principle (DIP):**
- Services depend on abstractions (`IProductService`, `ICategoryService`) rather than concrete implementations
- Controllers depend on service interfaces, not concrete services
- Services depend on `ApplicationDbContext` abstraction via constructor injection
- All dependencies registered in `Program.cs` using dependency injection container

**Example:**
```csharp
// Controller depends on interface
public ProductsController(IProductService productService) { ... }

// Service implements interface
public class ProductService : IProductService { ... }

// Registered in Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
```

### EF Core Approach and Query Optimization

**Query Optimization Strategies:**

1. **AsNoTracking() for Read Operations**: All read queries use `.AsNoTracking()` to prevent EF Core from tracking entities, reducing memory usage and improving performance:
   ```csharp
   return await _context.Products
       .Where(p => p.IsActive)
       .Include(p => p.Category)
       .AsNoTracking()
       .Select(p => new ProductDto { ... })
       .ToListAsync();
   ```

2. **Projection to DTOs**: Queries project directly to DTOs using `.Select()`, avoiding loading full entities and reducing data transfer:
   ```csharp
   .Select(p => new ProductDto
   {
       Id = p.Id,
       Name = p.Name,
       // ... only needed fields
   })
   ```

3. **Eager Loading with Include()**: Used strategically to avoid N+1 queries when related data is needed:
   ```csharp
   .Include(p => p.Category)
   ```

4. **Single Query for Complex Operations**: The search endpoint builds a single queryable and executes once, avoiding multiple round trips:
   ```csharp
   var query = _context.Products.Where(p => p.IsActive).AsQueryable();
   // Apply filters conditionally
   if (categoryId.HasValue) query = query.Where(...);
   // Execute once
   var totalCount = await query.CountAsync();
   var items = await query.Skip(...).Take(...).ToListAsync();
   ```

5. **Conditional Filtering**: Filters are applied conditionally to the queryable before execution, allowing EF Core to optimize the SQL:
   ```csharp
   if (minPrice.HasValue)
       query = query.Where(p => p.Price >= minPrice.Value);
   ```

**No Raw SQL**: All queries use EF Core LINQ, which provides type safety, maintainability, and allows EF Core to optimize SQL generation.

### Complex Endpoint Choice and Rationale

**Chosen: Product Search Endpoint** (`GET /api/products/search`)

**Rationale:**
1. **Real-world Utility**: Product search is a common e-commerce requirement with multiple filter combinations
2. **Query Complexity**: Demonstrates advanced EF Core query building with conditional filters, sorting, and pagination
3. **Performance Challenge**: Requires efficient single-query implementation to avoid N+1 problems
4. **Scalability**: Pagination is essential for large datasets

**Implementation Highlights:**
- **AND Logic for Search Terms**: Multiple words in `searchTerm` are split and each word must match (AND logic) across name or description
- **Case-Insensitive Search**: Uses PostgreSQL's `ILike` function via `EF.Functions.ILike()`
- **Composable Filters**: All filters are optional and can be combined
- **Efficient Pagination**: Total count calculated before pagination, single query execution
- **Flexible Sorting**: Supports sorting by name, price, or created date in ascending/descending order

**Alternative Considered (Category Analytics):**
- Rejected because aggregation queries are more straightforward and less demonstrative of complex query building
- Search endpoint better showcases conditional query composition and real-world patterns

### Repository Pattern Decision and Trade-offs

**Decision: Direct DbContext Access (No Repository Pattern)**

**Rationale:**
1. **EF Core is Already an Abstraction**: EF Core's `DbContext` and `DbSet<T>` already provide a unit of work and repository-like interface
2. **Reduced Abstraction Overhead**: Adding another layer would introduce complexity without significant benefit for this project size
3. **EF Core Query Flexibility**: Direct access allows leveraging EF Core's powerful LINQ capabilities and query composition
4. **Simpler Codebase**: Fewer classes to maintain, clearer data flow

**Trade-offs:**

**Advantages:**
- Less boilerplate code
- Direct access to EF Core features (e.g., `EF.Functions.ILike()`)
- Easier to optimize queries with projections and `AsNoTracking()`
- Simpler dependency graph

**Disadvantages:**
- **Tight Coupling to EF Core**: Services are coupled to EF Core, making it harder to switch ORMs (rarely needed in practice)
- **Testing Complexity**: Requires mocking `DbContext` or using in-memory database for unit tests
- **No Centralized Data Access Logic**: Business logic and data access are more intertwined

**When Repository Pattern Would Be Beneficial:**
- Multiple data sources (database + cache + external APIs)
- Need to abstract away ORM for testing without EF Core
- Complex data access patterns that benefit from centralized logic
- Team preference for strict layering

**For This Project:**
Given the single data source (PostgreSQL), straightforward CRUD operations, and emphasis on query optimization, direct `DbContext` access provides the best balance of simplicity and performance.

### Index Strategy

Indexes were designed based on query patterns identified in the API endpoints:

**Category Indexes:**
- `idx_category_is_active`: Supports filtering active categories (used in `GET /api/categories`)
- `idx_category_name`: Supports lookups and potential sorting by name

**Product Indexes:**

1. **Single Column Indexes:**
   - `idx_product_category_id`: Foreign key index for efficient joins and category filtering
   - `idx_product_is_active`: Supports filtering active products (used in all product endpoints)
   - `idx_product_created_date DESC`: Supports sorting by creation date (used in search endpoint)
   - `idx_product_price`: Supports price range filtering and sorting (used in search endpoint)

2. **Partial Indexes (Filtered):**
   - `idx_product_category_active`: Composite index on `(category_id, is_active)` filtered to `is_active = true`
     - Optimizes queries filtering by category AND active status
     - Smaller index size (only active products)
   - `idx_product_stock_quantity`: Filtered to `stock_quantity > 0`
     - Optimizes "in stock" filtering in search endpoint
   - `idx_product_active_price`: Composite on `(is_active, price)` filtered to `is_active = true`
     - Optimizes active product queries sorted by price

**Index Design Principles:**
- **Query-Driven**: Each index supports a specific query pattern from the API
- **Partial Indexes**: Used where filtering by `is_active = true` is common, reducing index size
- **Composite Indexes**: Created for common filter combinations (category + active, active + price)
- **Covering Considerations**: Indexes chosen to support both filtering and sorting operations

**Trade-offs:**
- **Write Performance**: More indexes slow down INSERT/UPDATE operations (acceptable trade-off for read-heavy API)
- **Storage**: Partial indexes reduce storage overhead while maintaining query performance
- **Maintenance**: PostgreSQL automatically maintains indexes, but more indexes increase maintenance overhead

## 4. What I Would Do With More Time

### Unimplemented Features and Approach

1. **Category Analytics Endpoint** (Option B from requirements)
   - **Approach**: Implement `GET /api/categories/{id}/summary` with aggregation query
   - **Optimization**: Use EF Core's `GroupBy()` or raw SQL for efficient aggregations
   - **Caching**: Consider caching category summaries since they change infrequently

2. **Full-Text Search**
   - **Approach**: Implement PostgreSQL full-text search with `tsvector` and `tsquery`
   - **Benefits**: Better relevance ranking and performance for large text searches
   - **Migration**: Add full-text search columns and indexes to Product table

3. **Product Image Support**
   - **Approach**: Add `ImageUrl` field to Product entity, store images in cloud storage (S3/Azure Blob)
   - **API**: Add image upload endpoint with validation and resizing

4. **Audit Logging**
   - **Approach**: Implement audit trail for product changes (who, when, what changed)
   - **Pattern**: Use EF Core interceptors or shadow properties to track changes

5. **Bulk Operations**
   - **Approach**: Add endpoints for bulk create/update/delete operations
   - **Optimization**: Use `AddRange()` and batch processing for performance

6. **Advanced Frontend Features**
   - Product creation/edit forms with validation
   - Real-time search with debouncing
   - Category filtering UI
   - Product details view with full information
   - Error handling and loading states

### Refactoring Priorities

1. **Validation Layer**
   - **Current**: Validation attributes on DTOs + manual checks in services
   - **Improvement**: Extract validation logic to FluentValidation or dedicated validators
   - **Benefit**: Centralized, testable validation rules

2. **Mapping Layer**
   - **Current**: Manual DTO mapping in services
   - **Improvement**: Use AutoMapper or Mapster for entity ↔ DTO mapping
   - **Benefit**: Reduces boilerplate, easier to maintain

3. **Result Pattern**
   - **Current**: Services return DTOs or throw exceptions
   - **Improvement**: Implement Result<T> pattern for explicit error handling
   - **Benefit**: Makes error cases explicit in method signatures

4. **Specification Pattern**
   - **Current**: Query building logic in service methods
   - **Improvement**: Extract query specifications to separate classes
   - **Benefit**: Reusable, testable query logic

5. **Unit Tests**
   - **Priority**: Add comprehensive unit tests for services using in-memory database
   - **Coverage**: Test all business logic, edge cases, and error scenarios

6. **Integration Tests**
   - **Priority**: Add API integration tests using TestServer
   - **Coverage**: Test all endpoints with various scenarios

### Production Considerations

1. **Security**
   - **Authentication & Authorization**: Implement JWT-based auth with role-based access control
   - **API Rate Limiting**: Prevent abuse with rate limiting middleware
   - **Input Sanitization**: Additional validation and sanitization for user inputs
   - **HTTPS**: Enforce HTTPS in production
   - **CORS**: Restrict CORS to specific origins in production

2. **Performance**
   - **Caching**: Implement Redis caching for frequently accessed data (product lists, category summaries)
   - **Response Compression**: Enable gzip/brotli compression
   - **Connection Pooling**: Configure EF Core connection pool size appropriately
   - **Query Performance Monitoring**: Add logging for slow queries

3. **Reliability**
   - **Health Checks**: Implement health check endpoints for database and dependencies
   - **Retry Policies**: Add retry logic for transient database failures
   - **Circuit Breaker**: Implement circuit breaker pattern for external dependencies
   - **Logging**: Structured logging (Serilog) with correlation IDs

4. **Database**
   - **Migrations Strategy**: Automated migration deployment pipeline
   - **Backup Strategy**: Automated backups with point-in-time recovery
   - **Read Replicas**: Consider read replicas for scaling read operations
   - **Connection String Security**: Use Azure Key Vault or similar for secrets

5. **Monitoring & Observability**
   - **Application Insights / OpenTelemetry**: Distributed tracing and metrics
   - **Error Tracking**: Sentry or similar for error aggregation
   - **Performance Monitoring**: APM tools for performance bottlenecks

6. **Deployment**
   - **CI/CD Pipeline**: Automated build, test, and deployment
   - **Containerization**: Dockerize API for consistent deployments
   - **Environment Configuration**: Separate configs for dev/staging/prod
   - **Blue-Green Deployment**: Zero-downtime deployment strategy

## 5. Assumptions & Trade-offs

### Key Assumptions Made

1. **Single Database**: Assumed single PostgreSQL database; no multi-tenant or distributed data requirements
2. **Read-Heavy Workload**: Assumed more reads than writes, optimizing indexes and queries accordingly
3. **Soft Delete Only**: Assumed soft deletes (IsActive = false) are sufficient; no hard delete requirement
4. **Active-Only Default**: Assumed API consumers primarily need active products/categories; inactive items filtered by default
5. **No Authentication Required**: Assumed public API for assessment purposes; production would require auth
6. **Small to Medium Dataset**: Index strategy assumes dataset fits in memory; may need adjustment for very large datasets
7. **Synchronous Operations**: All operations are synchronous/awaitable; no background job requirements assumed
8. **English Language Only**: Search implementation assumes English text; full-text search would need language-specific configuration

### Trade-offs in Design

1. **Direct DbContext vs Repository Pattern**
   - **Chosen**: Direct DbContext access
   - **Trade-off**: Simpler code vs. easier testing and ORM abstraction
   - **Rationale**: EF Core is already an abstraction; repository adds little value for this use case

2. **DTOs vs Direct Entity Exposure**
   - **Chosen**: DTOs for all API responses
   - **Trade-off**: More mapping code vs. controlled API contracts and security
   - **Rationale**: Prevents over-posting, allows API evolution independent of entities

3. **Soft Delete vs Hard Delete**
   - **Chosen**: Soft delete (IsActive = false)
   - **Trade-off**: Data retention vs. true deletion and storage overhead
   - **Rationale**: Preserves audit trail and allows recovery; acceptable storage cost

4. **EF Core LINQ vs Raw SQL**
   - **Chosen**: EF Core LINQ exclusively
   - **Trade-off**: Type safety and maintainability vs. potential performance in edge cases
   - **Rationale**: EF Core generates efficient SQL; raw SQL only if profiling shows bottlenecks

5. **Partial Indexes vs Full Indexes**
   - **Chosen**: Partial indexes where appropriate (filtered by is_active = true)
   - **Trade-off**: Smaller index size and faster writes vs. flexibility for inactive data queries
   - **Rationale**: Most queries filter by active status; partial indexes optimize common case

6. **Single Query vs Multiple Queries**
   - **Chosen**: Single query with conditional filters for search endpoint
   - **Trade-off**: Slightly more complex query building vs. multiple round trips
   - **Rationale**: Better performance and atomicity; EF Core optimizes the generated SQL

7. **AsNoTracking() for All Reads**
   - **Chosen**: Use AsNoTracking() for all read operations
   - **Trade-off**: Better performance vs. ability to update tracked entities
   - **Rationale**: Read operations don't need tracking; updates fetch entities separately

8. **Thin Controllers vs Fat Controllers**
   - **Chosen**: Thin controllers delegating to services
   - **Trade-off**: More classes vs. better separation of concerns and testability
   - **Rationale**: Aligns with Single Responsibility Principle; easier to test business logic

9. **Global Exception Handler vs Try-Catch Everywhere**
   - **Chosen**: Global exception handling middleware
   - **Trade-off**: Centralized handling vs. endpoint-specific error responses
   - **Rationale**: Consistent error format; reduces boilerplate; specific cases can still be handled in controllers

10. **PostgreSQL vs SQL Server**
    - **Chosen**: PostgreSQL
    - **Trade-off**: Open-source vs. Microsoft ecosystem integration
    - **Rationale**: Free, performant, excellent JSON support; requirement allowed choice

---

## Project Structure

- `migrations/` - Database schema migrations
- `Products.Api/` - .NET 8.0 Web API backend
  - `Controllers/` - API endpoints
  - `Services/` - Business logic layer
  - `Models/` - Domain entities and DTOs
  - `Data/` - EF Core DbContext and data seeding
  - `Middleware/` - Cross-cutting concerns
- `frontend/` - Angular 21 frontend application
- `docker-compose.yml` - PostgreSQL and PGAdmin containers

## Additional Documentation

- See `migrations/README.md` for database schema documentation and migration instructions.
- See `Products.Api/README.md` for detailed API endpoint documentation.
