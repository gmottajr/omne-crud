using Omne_Crud_Demo.Application;
using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Domain;
using Riok.Mapperly.Abstractions;

namespace Omne_Crud_Demo.Application.Mappings;

[Mapper]
public static partial class ProductMapper
{
    [MapProperty(nameof(CreateProductCommand.Sku), nameof(Product.Sku))]
    public static partial Product ToEntity(CreateProductCommand command);

    public static Sku ToSku(string value) => Sku.Create(value);

    public static Price ToPrice(decimal value) => Price.Create(value);

    [MapProperty([nameof(Product.Sku), nameof(Sku.Value)], nameof(ProductDto.Sku))]
    [MapProperty([nameof(Product.Price), nameof(Price.Value)], nameof(ProductDto.Price))]
    public static partial ProductDto ToDto(Product product);

    public static partial IReadOnlyList<ProductDto> ToDto(IReadOnlyList<Product> products);
}
