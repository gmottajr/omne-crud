using System;
using System.Collections.Generic;
using System.Text;

namespace Omne_Crud_Demo.Core.Models.Requests;

public sealed class CreateProductRequest
{
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
}
