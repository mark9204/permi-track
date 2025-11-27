using Microsoft.EntityFrameworkCore;
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


namespace PermiTrack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext Configuration with Factory for Audit Service
            var connectionString = builder.Configuration.GetConnectionString("PermiTrackContextDocker");
            
            builder.Services.AddDbContext<PermiTrackDbContext>(options => 
                options.UseSqlServer(connectionString));

            // Add DbContext Factory for Audit Service (to avoid DbContext lifetime conflicts)
            builder.Services.AddDbContextFactory<PermiTrackDbContext>(options =>
                options.UseSqlServer(connectionString));

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
            
            // Register Audit Service (Scoped for proper DI)
            builder.Services.AddScoped<IAuditService, AuditService>();

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
            builder.Services.AddControllers();

            // Frontend CORS policy - allow requests from frontend
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            
            // Swagger/OpenAPI configuration for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            // Authentication and Authorization MUST come before Audit Logging
            // so that user context is available
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
