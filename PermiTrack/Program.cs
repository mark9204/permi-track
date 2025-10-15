using Microsoft.EntityFrameworkCore;
using PermiTrack.DataContext;
using AutoMapper;
using PermiTrack.DataContext.Mappings;
using Microsoft.Extensions.DependencyInjection;


namespace PermiTrack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<PermiTrackDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PermiTrackDb")));

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<PermiTrack.DataContext.Mappings.PermiTrackProfile>();
            });


        

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
    }
}
