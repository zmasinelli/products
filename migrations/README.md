# Database Migrations

## Schema Overview

### Category Table
- `id` (SERIAL PRIMARY KEY): Unique identifier
- `name` (VARCHAR(255)): Category name
- `description` (TEXT): Category description
- `is_active` (BOOLEAN): Active status flag

### Product Table
- `id` (SERIAL PRIMARY KEY): Unique identifier
- `name` (VARCHAR(255)): Product name
- `description` (TEXT): Product description
- `price` (DECIMAL(10, 2)): Product price (non-negative)
- `category_id` (INTEGER, FK): Reference to category
- `stock_quantity` (INTEGER): Available stock (non-negative)
- `created_date` (TIMESTAMP WITH TIME ZONE): Creation timestamp
- `is_active` (BOOLEAN): Active status flag

## Foreign Key Relationship

- `product.category_id` → `category.id`
  - `ON DELETE RESTRICT`: Prevents deletion of categories with associated products
  - `ON UPDATE CASCADE`: Updates category ID if category ID changes

## Indexing Strategy

This section documents all indexing decisions, their rationale, and performance implications.

### Indexing Principles

1. **Foreign Key Indexes**: Always indexed for join performance
2. **Filter Columns**: Indexed when frequently used in WHERE clauses
3. **Sort Columns**: Indexed when frequently used in ORDER BY
4. **Partial Indexes**: Used for filtered subsets to reduce size and improve selectivity
5. **Composite Indexes**: Created for multi-column query patterns with proper column order

### Category Indexes

#### 1. `idx_category_is_active` on `is_active`
- **Type**: B-tree index
- **Purpose**: Fast filtering of active/inactive categories
- **Query Patterns**:
  - `WHERE is_active = true`
  - `WHERE is_active = false`
- **Rationale**: 
  - Most queries filter by active status
  - Low cardinality (boolean) but high selectivity when filtering
  - Enables fast category listing for UI dropdowns
- **Trade-off**: Minimal overhead, boolean indexes are small and efficient

#### 2. `idx_category_name` on `name`
- **Type**: B-tree index
- **Purpose**: Efficient category lookups by name and alphabetical sorting
- **Query Patterns**:
  - `WHERE name = 'Electronics'`
  - `ORDER BY name`
  - `WHERE name LIKE 'Ele%'` (prefix searches)
- **Rationale**:
  - Categories are often looked up by name
  - UI typically displays categories alphabetically
  - Supports case-sensitive exact matches and prefix searches
- **Trade-off**: Small overhead for write operations, but essential for read performance

### Product Indexes

#### 1. `idx_product_category_id` on `category_id`
- **Type**: B-tree index
- **Purpose**: Optimize foreign key joins and category-based filtering
- **Query Patterns**:
  - `WHERE category_id = ?`
  - `JOIN category ON product.category_id = category.id`
  - `WHERE category_id IN (1, 2, 3)`
- **Rationale**:
  - **Critical**: Foreign keys should always be indexed for join performance
  - Most common filter pattern: "show products in category X"
  - Without this index, joins would require full table scans
- **Performance Impact**: 
  - Join queries: O(n) → O(log n) with index
  - Essential for referential integrity checks
- **Trade-off**: Required index, no downside

#### 2. `idx_product_is_active` on `is_active`
- **Type**: B-tree index
- **Purpose**: Fast filtering of active/inactive products
- **Query Patterns**:
  - `WHERE is_active = true`
  - `WHERE is_active = false`
- **Rationale**:
  - Most product listings show only active products
  - Low cardinality but high query frequency
  - Works in combination with other filters
- **Trade-off**: 
  - Small index size (boolean)
  - Used in conjunction with partial indexes for better selectivity

#### 3. `idx_product_created_date` on `created_date DESC`
- **Type**: B-tree index (descending)
- **Purpose**: Efficient sorting by creation date (newest first)
- **Query Patterns**:
  - `ORDER BY created_date DESC`
  - `WHERE created_date >= ? ORDER BY created_date DESC`
  - `WHERE created_date BETWEEN ? AND ?`
- **Rationale**:
  - "New arrivals" is a common product listing pattern
  - Descending order matches typical display (newest first)
  - Supports date range queries for reporting
- **Performance Impact**: 
  - Without index: O(n log n) sort
  - With index: O(log n) lookup + sequential scan
- **Trade-off**: 
  - Slightly larger index (timestamp), but essential for date-based queries
  - Descending order optimized for "newest first" pattern

