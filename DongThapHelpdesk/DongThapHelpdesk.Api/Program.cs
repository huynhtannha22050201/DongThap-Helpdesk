using DongThapHelpdesk.Api.Configurations;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Repositories;
using DongThapHelpdesk.Api.Services;
using DongThapHelpdesk.Api.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using System.Text;
using System.Text.Json.Serialization;
using DongThapHelpDesk.Api.Services;
using System.Threading.Tasks;

namespace DongThapHelpdesk.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Settings ──────────────────────────────────────
            var mongoSettings = builder.Configuration
                .GetSection("MongoDbSettings")
                .Get<MongoDbSettings>()!;

            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()!;   

            // ── MongoDB Global Conventions ────────────────────────────
            var conventionPack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("GlobalConventions", conventionPack, _ => true);

            builder.Services.AddSingleton(mongoSettings);
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddSingleton<MongoDbContext>();

            // ── Repositories ──────────────────────────────────────────
            builder.Services.AddSingleton<TicketRepository>();
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<PointHistoryRepository>();

            // ── Services ──────────────────────────────────────────────
            builder.Services.AddSingleton<TicketService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<JwtTokenService>();
            builder.Services.AddSingleton<PointService>();

            // ── CORS ──────────────────────────────────────────────────
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("VueApp", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // ── JWT Authentication ────────────────────────────────────
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };

                // Đọc Token từ HttpOnly Cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token =
                            context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ── Swagger + Controllers ─────────────────────────────────
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters
                        .Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy
                        = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ── Middleware Pipeline ───────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                var seeder = new DatabaseSeeder(mongoSettings);
                await seeder.ClearAllAsync();
                await seeder.SeedAsync();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("VueApp");
            app.UseAuthentication();    
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}