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
using PermiTrack.BackgroundJobs;


namespace PermiTrack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext Configuration with Factory for Audit Service
            var connectionString = builder.Configuration.GetConnectionString("PermiTrackContextDocker");
            

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
