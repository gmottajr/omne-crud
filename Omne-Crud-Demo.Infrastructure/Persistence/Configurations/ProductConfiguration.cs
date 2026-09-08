using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", table =>
        {
            table.HasCheckConstraint(
                "ck_products_sku_min_length",
                "char_length(btrim(sku)) >= 4");

            table.HasCheckConstraint(
                "ck_products_name_min_length",
                "char_length(btrim(name)) >= 3");

            table.HasCheckConstraint(
                "ck_products_description_min_length",
                "char_length(btrim(description)) >= 10");

            table.HasCheckConstraint(
                "ck_products_price_positive",
                "price >= 0");

            table.HasCheckConstraint(
                "ck_products_updated_at_after_created_at",
                "updated_at IS NULL OR updated_at >= created_at");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasConversion(
                sku => sku.Value,
                value => Sku.Create(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Sku)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
