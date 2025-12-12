using Microsoft.EntityFrameworkCore;
using ProdCats.Api.Models;

namespace ProdCats.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.Description)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_category_is_active");
            entity.HasIndex(e => e.Name).HasDatabaseName("idx_category_name");
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.Description)
                .HasColumnName("description");
            entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();
            entity.Property(e => e.StockQuantity)
                .HasColumnName("stock_quantity")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(e => e.CreatedDate)
                .HasColumnName("created_date")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            // Foreign key relationship
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.CategoryId).HasDatabaseName("idx_product_category_id");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_product_is_active");
            entity.HasIndex(e => e.CreatedDate)
                .HasDatabaseName("idx_product_created_date")
                .IsDescending();
            entity.HasIndex(e => e.Price).HasDatabaseName("idx_product_price");
            
            // Composite and partial indexes
            entity.HasIndex(e => new { e.CategoryId, e.IsActive })
                .HasDatabaseName("idx_product_category_active")
                .HasFilter("\"is_active\" = true");
            entity.HasIndex(e => e.StockQuantity)
                .HasDatabaseName("idx_product_stock_quantity")
                .HasFilter("\"stock_quantity\" > 0");
            entity.HasIndex(e => new { e.IsActive, e.Price })
                .HasDatabaseName("idx_product_active_price")
                .HasFilter("\"is_active\" = true");

            // Check constraints
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price_Positive", "\"price\" > 0");
                t.HasCheckConstraint("CK_Product_StockQuantity_NonNegative", "\"stock_quantity\" >= 0");
            });
        });
    }
}



