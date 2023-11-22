using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using WebApiStudy.Extensions;
using WebApiStudy.Logger;

namespace WebApiStudy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(configuration["Logging:LogLevel:Default"].ToLogEventLevel())
                .MinimumLevel.Override("Microsoft", configuration["Logging:LogLevel:Microsoft"].ToLogEventLevel())
                .WriteTo.Console(new LogFormat())
                .WriteTo.MongoDB(
                    database: new MongoClient(configuration["Logging:MongoDB:Connect"])
                        .GetDatabase(configuration["Logging:MongoDB:DatabaseName"]),
                    collectionName: configuration["Logging:MongoDB:CollectionName"],
                    mongoDBJsonFormatter: new LogFormat())
                .WriteTo.File(
                    new LogFormat(),
                    configuration["Logging:File:Path"] + "/.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: int.Parse(configuration["Logging:File:SizeLimitMB"]) * 1024 * 1024,
                    retainedFileCountLimit: null,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
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
            catch(Exception ex)
            {
                Log.Logger.ApiWrite(LogEventLevel.Fatal, $"Host terminated unexpectedly,ex:\n{ex}");

            }
            finally
            {
                Log.CloseAndFlush();

            }
        }
    }
}
