using System;
using System.Collections.Generic;
using System.Text;
using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain.Tests.Entities;

public sealed class DomainArchitectureTests
{
    [Fact]
    public void DomainEntities_ShouldInheritFromEntityBase()
    {
        var domainAssembly = typeof(Product).Assembly;

        var entityTypes = domainAssembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(EntityBase).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(entityTypes);

        foreach (var type in entityTypes)
        {
            Assert.True(
                typeof(EntityBase).IsAssignableFrom(type),
                $"{type.Name} must inherit from {nameof(EntityBase)}.");
        }
    }
}
