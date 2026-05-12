namespace Infrastructure;

using Application.Interfaces;
using Application.Interfaces.Messaging;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres")));

        services.AddSingleton<IRabbitMqConnectionProvider,RabbitMqConnectionProvider>();

        services.AddScoped<ITextExtractor, PdfTextExtractor>();
        services.AddScoped<IPdfRepository, PdfRepository>();
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}