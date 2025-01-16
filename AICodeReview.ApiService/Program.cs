using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Scalar.AspNetCore;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add service defaults & Aspire client integrations.
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddProblemDetails();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "all",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddHttpClient();
        // 注册IMongoClient和IMongoDatabase作为单例服务
        builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            return new MongoClient("");
        });
        var app = builder.Build();
        app.UseCors("all");
        // Configure the HTTP request pipeline.
        app.UseExceptionHandler();

        app.MapOpenApi();
        app.MapScalarApiReference(x => {
            x.DarkMode = false;
        });
        app.MapControllers();
        app.Run();
    }
}
