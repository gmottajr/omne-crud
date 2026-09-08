using Microsoft.Extensions.DependencyInjection;
using Omne_Crud_Demo.Application.Abstractions.Persistence;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;

namespace Omne_Crud_Demo.IoC;

public static class DependencyInjection
{
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
