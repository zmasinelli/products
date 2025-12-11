# Products Project

A product management system with PostgreSQL database and .NET Web API backend.

## Project Structure

- `migrations/` - Database schema migrations
- `Products.Api/` - .NET 9.0 Web API backend

## Getting Started

### Database Setup

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

#### Connecting PGAdmin to the Database

1. Open http://localhost:5050 in your browser
2. Login with `admin@products.local` / `admin`
3. Right-click "Servers" → "Register" → "Server"
4. General tab:
   - Name: `Products Local`
5. Connection tab:
   - Host name/address: `postgres` (use the service name from docker-compose)
   - Port: `5432` (internal container port)
   - Maintenance database: `products`
   - Username: `admin`
   - Password: `password123`
6. Click "Save"

See `migrations/README.md` for database schema documentation and migration instructions.

### API Setup

See `Products.Api/README.md` for API setup, configuration, and endpoint documentation.

## Overview

This project provides a complete product management system with:
- PostgreSQL database with optimized schema and indexes
- RESTful Web API for products and categories
- Soft delete functionality
- Active/inactive filtering
