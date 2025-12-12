-- Create Category table
CREATE TABLE category (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true
);

-- Create Product table
CREATE TABLE product (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL CHECK (price > 0),
    category_id INTEGER NOT NULL,
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT fk_product_category 
        FOREIGN KEY (category_id) 
        REFERENCES category(id) 
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

-- Indexes for Category table
-- Index on is_active for filtering active categories
CREATE INDEX idx_category_is_active ON category(is_active);

-- Index on name for lookups and sorting
CREATE INDEX idx_category_name ON category(name);

-- Indexes for Product table
-- Index on category_id for efficient joins and filtering by category
CREATE INDEX idx_product_category_id ON product(category_id);

-- Index on is_active for filtering active products
CREATE INDEX idx_product_is_active ON product(is_active);

-- Index on created_date for sorting and date range queries
CREATE INDEX idx_product_created_date ON product(created_date DESC);

-- Composite index for common query pattern: active products in a category
CREATE INDEX idx_product_category_active ON product(category_id, is_active) WHERE is_active = true;

-- Index on price for sorting and range queries
CREATE INDEX idx_product_price ON product(price);

-- Index on stock_quantity for filtering low stock items
CREATE INDEX idx_product_stock_quantity ON product(stock_quantity) WHERE stock_quantity > 0;

-- Composite index for active products sorted by price
CREATE INDEX idx_product_active_price ON product(is_active, price) WHERE is_active = true;




