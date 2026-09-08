using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Domain;
using Riok.Mapperly.Abstractions;

namespace Omne_Crud_Demo.Application.Mappings;

[Mapper]
public static partial class ProductMapper
{
    [MapProperty([nameof(Product.Sku), nameof(Sku.Value)], nameof(ProductDto.Sku))]
    public static partial ProductDto ToDto(Product product);

    public static partial IReadOnlyList<ProductDto> ToDto(IReadOnlyList<Product> products);
}
