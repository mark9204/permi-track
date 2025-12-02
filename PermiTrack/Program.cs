using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PermiTrack.DataContext;
using AutoMapper;
using PermiTrack.DataContext.Mappings;
using Microsoft.Extensions.DependencyInjection;
using PermiTrack.Services.Interfaces;
using PermiTrack.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using PermiTrack.Authorization;
using PermiTrack.Extensions;
using PermiTrack.BackgroundJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace PermiTrack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext Configuration with Factory for Audit Service
            var connectionString = builder.Configuration.GetConnectionString("PermiTrackContextDocker");

            // If running in Development, prevent a single BackgroundService failure from stopping the host.
            // This makes debugging database/migration issues easier while developing.
            if (builder.Environment.IsDevelopment())
            {
                builder.Host.ConfigureServices((context, services) =>
                {
                    services.Configure<HostOptions>(o =>
                        o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
                });
            }

            // Add DbContext Factory for Audit Service (to avoid DbContext lifetime conflicts)
            // Register with a lambda that creates options inline to avoid singleton/scoped conflict
            builder.Services.AddDbContextFactory<PermiTrackDbContext>((serviceProvider, options) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var connString = config.GetConnectionString("PermiTrackContextDocker");
                options.UseSqlServer(connString);
            });

            // AutoMapper Configuration
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<PermiTrack.DataContext.Mappings.PermiTrackProfile>();
            });

            // Register Services
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            
            // SPEC 7: Register Security Service for Login Tracking
            builder.Services.AddScoped<ISecurityService, SecurityService>();
            
            // Register Audit Service (Scoped for proper DI)
            builder.Services.AddScoped<IAuditService, AuditService>();

            // SPEC 12: Register Role Expiration Background Job (Automatic Role Expiration)
            builder.Services.AddHostedService<RoleExpirationJob>();

            // JWT Authentication Configuration
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Authorization Configuration with Permission-Based Policies
            builder.Services.AddAuthorization();
            
            // Register Permission Authorization Handler
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            
            // Register Custom Policy Provider as Singleton
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Add controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // 1. Circular reference handling
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                    // 2. Serialize enums as strings (e.g., "Pending" instead of 0)
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // CORS Configuration - FIXED to allow both localhost:5173 and 5174
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5173",  // Vite default port
                            "http://localhost:5174",  // Alternate Vite port
                            "https://localhost:5173", // HTTPS variants (if needed)
                            "https://localhost:5174"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Required for JWT tokens in cookies/headers
                });
            });

            // Swagger/OpenAPI configuration for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PermiTrack API", Version = "v1" });
            
                // Define the BearerAuth security scheme
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            var app = builder.Build();

            // Apply pending EF Core migrations at startup (non-fatal; logged). Uses the registered DbContextFactory.
            try
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PermiTrackDbContext>>();
                try
                {
                    using var db = factory.CreateDbContext();
                    db.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully at startup.");
                }
                catch (Exception ex)
                {
                    // Log but do not rethrow – in Development host will remain up due to HostOptions configuration above.
                    logger.LogError(ex, "Failed to apply database migrations at startup.");
                }
            }
            catch
            {
                // Swallow any scope/DI resolution failures here to avoid hard crash during startup logging.
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Ensure UI points to the generated JSON endpoint
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PermiTrack API v1");
                });
            }

            app.UseHttpsRedirection();

            // CRITICAL: CORS must be placed BEFORE Authentication/Authorization
            app.UseCors();

            // Authentication and Authorization MUST come after CORS
            // but BEFORE Audit Logging so that user context is available
            app.UseAuthentication();
            app.UseAuthorization();

            // Add Audit Logging Middleware AFTER authentication/authorization
            // but BEFORE endpoint mapping to capture user information
            app.UseAuditLogging();

            app.MapControllers();

            app.Run();
        }
    }
}