#### 4. `idx_product_category_active` on `(category_id, is_active)` WHERE `is_active = true`
- **Type**: Partial composite index
- **Purpose**: Optimize the most common query pattern: active products in a category
- **Query Patterns**:
  - `WHERE category_id = ? AND is_active = true`
  - `WHERE category_id IN (?) AND is_active = true`
- **Rationale**:
  - **Most frequent query**: "Show active products in category X"
  - Partial index (WHERE clause) reduces index size by ~50% (assuming ~50% active)
  - Composite index covers both filter conditions
  - Column order: `category_id` first (higher selectivity), then `is_active`
- **Performance Impact**:
  - Single index scan instead of two separate index scans
  - Smaller index = faster scans, less memory usage
- **Trade-off**:
  - Only useful for `is_active = true` queries
  - Requires maintenance, but worth it for primary use case

#### 5. `idx_product_price` on `price`
- **Type**: B-tree index
- **Purpose**: Sorting and range queries by price
- **Query Patterns**:
  - `ORDER BY price`
  - `ORDER BY price DESC`
  - `WHERE price BETWEEN 10 AND 100`
  - `WHERE price <= 50`
- **Rationale**:
  - Price sorting is common (low to high, high to low)
  - Price filtering for budget constraints
  - Supports e-commerce "sort by price" functionality
- **Performance Impact**:
  - Enables index-only scans for price-sorted queries
  - Range queries use index efficiently
- **Trade-off**:
  - Numeric index is compact and efficient
  - Essential for price-based queries

#### 6. `idx_product_stock_quantity` on `stock_quantity` WHERE `stock_quantity > 0`
- **Type**: Partial index
- **Purpose**: Fast filtering of in-stock items
- **Query Patterns**:
  - `WHERE stock_quantity > 0`
  - `WHERE stock_quantity BETWEEN 1 AND 10` (low stock alerts)
- **Rationale**:
  - Partial index excludes zero-stock items (most common case)
  - Reduces index size significantly if many products are out of stock
  - Optimizes "show only available products" queries
- **Performance Impact**:
  - Smaller index = faster scans
  - Better selectivity (only non-zero values indexed)
- **Trade-off**:
  - Only useful for `stock_quantity > 0` queries
  - Zero-stock queries won't use this index (but are less common)

#### 7. `idx_product_active_price` on `(is_active, price)` WHERE `is_active = true`
- **Type**: Partial composite index
- **Purpose**: Single index for filtering active products and sorting by price
- **Query Patterns**:
  - `WHERE is_active = true ORDER BY price`
  - `WHERE is_active = true ORDER BY price DESC`
- **Rationale**:
  - Common e-commerce pattern: "Show active products sorted by price"
  - Partial index reduces size (only active products)
  - Composite index covers both filter and sort in one scan
  - Column order: `is_active` first (filter), then `price` (sort)
- **Performance Impact**:
  - Single index scan instead of filter + sort
  - Index-only scan possible for price-sorted listings
- **Trade-off**:
  - Only for active products
  - Redundant with `idx_product_is_active` and `idx_product_price`, but optimized for this specific pattern

### Index Maintenance Considerations

- **Write Performance**: Each index adds overhead to INSERT/UPDATE/DELETE operations
- **Storage**: Indexes consume disk space (~20-30% of table size estimated)
- **Query Planner**: PostgreSQL automatically chooses optimal indexes
- **Monitoring**: Use `EXPLAIN ANALYZE` to verify index usage
- **Vacuum**: Regular VACUUM recommended to maintain index efficiency

### Indexes NOT Created (and why)

- **Product name index**: Not indexed - full-text search would require different approach (GIN/GiST)
- **Description indexes**: TEXT columns not indexed - would require full-text search indexes
- **Composite (category_id, price)**: Covered by `idx_product_category_active` and separate price index
- **Composite (is_active, created_date)**: Less common pattern, separate indexes sufficient

## Usage

### Apply Migration

```bash
psql -U your_user -d your_database -f migrations/001_create_schema.sql
```

## Design Decisions

1. **Partial Indexes**: Used for `is_active = true` patterns to reduce index size and improve performance
2. **Composite Indexes**: Created for common multi-column query patterns
3. **Foreign Key Constraint**: `ON DELETE RESTRICT` prevents orphaned products; `ON UPDATE CASCADE` maintains referential integrity
4. **Check Constraints**: Ensure data integrity (non-negative price and stock)
5. **Default Values**: `is_active` defaults to `true`, timestamps default to current time




