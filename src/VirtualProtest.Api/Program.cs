
using Serilog;
using VirtualProtest.Core.Interfaces;
using VirtualProtest.Services;

namespace VirtualProtest.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console() // Or any other sinks
                .CreateLogger();

            builder.Host.UseSerilog(); // Use Serilog for logging

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMemoryCache(); // Add memory cache services
            builder.Services.AddScoped<IProtestService, ProtestService>(); // Add ProtestService
            builder.Services.AddSingleton<WebSocketManagerService>(); // Add WebSocketManagerService

            var app = builder.Build();

            // Enable WebSockets
            app.UseWebSockets();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
