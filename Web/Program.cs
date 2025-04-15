using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Service.OrderServices;
using StackExchange.Redis;

namespace Web;

public class Program
{
    private static string GetSecret()
    {
        var userSecretId = typeof(Web.Program).Assembly
                        .GetCustomAttributes(typeof(UserSecretsIdAttribute), false)
                        .OfType<UserSecretsIdAttribute>()
                        .FirstOrDefault()?.UserSecretsId;
        if (userSecretId == null)
            throw new Exception("Init Application Configuration File Has Not Set Yet!");
        return userSecretId!;
    }

    private static void AddConnectionStrings(WebApplicationBuilder builder)
    {
        var userSecretId = GetSecret();
        builder.Configuration.AddUserSecrets(userSecretId);

        var masterConnectionString = builder.Configuration.GetConnectionString("MasterConnection");
        var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

        builder.Services.AddDbContext<MasterDbContext>(options =>
                                                         options.UseSqlServer(masterConnectionString)
                                                         .EnableSensitiveDataLogging());

        builder.Services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection!));
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOrderServices, OrderServices>();
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        AddConnectionStrings(builder);

        RegisterServices(builder);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
