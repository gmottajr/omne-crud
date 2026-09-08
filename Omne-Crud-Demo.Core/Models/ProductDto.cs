using System;
using System.Collections.Generic;
using System.Text;

namespace Omne_Crud_Demo.Core.Models;

public sealed record ProductDto(
    int Id,
    string Sku,
    string Name,
    decimal Price,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);