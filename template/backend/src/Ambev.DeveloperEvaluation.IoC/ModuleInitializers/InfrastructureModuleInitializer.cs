using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Caching;
using Ambev.DeveloperEvaluation.ORM.MongoDB;
using Ambev.DeveloperEvaluation.ORM.MongoDB.Repositories;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using StackExchange.Redis;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();

        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddScoped<ISaleEventStore, SaleEventStoreRepository>();
        builder.Services.AddScoped<ISaleReadRepository, SaleReadRepository>();

        var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";

        builder.Services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString));

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "DeveloperEvaluation:";
        });

        builder.Services.AddSingleton<ICacheService, RedisCacheService>();

        var rebusNetwork = new InMemNetwork();
        builder.Services.AddRebus(configure => configure
            .Transport(t => t.UseInMemoryTransport(rebusNetwork, "sales-queue"))
            .Routing(r => r.TypeBased()
                .Map<SaleCreatedEvent>("sales-queue")
                .Map<SaleModifiedEvent>("sales-queue")
                .Map<SaleCancelledEvent>("sales-queue")
                .Map<ItemCancelledEvent>("sales-queue")),
            onCreated: async bus => { await Task.CompletedTask; });

        builder.Services.AutoRegisterHandlersFromAssemblyOf<Application.Sales.Events.SaleCreatedEventHandler>();
    }
}
