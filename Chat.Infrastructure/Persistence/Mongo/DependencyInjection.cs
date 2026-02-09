using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Chat.Infrastructure.Persistence.Mongo;

public static class DependencyInjection
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MongoOptions>(config.GetSection(MongoOptions.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            return new MongoClient(opt.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(opt.Database);
        });

        return services;
    }
}