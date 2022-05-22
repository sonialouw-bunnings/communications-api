using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Communications.Domain.Entities;

namespace Communications.Persistence.Configurations
{
    public class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(e => e.Id)
                .HasColumnName("ProductId")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.UnitPrice)
                .HasColumnType("decimal")
                .HasPrecision(19, 2)
                .IsUnicode(false);

            builder.Property(e => e.CategoryId)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder
            .HasOne<Category>(category => category.Category)
            .WithMany(products => products.Products)
            .HasForeignKey(category => category.CategoryId);


        }
    }
}