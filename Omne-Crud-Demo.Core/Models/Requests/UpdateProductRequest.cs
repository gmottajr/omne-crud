using System;
using System.Collections.Generic;
using System.Text;

namespace Omne_Crud_Demo.Core.Models.Requests;

public sealed class UpdateProductRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
}