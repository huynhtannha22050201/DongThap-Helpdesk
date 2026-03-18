using DongThapHelpdesk.Api.Configurations;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Repositories;
using DongThapHelpdesk.Api.Services;
using Microsoft.OpenApi;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace DongThapHelpdesk.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── MongoDB Settings ──────────────────────────────────────
            var mongoSettings = builder.Configuration
                .GetSection("MongoDbSettings")
                .Get<MongoDbSettings>()!;

            // ── MongoDB Global Conventions ────────────────────────────
            var conventionPack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("GlobalConventions", conventionPack, _ => true);

            builder.Services.AddSingleton(mongoSettings);
            builder.Services.AddSingleton<MongoDbContext>();

            // ── Repositories ──────────────────────────────────────────
            builder.Services.AddSingleton<TicketRepository>();

            // ── Services ──────────────────────────────────────────────
            builder.Services.AddSingleton<TicketService>();


            // ── Swagger + Controllers ─────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ── Middleware Pipeline ───────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}