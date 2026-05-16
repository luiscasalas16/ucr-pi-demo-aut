global using DemoAutApi.Database;
global using DemoAutApi.Endpoints;
global using DemoAutApi.Models;
global using DemoAutApi.Utils;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;

namespace DemoAutApi;

static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<EmpresaContext>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapReadEndpoints();
        app.MapWriteEndpoints();

        app.Run();
    }
}
