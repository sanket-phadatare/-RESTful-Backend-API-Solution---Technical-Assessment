using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using Application.Interfaces;
using Application.Services;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<Mapping.MappingProfile>();
            });

            services.AddValidatorsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IItemService, ItemService>();

            return services;
        }
    }
}
